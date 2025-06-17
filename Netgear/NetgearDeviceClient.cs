using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.Net.Https;
using Crestron.SimplSharp.CrestronIO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Netgear.Resources;
using Netgear.Events;
using Netgear.Communications;
using Newtonsoft.Json;

namespace Netgear
{
    internal class NetgearDeviceClient
    {
        private string username;
        public string Username
        {
            get { return this.username; }
            set
            {
                if (this.username != value)
                {
                    this.username = value;
                    this.DebugMessage("Username: {0}", this.username);
                }
            }
        }

        private string password;
        public string Password
        {
            get { return this.password; }
            set
            {
                if (this.password != value)
                {
                    this.password = value;
                    this.DebugMessage("Password: {0}", this.password);
                }
            }
        }

        private string host;
        public string Host
        {
            get { return this.host; }
            set
            {
                if (this.host != value)
                {
                    this.host = value;
                    this.DebugMessage("Host: {0}", this.host);
                }
            }
        }

        public string SymbolName;

        private int PortCount;

        private Dictionary<ushort, List<ForwardingDatabaseEntry>> fdb = new Dictionary<ushort, List<ForwardingDatabaseEntry>>();
        private Dictionary<ushort, List<ForwardingDatabaseEntry>> forwardingDatabase
        {
            get { return this.fdb; }
            set
            {
                if (this.fdb != null && value != null)
                {
                    //if we intersect the lists, and the count
                    List<ushort> differences = this.fdb.Keys.Except(value.Keys).ToList();
                    this.DebugMessage("Current Fowarding Database Ports: {0}\r\nNew Forwarding Database Ports: {1}\r\nDifferences: {2}",
                        String.Join(", ", this.fdb.Keys.Select(entry => entry.ToString()).ToArray()), 
                        String.Join(", ", value.Keys.Select(entry => entry.ToString()).ToArray()),
                        differences.Count);

                    //if (differences.Count != 0) { this.GetAllPortDetails(this.PortCount); }
                }

                //make sure not to assign null values
                if (value != null)
                {
                    //only assign if not the same
                    if (this.fdb != value) { this.fdb = value; }
                }
            }
        }

        private string LoginToken;
        private DateTime LoginTokenExpirationTime;

        public event DigitalAnalogPayload Connected;
        public event DigitalAnalogPayload ConnectionStatus;
        public event DigitalAnalogPayload LoggedIn;
        public event DeviceInfoPayload DeviceInfo;
        public event DeviceFowardingDatabasePayload DeviceForwardingDatabase;
        public event DeviceNamePayload DeviceName;
        public event DevicePortStatisticsPayload DevicePortStatistics;
        public event DevicePortConfigurationPayload DevicePortConfiguration;
        public event DevicePortPOEConfigurationPayload DevicePortPOEConfiguration;

        private bool isLogin;
        public bool IsLoggedIn
        {
            get { return this.isLogin; }
            private set 
            {
                //only fire the event and store the new value if it is different from our current value
                if (this.isLogin != value) 
                { 
                    //first, store the new value
                    this.isLogin = value;
                    this.DebugMessage("{0}", this.isLogin == true ? "Logged In" : "Logged Out");
                    if (this.isLogin)
                    {
                        this.GetDeviceInfo();
                        this.GetDeviceName();
                        this.GetForwardingDatabase();
                    }
                    else
                    {
                        //reset properties so that things are refreshed next login
                        this.PortCount = 0;
                        lock (this.forwardingDatabase) { this.forwardingDatabase = new Dictionary<ushort, List<ForwardingDatabaseEntry>>(); }
                    }
                    //do a null check, then fire the event
                    if (this.LoggedIn != null) { this.LoggedIn(this, new DigitalAnalogPayloadEventArgs(this.isLogin)); }
                }
            }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get { return this.isConnected; }
            private set
            {
                //only fire the event and store the new value if it is different
                if (this.isConnected != value)
                {
                    //store the value
                    this.isConnected = value;
                    //notify console
                    this.DebugMessage("{0}", this.IsConnected == true ? "Connected" : "Disconnected");
                    //if we arent connected we cannot be logged in
                    if (this.isConnected == false) this.IsLoggedIn = false;
                    //do a null check, then fire the event
                    if (this.Connected != null) { this.Connected(this, new DigitalAnalogPayloadEventArgs(this.isConnected)); }
                }
            }
        }

        private BindingList<Request> Queue;

        internal bool IsDebug;

        private CTimer PollTimer;
        private CTimer LoginTokenRenewalTimer;

        private bool pollEnabled;

        private ushort pollTime;
        public ushort PollTime
        {
            get { return (ushort)(this.pollTime * 10); }
            set
            {
                if (this.pollTime != value)
                {
                    this.pollTime = value;
                    if (this.pollEnabled) { this.PollTimer.Reset(this.PollTime, this.PollTime); }
                }
            }
        }

        /// <summary>
        /// default constructor for netgear device
        /// </summary>
        public NetgearDeviceClient()
        {
            this.forwardingDatabase = new Dictionary<ushort, List<ForwardingDatabaseEntry>>();
            this.SymbolName = "Not Set";
            //this.IsDebug = true;
            this.LoginTokenExpirationTime = new DateTime();

            this.Username = String.Empty;
            this.Password = String.Empty;
            this.Host = String.Empty;
            
            this.Queue = new BindingList<Request>();
            this.Queue.ListChanged += this.OnQueueChanged;
            this.PollTimer = new CTimer(this.OnPollTimerExpired, Timeout.Infinite);
            this.LoginTokenRenewalTimer = new CTimer(this.OnPollTimerExpired, Timeout.Infinite);
        }

        /// <summary>
        /// print the message if debug mode is enabled
        /// </summary>
        /// <param name="msg">a format string</param>
        /// <param name="objects">the array of objects to print inside the format string</param>
        private void DebugMessage(string msg, params object[] objects)
        {
            if (this.IsDebug) { this.Message(msg, objects); }
        }

        /// <summary>
        /// prints a message to the console
        /// </summary>
        /// <param name="msg">a format string</param>
        /// <param name="objects">the array of objects to print inside the format string</param>
        private void Message(string msg, params object[] objects)
        {
            CrestronConsole.PrintLine(String.Format("Netgear Device Client @ {0}: {1}", this.SymbolName, msg), objects);
            Console.WriteLine(String.Format("Netgear Device Client @ {0}: {1}", this.SymbolName, msg), objects); 
        }

        /// <summary>
        /// generate a url for the https request
        /// </summary>
        /// <param name="url">the api path to send the request to</param>
        /// <returns></returns>
        private UrlParser GenerateURL(string url)
        {
            UrlParser result = new UrlParser();
            string fullUrl = String.Format("https://{0}:{1}{2}{3}", this.Host, Numbers.Port, Strings.DeviceBaseURL, url);
            try {
                result.Parse(fullUrl);
            }
            catch (Exception e) {
                if (this.IsDebug) { this.DebugMessage(e.Message); }
            }

            //this.DebugMessage("Generated URL: {0}", fullUrl);
            return result;
        }

        /// <summary>
        /// check if the configuration is okay, based on the host, username and password
        /// </summary>
        /// <returns>a boolean representing if the config is good or bad</returns>
        private bool ConfigurationOkay()
        {
            bool result = false;

            if (this.Host != String.Empty && this.Username != String.Empty && this.Password != String.Empty)
            {
                result = true;
            }
            //this.DebugMessage("Configuration {0}", result == true ? "Valid" : "Invalid");
            return result;
        }
        
        /// <summary>
        /// called when the poll timer expires, so that we can keep to to date on the device's status
        /// </summary>
        /// <param name="sender">the timer object who sent the update</param>
        private void OnPollTimerExpired(object sender)
        {
            if (this.pollEnabled)
            {
                if (this.ConfigurationOkay())
                {
                    if (this.IsLoggedIn)
                    {
                        this.GetDeviceInfo();
                        this.GetForwardingDatabase();
                        this.GetDeviceName();
                    }
                    else { this.Login(); }
                }
                else { this.PollTimer.Reset(Timeout.Infinite); }
            }
        }

        /// <summary>
        /// an event handler that will be fired whenever the queue changes, allowing us to pop an item if required and send the command to the device
        /// </summary>
        /// <param name="sender">the list who changed</param>
        /// <param name="args">the list changed args to determine the change</param>
        private void OnQueueChanged(object sender, ListChangedEventArgs args)
        {
            switch (args.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    if (this.IsConnected && this.IsLoggedIn && this.ConfigurationOkay()) { this.SendRequest(this.Dequeue()); }
                    break;
                case ListChangedType.ItemDeleted:
                    break;
            }
        }

        /// <summary>
        /// adds an item to the end of the queue
        /// </summary>
        /// <param name="request">the custom request object to add to the queue</param>
        private void Enqueue(Request request)
        {
            lock (this.Queue)
            {
                if (!this.Queue.Contains(request)) { this.Queue.Add(request); }
                else { this.DebugMessage("Queue Already Contains {0} Request", request.Type); }
            }
        }

        /// <summary>
        /// retrieves the first object from the queue, creating a monitorable FIFO queue
        /// </summary>
        /// <returns>a custom request object</returns>
        private Request Dequeue()
        {
            lock (this.Queue)
            {
                if (this.Queue.Count != 0) { 
                    Request request = this.Queue.First();
                    bool result = this.Queue.Remove(request);
                    if (result == false) { this.DebugMessage("Error Removing Request {0}", request.Type); }
                    return request;
                }
                else { return null; }
            }
        }

        /// <summary>
        /// handle any http response, and any encompassing errors
        /// </summary>
        /// <param name="obj">the http response object</param>
        /// <param name="err">the callback error</param>
        /// <param name="userobj">the user object accompanying the callback, so we can determine what response was captured</param>
        private void OnHttpsResponse(HttpsClientResponse obj, HTTPS_CALLBACK_ERROR err, object userobj)
        {
            try
            {
                if (obj != null)
                {
                    if (err != HTTPS_CALLBACK_ERROR.INVALID_PARAM && err != HTTPS_CALLBACK_ERROR.UNKNOWN_ERROR && obj.Code == Numbers.SuccessCode)
                    {
                        this.UpdateConnection(obj.Code, err);
                        this.HandleSuccessfulResponse(obj, userobj);
                        this.SendRequest(this.Dequeue());
                    }
                    else
                    {
                        this.UpdateConnection(obj.Code, err);
                        this.HandleFailedResponse(obj.Code);
                    }
                }
                else {
                    this.HandleFailedResponse(-1);
                    this.UpdateConnection(-1, err);
                }
            }
            catch (Exception e)
            {
                this.DebugMessage("Error Parsing Https Response: {0} {1}", e.Message, e.StackTrace);
            }
        }

        private void HandleFailedResponse(int code)
        {
            this.DebugMessage("Https Callback Failed: {0}", code);
        }

        /// <summary>
        /// determines how a successful response should be handled
        /// </summary>
        /// <param name="response">the http response</param>
        /// <param name="obj">the user object we sent along to determine which kind of response was received</param>
        private void HandleSuccessfulResponse(HttpsClientResponse response, object obj)
        {
            //this.DebugMessage("{0}", response.ContentString);
            this.DebugMessage("Successful {0} Response", (string)obj);
            switch ((string)obj)
            {
                case Strings.DeviceInfo:
                    this.HandleDeviceInfoResponse(response);
                    break;
                case Strings.DeviceForwardingDatabase:
                    this.HandleDeviceForwardingDatabaseResponse(response);
                    break;
                case Strings.DeviceLogin:
                    this.HandleLoginResponse(response);
                    break;
                case Strings.DeviceName:
                    this.HandleDeviceNameResponse(response);
                    break;
                case Strings.DevicePortStatistics:
                    this.HandleDevicePortStatistics(response);
                    break;
                case Strings.DeviceLogout:
                    this.HandleDeviceLogout(response);
                    break;
                case Strings.DevicePortConfig:
                    this.HandleDevicePortConfiguration(response);
                    break;
                case Strings.DevicePortPOEConfig:
                    this.HandleDevicePortPOEConfiguration(response);
                    break;
            }
        }

        private void HandleDeviceLogout(HttpsClientResponse response)
        {
            this.DebugMessage("Stopping Poll Timer");

            if (response.Code == Numbers.SuccessCode)
            {
                Response logoutResponse = JsonConvert.DeserializeObject<Response>(response.ContentString);
                if (logoutResponse != null)
                {
                    if (logoutResponse.Details != null)
                    {
                        if (logoutResponse.Details.ResponseCode != 0) { this.DebugMessage("Response Details {0} // {1} // {2}", logoutResponse.Details.ResponseCode, logoutResponse.Details.ResponseMessage, logoutResponse.Details.Status); }
                    }
                }
            }

            this.IsLoggedIn = false;
        }

        /// <summary>
        /// handles the login response json, and then assigns the necessary variables as needed
        /// </summary>
        /// <param name="response">the https response</param>
        private void HandleLoginResponse(HttpsClientResponse response)
        {
            LoginResponse loginResponse = JsonConvert.DeserializeObject<LoginResponse>(response.ContentString);

            if (loginResponse != null)
            {
                this.LoginToken = loginResponse.Payload.Token;
                DateTime now = DateTime.Now;
                DateTime expiration = now.AddSeconds(loginResponse.Payload.Expiration);
                this.LoginTokenExpirationTime = expiration;
                //this.DebugMessage("{0}", response.ContentString);
                this.DebugMessage("Now: {0} // Login Expiration Time: {1} // Expiration: {2}s", now, this.LoginTokenExpirationTime, loginResponse.Payload.Expiration);
                this.DebugMessage("Login Token: {0}", this.LoginToken);
                this.LoginTokenRenewalTimer.Reset(loginResponse.Payload.Expiration - 1);
                this.IsLoggedIn = true;
            }
            else { this.DebugMessage("LoginResponse Null!"); }
        }

        /// <summary>
        /// handles a device name response from the device
        /// </summary>
        /// <param name="response">the https response received</param>
        private void HandleDeviceNameResponse(HttpsClientResponse response)
        {
            try
            {
                DeviceNameResponse deviceName = JsonConvert.DeserializeObject<DeviceNameResponse>(response.ContentString);

                if (deviceName != null)
                {
                    if (deviceName.Payload != null)
                    {
                        this.DebugMessage("Device Name: {0}, Device Location: {1}", deviceName.Payload.Name, deviceName.Payload.Location);
                        if (this.DeviceName != null) { this.DeviceName(this, new DeviceNamePayloadEventArgs(deviceName.Payload)); }
                    }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Handling Device Name Response: {0} {1}", e.Message, e.StackTrace); }
        }

        /// <summary>
        /// handles a device info response from the device
        /// </summary>
        /// <param name="response">the https device info response received</param>
        private void HandleDeviceInfoResponse(HttpsClientResponse response)
        {
            try
            {
                DeviceInfoResponse deviceInfo = JsonConvert.DeserializeObject<DeviceInfoResponse>(response.ContentString);

                if (deviceInfo != null)
                {
                    if (deviceInfo.Payload != null)
                    {
                        this.DebugMessage("Device Info: {0}", String.Format("{0} // {1} // {2}", deviceInfo.Payload.IPAddress, deviceInfo.Payload.MAC, deviceInfo.Payload.ModelNumber));
                        if (this.PortCount != deviceInfo.Payload.PortCount)
                        {
                            this.PortCount = deviceInfo.Payload.PortCount;
                            //this.GetAllPortDetails(this.PortCount);
                        }
                        if (this.DeviceInfo != null) { this.DeviceInfo(this, new DeviceInfoPayloadEventArgs(deviceInfo.Payload)); }
                    }
                    else { this.DebugMessage("Device Info Payload Null!"); }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Handling Device Info Response: {0} {1}", e.Message, e.StackTrace); }
        }

        /// <summary>
        /// handles what should occur when a forwarding database response is received
        /// </summary>
        /// <param name="response">the https response received from the device</param>
        private void HandleDeviceForwardingDatabaseResponse(HttpsClientResponse response)
        {
            try
            {
                ForwardingDatabaseResponse databaseResponse = JsonConvert.DeserializeObject<ForwardingDatabaseResponse>(response.ContentString);

                if (databaseResponse != null)
                {
                    Dictionary<ushort, List<ForwardingDatabaseEntry>> entries = this.CollateForwardingDatabase(databaseResponse.Payload);
                    this.DebugMessage("Forwarding Database Entries: {0}", entries.Keys.Count);
                    //if there are no subscribers to the events dont bother firing any 
                    if (this.DeviceForwardingDatabase != null)
                    {
                        //fire an event for each interface so that any potential subscribers are notified of updates
                        foreach (KeyValuePair<ushort, List<ForwardingDatabaseEntry>> entry in entries)
                        {
                            this.DebugMessage("Device Interface {0} Members: {1}", entry.Key, String.Join(",", entry.Value.Select(en => en.MAC).ToArray()));
                            
                            //only get ports if they are less than the port count, other ports are not phsyical ports
                            //if (entry.Key <= this.PortCount) { this.GetPortDetails(entry.Key); }
                            //fire the event
                            if (this.DeviceForwardingDatabase != null) this.DeviceForwardingDatabase(this, new DeviceForwardingDatabasePayloadEventArgs(entry.Key, entry.Value));
                        }

                    }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Handling Forwarding Database Response: {0} {1}", e.Message, e.StackTrace); }
        }
        
        /// <summary>
        /// handles a device port configuration response from the device
        /// </summary>
        /// <param name="response">the https response</param>
        private void HandleDevicePortConfiguration(HttpsClientResponse response)
        {
            try
            {
                //this.DebugMessage("{0}", response.ContentString);
                PortConfigurationResponse portConfigurationResponse = JsonConvert.DeserializeObject<PortConfigurationResponse>(response.ContentString);

                if (portConfigurationResponse != null)
                {
                    if (portConfigurationResponse.Payload != null)
                    {
                        if (portConfigurationResponse.Payload.Port != 0)
                        {
                            this.DebugMessage("Port Configuration Overview {0} {1} | {2}", portConfigurationResponse.Payload.Port, portConfigurationResponse.Payload.Description, portConfigurationResponse.Payload.Status);
                            if (this.DevicePortConfiguration != null) { this.DevicePortConfiguration(this, new DevicePortConfigurationPayloadEventArgs(portConfigurationResponse.Payload)); }
                        }
                    }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Handling Port Configuration Response: {0} {1}", e.Message, e.StackTrace); }
        }

        private void HandleDevicePortPOEConfiguration(HttpsClientResponse response)
        {
            try
            {
                
                //this.DebugMessage("{0}", response.ContentString);
                PortPOEConfigurationResponse portConfigurationResponse = JsonConvert.DeserializeObject<PortPOEConfigurationResponse>(response.ContentString);

                if (portConfigurationResponse != null)
                {
                    if (portConfigurationResponse.Payload != null)
                    {
                        if (portConfigurationResponse.Payload.Port != 0)
                        {
                            this.DebugMessage("Port POE Configuration Overview {0} | POE {1} : {2}", portConfigurationResponse.Payload.Port, portConfigurationResponse.Payload.Enable == true ? "Enabled" : "Disabled", portConfigurationResponse.Payload.Status);
                            if (this.DevicePortPOEConfiguration != null) { this.DevicePortPOEConfiguration(this, new DevicePortPOEConfigurationPayloadEventArgs(portConfigurationResponse.Payload)); }
                        }
                    }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Handling Port Configuration Response: {0} {1}", e.Message, e.StackTrace); }
        }

        /// <summary>
        /// handles a device port statistics response from the device
        /// </summary>
        /// <param name="response">the https response</param>
        private void HandleDevicePortStatistics(HttpsClientResponse response)
        {
            try
            {
                PortStatisticsReponse portStatisticsResponse = JsonConvert.DeserializeObject<PortStatisticsReponse>(response.ContentString);

                if (portStatisticsResponse != null)
                {
                    if (portStatisticsResponse.Payload != null)
                    {
                        if (portStatisticsResponse.Payload.Port != 0)
                        {
                            this.DebugMessage("Port Statistics Overview {0} {1} | {2}", portStatisticsResponse.Payload.Port, portStatisticsResponse.Payload.Description, portStatisticsResponse.Payload.Status);
                            if (this.DevicePortStatistics != null) { this.DevicePortStatistics(this, new DevicePortStatisticsPayloadEventArgs(portStatisticsResponse.Payload)); }
                        }
                    }
                    else { this.DebugMessage("Port Statistics Payload Null\r\n{0}", response.ContentString); }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Handling Port Statistics Response: {0} {1}", e.Message, e.StackTrace); }
        }

        /// <summary>
        /// creates a dictionary with all the current forwarding database entries contained in a list according to each interface, which will be the key within the dictionary
        /// </summary>
        /// <param name="database">the current state of the forwarding database</param>
        /// <returns>a dictionary with the sorted forwarding database</returns>
        private Dictionary<ushort, List<ForwardingDatabaseEntry>> CollateForwardingDatabase(List<ForwardingDatabaseEntry> database)
        {
            Dictionary<ushort, List<ForwardingDatabaseEntry>> result = new Dictionary<ushort, List<ForwardingDatabaseEntry>>();

            database.ForEach(delegate(ForwardingDatabaseEntry entry)
            {
                //make sure the list is not null
                if (!result.ContainsKey((ushort)entry.Interface)) { result[(ushort)entry.Interface] = new List<ForwardingDatabaseEntry>(); }
                //add the entry to the list
                result[(ushort)entry.Interface].Add(entry);
            });

            return result;
        }

        /// <summary>
        /// determines the updates to the connection status that need to be made, and will cause any SIMPL events to be fired
        /// </summary>
        /// <param name="code">the http response code that was recieved</param>
        /// <param name="err">the http callback error code that was received</param>
        private void UpdateConnection(int code, HTTPS_CALLBACK_ERROR err)
        {
            this.DebugMessage("Https Response Code: {0}, Async Callback Result: {1}", code, err);

            if (this.ConnectionStatus != null) { this.ConnectionStatus(this, new DigitalAnalogPayloadEventArgs(code)); }

            if (err != HTTPS_CALLBACK_ERROR.COMPLETED) 
            { 
                this.IsConnected = false;
                this.PortCount = 0;
            }
            else { this.IsConnected = true; }
            
            //if we receive a response code that is not success, assume we logged out
            if (code == Numbers.UnauthorizedCode || code == Numbers.ForbiddenCode) { this.IsLoggedIn = false; }
        }

        /// <summary>
        /// generates a new https client for us to use to send a request
        /// </summary>
        /// <returns>a https client with the proper settings required</returns>
        private HttpsClient GenerateHttpsClient()
        {
            HttpsClient client = new HttpsClient();

            client.PeerVerification = false;
            client.HostVerification = false;

            return client;
        }

        /// <summary>
        /// creates a https request, configuring the default nonsense in a quick function
        /// </summary>
        /// <param name="url">the api url at which to target the request</param>
        /// <param name="type">the request type</param>
        /// <returns>a properly formatted https client request</returns>
        private HttpsClientRequest GenerateHttpsRequest(string url, Crestron.SimplSharp.Net.Https.RequestType type)
        {
            HttpsClientRequest request = new HttpsClientRequest();
            request.Header.AddHeader(new HttpsHeader("Accept", "*/*"));
            request.Header.AddHeader(new HttpsHeader("User-Agent", "rblajda-module"));
            request.Url = this.GenerateURL(url);
            request.Encoding = Encoding.UTF8;
            request.RequestType = type;

            //only add the auth header if the device is logged in.
            if (this.IsLoggedIn) { request.Header.AddHeader(new HttpsHeader(Strings.AuthorizationHeader, String.Format("Bearer {0}", this.LoginToken))); }
           
            return request;
        }

        /// <summary>
        /// actually sends the request to the device
        /// </summary>
        /// <param name="request">the custom request object class</param>
        private void SendRequest(Request request)
        {
            if (request != null)
            {
                HttpsClient client = this.GenerateHttpsClient();
                HttpsClient.DISPATCHASYNC_ERROR err = client.DispatchAsyncEx(request.HttpsRequest, this.OnHttpsResponse, request.Type);
                //this.DebugMessage("Dispatch Err: {0}", err);
            }
            //else { this.Message("Request To Send Was Null!!"); }
        }

        /// <summary>
        /// adds a device name request to the queue
        /// </summary>
        private void GetDeviceName()
        {
            HttpsClientRequest request = this.GenerateHttpsRequest(Strings.DeviceName, Crestron.SimplSharp.Net.Https.RequestType.Get);
            //if not logged in, but the configuration is okay, attempt to login
            if (this.ConfigurationOkay())
            {
                //check if we are logged in, and attempt to do so.
                if (!this.IsLoggedIn) { this.Login(); }
                this.DebugMessage("Get Device Name");
                //only add the request to the queue if the configuration is okay, regardless of the login status
                this.Enqueue(new Request(request, Strings.DeviceName));
            }
        }

        /// <summary>
        /// adds a device info request to the queue
        /// </summary>
        private void GetDeviceInfo()
        {
            if (this.ConfigurationOkay())
            {
                HttpsClientRequest request = this.GenerateHttpsRequest(Strings.DeviceInfo, Crestron.SimplSharp.Net.Https.RequestType.Get);
                //if not logged in, but the configuration is okay, attempt to login
                //check if we are logged in, and attempt to do so.
                if (!this.IsLoggedIn) { this.Login(); }
                this.DebugMessage("Get Device Info");
                //only add the request to the queue if the configuration is okay, regardless of the login status
                this.Enqueue(new Request(request, Strings.DeviceInfo));
            }
        }

        /// <summary>
        /// adds a forwarding database request to the queue
        /// </summary>
        private void GetForwardingDatabase()
        {
            if (this.ConfigurationOkay())
            {
                HttpsClientRequest request = this.GenerateHttpsRequest(Strings.DeviceForwardingDatabase, Crestron.SimplSharp.Net.Https.RequestType.Get);
                //check if we are logged in, and attempt to do so.
                if (!this.IsLoggedIn) { this.Login(); }
                this.DebugMessage("Get Forwarding Database");
                //only add the request to the queue if the configuration is okay, regardless of the login status
                this.Enqueue(new Request(request, Strings.DeviceForwardingDatabase));
            }
        }

        /// <summary>
        /// gets the full details for a port
        /// </summary>
        /// <param name="port"></param>
        internal void GetPortDetails(ushort port)
        {
            this.DebugMessage("Get Port {0} Details", port);
            this.GetPortStatistics(port);
            this.GetPortConfiguration(port);
            this.GetPortPOEConfiguration(port);
        }

        private void GetPortPOEConfiguration(ushort port)
        {
            if (this.ConfigurationOkay())
            {
                string urlWithParams = String.Format(Strings.DevicePortPOEConfig, port);
                HttpsClientRequest request = this.GenerateHttpsRequest(urlWithParams, Crestron.SimplSharp.Net.Https.RequestType.Get);
                //if the configuration is okay, continue
                //if we arent logged in, try to do so
                if (!this.IsLoggedIn) { this.Login(); }
                this.DebugMessage("Get Port POE {0} Configuration", port);
                //only add the request to the queue if the configuration is okay
                this.Enqueue(new Request(request, Strings.DevicePortPOEConfig));
            }
        }

        /// <summary>
        /// adds a request to get port config to the queue, called by GetPortDetails
        /// </summary>
        /// <param name="port"></param>
        private void GetPortConfiguration(ushort port)
        {
            if (this.ConfigurationOkay())
            {
                string urlWithParams = String.Format(Strings.DevicePortConfig, port);
                HttpsClientRequest request = this.GenerateHttpsRequest(urlWithParams, Crestron.SimplSharp.Net.Https.RequestType.Get);
                //if the configuration is okay, continue
                //if we arent logged in, try to do so
                if (!this.IsLoggedIn) { this.Login(); }
                this.DebugMessage("Get Port {0} Configuration", port);
                //only add the request to the queue if the configuration is okay
                this.Enqueue(new Request(request, Strings.DevicePortConfig));
            }
        }

        /// <summary>
        /// adds a request to get port statistics to the queue, called by GetPortDetails
        /// </summary>
        /// <param name="port">the port to get the status of</param>
        private void GetPortStatistics(ushort port)
        {
            if (this.ConfigurationOkay())
            {
                string urlWithParams = String.Format(Strings.DevicePortStatistics, port);
                HttpsClientRequest request = this.GenerateHttpsRequest(urlWithParams, Crestron.SimplSharp.Net.Https.RequestType.Get);
                //if the configuration is okay, continue
                //if we arent logged in, try to do so
                if (!this.IsLoggedIn) { this.Login(); }
                this.DebugMessage("Get Port {0} Statistics", port);
                //only add the request to the queue if the configuration is okay
                this.Enqueue(new Request(request, Strings.DevicePortStatistics));
            }
        }

        /// <summary>
        /// gets the initial status for all ports on a switch
        /// </summary>
        /// <param name="ports">the total port count a switch has</param>
        public void GetAllPortDetails(int ports)
        {
            this.DebugMessage("Getting All Port Information");
            for (int port = 1; port <= ports; port++) { this.GetPortDetails((ushort)port); }
        }

        public void EnablePoll(bool poll)
        {
            this.pollEnabled = poll;

            if (poll) { this.PollTimer.Reset(this.PollTime, this.PollTime); }
            else { this.PollTimer.Stop(); }
                
        }

        /// <summary>
        /// attempts to login to the switch
        /// </summary>
        public void Login()
        {
            //print a message
            this.DebugMessage("Attempt Login: {0} // {1}", this.Username, this.Password);
            //generate request
            HttpsClientRequest request = this.GenerateHttpsRequest(Strings.DeviceLogin, Crestron.SimplSharp.Net.Https.RequestType.Post);
            request.Header.ContentType = "application/json";
            request.ContentString = JsonConvert.SerializeObject(new LoginRequestPayload(this.Username, this.Password));
            //send request
            this.SendRequest(new Request(request, Strings.DeviceLogin));
        }

        /// <summary>
        /// attempts to log out of the device
        /// </summary>
        public void Logout()
        {
            this.DebugMessage("Attempt Logout");
            HttpsClientRequest request = this.GenerateHttpsRequest(Strings.DeviceLogout, Crestron.SimplSharp.Net.Https.RequestType.Post);
            this.SendRequest(new Request(request, Strings.DeviceLogout));
        }

        public void PortEnablePOE(NetgearSwitchInterface port, bool poeEnable)
        {
            if (this.ConfigurationOkay())
            {
                //generate the actual request url
                string urlWithParams = String.Format(Strings.DevicePortPOEConfig, port.Port);
                //generate the request
                HttpsClientRequest request = this.GenerateHttpsRequest(urlWithParams, Crestron.SimplSharp.Net.Https.RequestType.Post);
                this.DebugMessage("Set POE {0} // Port: {1}", poeEnable == true ? "Enable" : "Disable", port.Port);
                //generate a payload based on the current status of the port
                PortPOEConfigurationResponse payload = new PortPOEConfigurationResponse(new DevicePortPOEConfigurationRequestResponsePayload(port));
                //adjust the new poe state at the payload, to prevent events from being fired prior to realtime action
                payload.Payload.Enable = poeEnable;
                string cmd = JsonConvert.SerializeObject(payload);
                //add the serialized command to the request
                this.DebugMessage("{0}", cmd);
                request.ContentString = cmd;
                //add request to the queue
                if (!this.IsLoggedIn) { this.Login(); }
                this.Enqueue(new Request(request, Strings.DevicePortPOEConfig));
            }  
        }

        public void PortResetPOE(NetgearSwitchInterface port)
        {
            if (this.ConfigurationOkay())
            {
                //generate the actual request url
                string urlWithParams = String.Format(Strings.DevicePortPOEConfig, port.Port);
                //generate the request
                HttpsClientRequest request = this.GenerateHttpsRequest(urlWithParams, Crestron.SimplSharp.Net.Https.RequestType.Post);
                this.DebugMessage("Reset POE // Port: {0}", port.Port);
                //generate a payload based on the current status of the port
                PortPOEConfigurationResponse payload = new PortPOEConfigurationResponse(new DevicePortPOEConfigurationRequestResponsePayload(port));
                //adjust the reset state at the payload, to prevent events from being fired prior to realtime action
                payload.Payload.Reset = true;
                string cmd = JsonConvert.SerializeObject(payload);
                //add the serialized command to the request
                this.DebugMessage("{0}", cmd);
                request.ContentString = cmd;
                //add request to the queue
                if (!this.IsLoggedIn) { this.Login(); }
                this.Enqueue(new Request(request, Strings.DevicePortConfig));
            }  
        }
    }
}
