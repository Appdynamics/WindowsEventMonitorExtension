using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Handlers;
using AppDynamics.Extension.SDK.Model.Enumeration;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

namespace AppD.NETExtensions
{
    class WindowsEventMonitor : AExtensionBase
    {

        #region private props

        private EventLogWatcher _eventLogWatcher = null;

        private string _logName = "";

        private List<string> _allowedSources = null;

        private List<string> _allowedEventType = null;

        private List<string> _filters = null;

        private List<string> _allowedEventIds = null;

        //TODO: Make this optionally readable from config file
        private readonly string _summaryFormat = "Type:{0},%0A Source:{1},%0A EventID:{2},%0A Instance ID:{3},%0A Machine:{4},%0A Message:{5}";

        #endregion

        #region Public methods
        public override void Initialize()
        {
            // optional- using current class as logger
            logger = NLog.LogManager.GetLogger(ExtensionName);

            // get event log name from parametrs
            var value = Parameters["EventLogPath"];

            // Set event log name if configured otherwise set a default
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentNullException("Event Log name can not be empty.");

            _logName = value.Trim();

            // Set sources if configured otherwise set a empty
            SetPropertyFromConfig("EventSources", "", out _allowedSources);

            // set filter if configured
            SetPropertyFromConfig("EventLogMessageContains", "", out _filters);

            //set event ID if configured, default empty to make sure it never matches
            SetPropertyFromConfig("EventID", "", out _allowedEventIds);

            //set error level
            SetPropertyFromConfig("EventLogEntryType", "", out _allowedEventType);            
        }

        public override void Stop()
        {
            if (_eventLogWatcher != null)
            {
                _eventLogWatcher.Enabled = false;

                _eventLogWatcher.Dispose();
            }
        }

        public override bool Execute()
        {
            EventLogQuery query = new EventLogQuery(_logName, PathType.LogName);
            // Not passing provider info to read all events for the configured event log

            _eventLogWatcher = new EventLogWatcher(query);

            _eventLogWatcher.EventRecordWritten += ProcessEventRecordWritten;

            _eventLogWatcher.Enabled = true;

            logger.Info($"Matching on EventSources={string.Join(",", _allowedSources)} and EventLogMessageContains={string.Join(",", _filters)} and EventId={string.Join(",", _allowedEventIds)} and EventLogEntryType={string.Join(",", _allowedEventType)}");
            logger.Info($"Created Event Log watch for {_logName}");

            return true;
        }

        #endregion

        #region Private methods
        private void SetPropertyFromConfig(string propertyNameinConfig, string defaultStringValue, out List<string> _liststr)
        {
            string value = "";

            Parameters.TryGetValue(propertyNameinConfig, out value);

            _liststr = new List<string>();

            if (string.IsNullOrEmpty(value))
            {
                // adding default value
                _liststr.Add(defaultStringValue.ToLowerInvariant());
            }
            else
            {
                value = value.ToLowerInvariant();

                _liststr.AddRange(value.Split(','));
            }

        }

        private void ProcessEventRecordWritten(object sender, EventRecordWrittenEventArgs e)
        {
            try
            {

                if (e.EventException != null)
                    throw e.EventException;

                else if (IsValidEvent(e.EventRecord))
                {
                    ProcessEventRecorrd(e.EventRecord);
                }
                else
                {
                    if (logger.IsTraceEnabled)
                        logger.Trace(
                            $"Not posting Event Log for Log={_logName}, Source={e.EventRecord.ProviderName}, Id={e.EventRecord.Id}, Type={e.EventRecord.LevelDisplayName}");

                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Error while processing event record for log={_logName}");
            }
        }

        private void ProcessEventRecorrd(EventRecord e)
        {

            string message = e.FormatDescription();

            string summary = string.Format(_summaryFormat,
                e.LevelDisplayName,
                e.ProviderName,
                e.Id,
                e.RecordId,
                e.MachineName,
                message);

            summary = ReplaceSplChars(summary);

            AddProperties(e);

            string comment = "Event originated from the Windows event log and provided by the .Net extension service. To filter this event use CustomEventType = " + ExtensionName;

            ControllerEventArgs args = new ControllerEventArgs(
                summary,
                comment,
                GetSeverity(e.LevelDisplayName),
                ExtensionName,
                EventProperties);

            if (logger.IsTraceEnabled)
                logger.Trace("posting Event Log for Log={0} => {1}", _logName, summary);

            PostEventToController(args);
        }

        private bool IsValidEvent(EventRecord entry)
        {
            string message = string.Empty;
            bool isValid = false;
            bool validType = ContainsItemInList(entry.LevelDisplayName.ToLowerInvariant(), _allowedEventType);
            bool validSource = ContainsItemInList(entry.ProviderName.ToLowerInvariant(), _allowedSources);
            bool validEventId = ContainsItemInList(entry.Id.ToString(), _allowedEventIds);

            if (validType && validSource && validEventId)
            {
                if (_filters.All(x => x == string.Empty))
                    isValid = true;
                else if (entry.Properties[0]?.Value != null)
                {
                    message = entry.Properties[0].Value.ToString();

                    foreach (string filter in _filters)
                    {
                        // Comparing this way make it culture insensitive
                        if (message.ToLowerInvariant().Contains(filter.ToLowerInvariant()))
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
            }

            if (!isValid && logger.IsTraceEnabled)
                logger.Trace($"Failed to match-- validType:{validType}|validSource:{validSource}|validEventId:{validEventId}|messageFilters:{_filters.Any(message.Contains)}");
            
            return isValid;
        }

        private void AddProperties(EventRecord e)
        {
            if (EventProperties == null)
            {
                logger.Warn("Needed to initialize event properties in extension");
                EventProperties = new Dictionary<string, string>();
            }

            _add(EventProperties, "eventid", e.Id.ToString());

            _add(EventProperties, "eventsource", e.ProviderName);

            _add(EventProperties, "machinename", e.MachineName);

            _add(EventProperties, "severity", e.LevelDisplayName);
        }

        private void _add(IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        /// <summary>
        /// returning true if list is empty or contains item with same value
        /// </summary>
        private bool ContainsItemInList(string strValue, List<string> listValues)
        {
            // Need to compare with empty string to make sure in case of default it passes. 
            return
                listValues == null ||
                listValues.Count == 0 ||
                listValues.Contains("") ||
                listValues.Contains(strValue);
        }

        private ControllerEventSeverity GetSeverity(string level)
        {
            ControllerEventSeverity severity = ControllerEventSeverity.INFO;

            switch (level.ToLowerInvariant())
            {
                case "critical":
                    severity = ControllerEventSeverity.ERROR;
                    break;
                case "error":
                    severity = ControllerEventSeverity.ERROR;
                    break;
                case "warning":
                    severity = ControllerEventSeverity.WARN;
                    break;
                default:
                    severity = ControllerEventSeverity.INFO;
                    break;
            }
            return severity;
        }

        private string ReplaceSplChars(string s)
        {
            s = s.Replace("&", " and ");

            return s;
        }

        #endregion

    }

}