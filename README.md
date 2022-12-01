<h1>Windows Event Log Monitor</h1>

<p>
  <span>Windows Event Log Monitor works with .NET Agent Extension Manager to capture and report specific windows events.</span>
</p>

<h2><span>Contents</span></h2>
<p>Windows Event Log monitor require extension.xml and WindowsEventMonitor.dll file to be placed in a new folder under <span style="font-weight:600">&lt;Extension Manager Root Directory&gt;/Extensions</span></p>

<h2><span>Prerequisites</span></h2>
<ul style="list-style:disc;padding-left:20px;"><li>.NET Agent Extension Manager </li><li><span>The AppDynamics .NET Agent</span></li><li><span>.NET 4.0 or later</span></li></ul>

<h2><span>Installation</span></h2>
<p>
  <span>Download, unzip and copy extension.xml and WindowsEventMonitor.dll in a new folder under Extensions directory of .Net Extension Manager. Please refer to the next section for detailed steps.</span>
</p>

<h2><span>Getting Started</span></h2>
<ol start="1"><li><span>Create a new folder under Extension Manager root directory. We can name it according to type of events we want to capture like IISEventMonitor or simply WindowsEventMonitor</span></li></ol>
<ol start="2"><li><span>Copy extension.xml to this folder.</span></li></ol>
<ol start="3"><li><span>Edit extension.xml to provide controller details and target specific events.</span><ul style="list-style:disc;padding-left:20px;padding-top:10px;"><li><span>Change the name of extension if required. (It will take the folder name if we leave it blank.)</span><pre>&lt;extension type="Event" name="WindowsEventLogMonitor" enabled="true"&gt;
</pre></li><li><span>Provide controller user credentials. These are required for publishing custom events to the controller. For single-tenant controllers, use "customer1" for the account. Controller host/port are read from the Agent Configuration file (config.xml).</span><pre>&lt;controller-info user="username" account="customer1" password="password" /&gt;
</pre></li><li><span>Associate event to any business transaction, node or tier. This is optional and if we leave these commented then the event will be associated with application only.</span><pre>  &lt;controller-event-properties&gt;
    &lt;add key="bt" value="/MyBT.aspx"&gt;&lt;/add&gt;
    &lt;add key="node" value="MyNodeName"&gt;&lt;/add&gt;
    &lt;add key="tier" value="MyTier"&gt;&lt;/add&gt;
  &lt;/controller-event-properties&gt;
</pre><span><span style="text-decoration:underline;">NOTE</span>: The node, BT, and Tier names must match an existing Tier/Node/BT exactly, or an error will be generated and event will not be registered. Also valid property combination are : Only Tier, BT+Tier, Node+Tier, BT+Node+Tier.</span></li><li><span>Specify filters to target any event(s) using following parameters:</span><ul style="list-style:square;padding-left:20px;padding-top:5px;"><li><span>EventLogPath: Possible values are Application or System or Setup, etc . It is required and we can not remove this parameter.</span></li><li><span>EventSources: We can provide any event source. Multiple values can be provided as comma separated strings. If we leave it empty it will report all event sources.</span></li><li><span>EventID: We can provide any event Id. Multiple values can be provided as comma separated strings. If we leave it empty it will report all event id.</span></li><li><span>EventLogEntryType: Possible values are Error, Information or Warning. If we leave it empty it will report all event types.</span></li><li><span>EventLogMessageContains: We can provide any strings to be matched in event message. Multiple values can be provided as comma separated strings. This parameter can be left empty or commented.</span></li></ul></li></ul></li></ol>

<ol start="4"><li><span>Save the file and launch Extension Manager UI. We should be able to see WindowsEventMonitor extension listed under "List of extensions loaded"</span><img src="https://www.appdynamics.com/media/uploaded-files/1465370544/windowseventlogmonitor.png"></li></ol>

<ol start="5"><li><span>Start Extension Service, if it was stopped or Restart for the changes to take effect.</span><ul><li><span>We will see new custom events sent to the controller. We should be able to view link to Custom Events on Application Dashboard. For more details https://docs.appdynamics.com/21.10/en/appdynamics-essentials/monitor-events.</span></li><img src="https://www.appdynamics.com/media/uploaded-files/1465370545/customevent.png"></ul></li></ol>

<h2><span>Troubleshooting</span></h2>
<p>
  <span>If you're not seeing events reported to the controller, check to make sure the controller credentials are correct and try removing node/tier or BT mapping if added. If this doesn't help, check the Logs folder for any errors.</span>
</p>

<h2><span>Upgrade</span></h2>
<ol start="1">
  <li><span>Upgrade to latest version of extension manager.</span></li>
  <li><span>Copy new extension.xml and make appropriate changes.</span></li>
  <li><span>Start Extension Service to use latest version.</span></li>
</ol>

<h2><span>Release Notes</span></h2>
<div>
  <div>2.0.0</div><ol start="1" style="padding-left:25px;">
  <li>
  <span>Support for non-classic event sources.</span>
  </li>
  <li>
  <span>Make event source filter optional to capture all events.</span>
  </li>
  </ol>
</div>
<h2><span>Notice and Disclaimer</span></h2>
<p><span>All Extensions published by AppDynamics are governed by the Apache License v2 and are excluded from the definition of covered software under any agreement between AppDynamics and the User governing AppDynamics Pro Edition, Test &amp; Dev Edition, or any other Editions.</span></p> 
