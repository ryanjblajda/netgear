using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using System.ComponentModel;
using Netgear.Communications;
using Netgear.Resources;
using Netgear.Events;

namespace Netgear
{
    public class NetgearSwitch : INotifyPropertyChanged
    {
        private NetgearDeviceClient Client;

        internal List<NetgearSwitchInterface> Interfaces;

        private ushort _id;
        public ushort ID
        {
            get { return this._id; }
            set
            {
                this._id = value;
                this.DebugMessage("ID: {0}", this.ID);
                if (this.ID != 0)
                {
                    bool registered = NetgearDevices.RegisterDevice(this);
                    this.DebugMessage(registered == true ? "Successfully Registered" : "Registration Failure");
                    //fire notification to simpl
                    if (this.Registered != null) this.Registered(this, new DigitalAnalogPayloadEventArgs(registered));
                }
            }
        }

        public bool IsDebug { get; private set; }

        private string symbolName;
        public string SymbolName
        {
            get { return this.symbolName; }
            set
            {
                this.symbolName = value;
                if (this.Client != null) this.Client.SymbolName = this.symbolName;
            }
        }

        private string host;
        public string Host
        {
            get { return this.host; }
            set
            {
                this.host = value;
                if (this.Client != null) this.Client.Host = this.Host;
            }
        }

        private string username;
        public string Username
        {
            get { return this.username; }
            set
            {
                this.username = value;
                if (this.Client != null) this.Client.Username = this.Username;
            }
        }

        private string password;
        public string Password
        {
            get { return this.password; }
            set
            {
                this.password = value;
                if (this.Client != null) this.Client.Password = this.Password;
            }
        }

        public ushort PollTime
        {
            get { return this.Client.PollTime; }
            set 
            {
                if (this.Client != null) { this.Client.PollTime = value; }
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            private set 
            {
                if (this.name != value)
                {
                    this.name = value;
                    this.Notify(Strings.PropertyName);
                    if (this.NameChanged != null) this.NameChanged(this, new StringPayloadEventArgs(this.Name));
                }
            }
        }

        private string location;
        public string Location
        {
            get { return this.location; }
            private set
            {
                if (this.location != value)
                {
                    this.location = value;
                    this.Notify(Strings.PropertyLocation);
                    if (this.LocationChanged != null) this.LocationChanged(this, new StringPayloadEventArgs(this.Location));
                }
            }
        }

        private string serialNumber;
        public string SerialNumber
        {
            get { return this.serialNumber; }
            private set
            {
                if (this.serialNumber != value)
                {
                    this.serialNumber = value;
                    this.Notify(Strings.PropertySerialNumber);
                    if (this.SerialNumberChanged != null) this.NameChanged(this, new StringPayloadEventArgs(this.Name));
                }
            }
        }

        private string mac;
        public string MAC
        {
            get { return this.mac; }
            private set
            {
                if (this.mac != value)
                {
                    this.mac = value;
                    this.Notify(Strings.PropertyMACAddress);
                    if (this.MACAddressChanged != null) this.MACAddressChanged(this, new StringPayloadEventArgs(this.MAC));
                }
            }
        }

        private string model;
        public string ModelNumber
        {
            get { return this.model; }
            private set
            {
                if (this.model != value)
                {
                    this.model = value;
                    this.Notify(Strings.PropertyModelNumber);
                    if (this.ModelNumberChanged != null) this.ModelNumberChanged(this, new StringPayloadEventArgs(this.ModelNumber));
                }
            }
        }

        private string swVersionNumber;
        public string SoftwareVersionNumber
        {
            get { return this.swVersionNumber; }
            private set
            {
                if (this.swVersionNumber != value)
                {
                    this.swVersionNumber = value;
                    this.Notify(Strings.PropertySoftwareVersionNumber);
                    if (this.SoftwareVersionNumberChanged != null) this.SoftwareVersionNumberChanged(this, new StringPayloadEventArgs(this.SoftwareVersionNumber));
                }
            }
        }

        private string lastReboot;
        public string LastReboot
        {
            get { return this.lastReboot; }
            private set
            {
                if (this.lastReboot != value)
                {
                    this.lastReboot = value;
                    this.Notify(Strings.PropertyLastReboot);
                    if (this.LastRebootChanged != null) this.LastRebootChanged(this, new StringPayloadEventArgs(this.LastReboot));
                }
            }
        }

        private int portCount;
        public int PortCount
        {
            get { return this.portCount; }
            private set
            {
                if (this.portCount != value)
                {
                    this.portCount = value;
                    this.Notify(Strings.PropertyPortCount);
                    //if (this.Client != null) { this.Client.GetAllPortDetails(this.PortCount); }
                    if (this.PortCountChanged != null) this.PortCountChanged(this, new DigitalAnalogPayloadEventArgs(this.PortCount));
                }
            }
        }

        private int activePorts;
        public int ActivePorts
        {
            get { return activePorts; }
            private set
            {
                if (this.activePorts != value)
                {
                    this.DebugMessage("Active Ports Currently: {0} // Previously: {1}", this.activePorts, value);
                    this.activePorts = value;
                    this.Notify(Strings.PropertyActivePortCount);
                    
                    //if (this.Client != null) { this.Client.GetAllPortDetails(this.PortCount); }
                    if (this.ActivePortsChanged != null) this.ActivePortsChanged(this, new DigitalAnalogPayloadEventArgs(this.ActivePorts));
                }
            }
        }

        private ushort rtsp;
        public ushort RTSPState
        {
            get { return rtsp; }
            private set
            {
                if (this.rtsp != value)
                {
                    this.rtsp = value;
                    this.Notify(Strings.PropertyRTSPState);
                    if (this.RTSPStateChanged != null) this.RTSPStateChanged(this, new DigitalAnalogPayloadEventArgs(this.RTSPState));
                }
            }
        }

        private string memoryInUse;
        public string MemoryInUse
        {
            get { return this.memoryInUse; }
            private set
            {
                if (this.memoryInUse != value)
                {
                    this.memoryInUse = value;
                    this.Notify(Strings.PropertyMemoryInUse);
                    if (this.MemoryInUseChanged != null) this.MemoryInUseChanged(this, new StringPayloadEventArgs(this.MemoryInUse));
                }
            }
        }

        private string memoryUsage;
        public string MemoryUsage
        {
            get { return this.memoryUsage; }
            private set
            {
                if (this.memoryUsage != value)
                {
                    this.memoryUsage = value;
                    this.Notify(Strings.PropertyMemoryUsage);
                    if (this.MemoryUsageChanged != null) this.MemoryUsageChanged(this, new StringPayloadEventArgs(this.MemoryUsage));
                }
            }
        }

        private string cpuUsage;
        public string CPUUsage
        {
            get { return this.cpuUsage; }
            private set
            {
                if (this.cpuUsage != value)
                {
                    this.cpuUsage = value;
                    this.Notify(Strings.PropertyCPUUsage);
                    if (this.CPUUsageChanged != null) this.CPUUsageChanged(this, new StringPayloadEventArgs(this.CPUUsage));
                }
            }
        }

        private List<Dictionary<string, string>> fanState;
        public List<Dictionary<string, string>> FanState
        {
            get { return this.fanState; }
            private set
            {
                if (this.fanState != value)
                {
                    this.fanState = value;
                    this.Notify(Strings.PropertyFanState);
                    this.FanStateDetails = this.GenerateFanStateDetailsString(this.FanState);
                }
            }
        }

        private string fanStateDetails;
        public string FanStateDetails
        {
            get { return this.fanStateDetails; }
            private set
            {
                if (this.fanStateDetails != value)
                {
                    this.fanStateDetails = value;
                    this.Notify(Strings.PropertyFanStateDetails);
                    if (this.FanStateChanged != null) this.FanStateChanged(this, new StringPayloadEventArgs(this.FanStateDetails));
                }
            }
        }

        public ushort poe;
        public ushort POEState
        {
            get { return this.poe; }
            private set
            {
                if (this.poe != value)
                {
                    this.poe = value;
                    this.Notify(Strings.PropertyPOEState);
                    if (this.POEStateChanged != null) this.POEStateChanged(this, new DigitalAnalogPayloadEventArgs(this.POEState));
                }
            }
        }

        private string uptime;
        public string Uptime
        {
            get { return this.uptime; }
            private set
            {
                if (this.uptime != value)
                {
                    this.uptime = value;
                    this.Notify(Strings.PropertyUptime);
                    if (this.UptimeChanged != null) this.UptimeChanged(this, new StringPayloadEventArgs(this.Uptime));
                }
            }
        }

        private List<TemperatureSensors> temp;
        public List<TemperatureSensors> Temperature
        {
            get { return this.temp; }
            private set
            {
                if (this.temp != value)
                {
                    this.temp = value;
                    this.Notify(Strings.PropertyTemperatureSensors);
                    this.TemperatureDetails = this.GenerateTemperatureDetailsString(this.Temperature);
                }
            }
        }
        
        private string tempDetails;
        public string TemperatureDetails 
        {
            get { return this.tempDetails; }
            private set 
            {
                if (this.tempDetails != value)
                {
                    this.tempDetails = value;
                    this.Notify(Strings.PropertyTemperatureSensorsDetails);
                    if (this.TemperatureDetailsChanged != null) this.TemperatureDetailsChanged(this, new StringPayloadEventArgs(this.TemperatureDetails));
                }
            }
        }

        private string bootVer;
        public string BootVersion
        {
            get { return this.bootVer; }
            private set
            {
                if (this.bootVer != value)
                {
                    this.bootVer = value;
                    this.Notify(Strings.PropertyBootVersion);
                    if (this.BootVersionChanged != null) this.BootVersionChanged(this, new StringPayloadEventArgs(this.BootVersion));
                }
            }
        }

        private string rxBytes;
        public string ReceivedDataBytes
        {
            get { return this.rxBytes; }
            private set
            {
                if (this.rxBytes != value)
                {
                    this.rxBytes = value;
                    this.Notify(Strings.PropertyRxBytes);
                    if (this.ReceivedBytesChanged != null) this.ReceivedBytesChanged(this, new StringPayloadEventArgs(this.ReceivedDataBytes));
                }
            }
        }

        private string txBytes;
        public string TransmittedDataBytes
        {
            get { return this.txBytes; }
            private set
            {
                if (this.txBytes != value)
                {
                    this.txBytes = value;
                    this.Notify(Strings.PropertyTxBytes);
                    if (this.TransmittedBytesChanged != null) this.TransmittedBytesChanged(this, new StringPayloadEventArgs(this.TransmittedDataBytes));
                }
            }
        }

        private string adminPoe;
        public string AdministratorPOEPowerWattage
        {
            get { return this.adminPoe; }
            private set
            {
                if (this.adminPoe != value)
                {
                    this.adminPoe = value;
                    this.Notify(Strings.PropertyAdminPOEWattage);
                    if (this.AdministratorPoEWattageChanged != null) this.AdministratorPoEWattageChanged(this, new StringPayloadEventArgs(this.AdministratorPOEPowerWattage));
                }
            }
        }

        private int connectionStatus;

        public event PropertyChangedEventHandler PropertyChanged;
        
        public event DigitalAnalogPayload LoggedIn;
        public event DigitalAnalogPayload Connected;
        public event DigitalAnalogPayload ConnectionStatus;
        public event DigitalAnalogPayload Registered;

        public event DigitalAnalogPayload ActivePortsChanged;
        public event StringPayload AdministratorPoEWattageChanged;
        public event StringPayload BootVersionChanged;
        public event StringPayload CPUUsageChanged;
        public event StringPayload FanStateChanged;
        public event StringPayload LastRebootChanged;
        public event StringPayload LocationChanged;
        public event StringPayload MACAddressChanged;
        public event StringPayload ModelNumberChanged;
        public event StringPayload MemoryInUseChanged;
        public event StringPayload MemoryUsageChanged;
        public event StringPayload NameChanged;
        public event DigitalAnalogPayload POEStateChanged;
        public event DigitalAnalogPayload PortCountChanged;
        public event DigitalAnalogPayload RTSPStateChanged;
        public event StringPayload ReceivedBytesChanged;
        public event StringPayload SoftwareVersionNumberChanged;
        public event StringPayload SerialNumberChanged;
        public event StringPayload TemperatureDetailsChanged;
        public event StringPayload TransmittedBytesChanged;
        public event StringPayload UptimeChanged;

        internal event DigitalAnalogPayload Debug;

        /// <summary>
        /// the default constructor for a netgear switch device
        /// </summary>
        public NetgearSwitch()
        {
            this.Client = new NetgearDeviceClient();
            this.SymbolName = "Symbol Name Not Set";

            this.Client.Connected += this.OnConnected;
            this.Client.ConnectionStatus += this.OnConnectionStatus;
            this.Client.LoggedIn += this.OnLogin;
            this.Client.DeviceInfo += this.OnDeviceInfo;
            this.Client.DeviceName += this.OnDeviceName;
            this.Client.DeviceForwardingDatabase += this.OnDeviceForwardingDatabase;
            this.Client.DevicePortStatistics += this.OnDevicePortStatistics;
            this.Client.DevicePortConfiguration += this.OnDevicePortConfiguration;
            this.Client.DevicePortPOEConfiguration += this.OnDevicePortPOEConfiguration;

            this.Interfaces = new List<NetgearSwitchInterface>();
        }

        private string GenerateTemperatureDetailsString(List<TemperatureSensors> temp)
        {
            StringBuilder result = new StringBuilder();
            result.Append("Temperature Details:\r\n\t");
            temp.ForEach(delegate(TemperatureSensors sensor)
            {
                result.Append(String.Format("Temp Sensor: {0} -- {1}\r\n\tState: {2} -- {3}\r\n\t", sensor.SensorNumber, sensor.SensorDescription, sensor.SensorState, sensor.SensorTemperature));
            });
            this.DebugMessage(result.ToString());
            return result.ToString();
        }

        private string GenerateFanStateDetailsString(List<Dictionary<string, string>> fans)
        {
            StringBuilder result = new StringBuilder();

            result.Append("Fan State:\r\n\t");

            fans.ForEach(delegate(Dictionary<string, string> entry) {
                foreach(KeyValuePair<string, string> fan in entry) { 
                    result.Append(String.Format("{0} : {1}\r\n\t", fan.Key, fan.Value)); 
                }
            });
            this.DebugMessage(result.ToString());
            return result.ToString();
        }

        private void OnLogin(object sender, DigitalAnalogPayloadEventArgs args)
        {
            //this.DebugMessage("Received Login Event {0}", args.Payload);
            if (this.LoggedIn != null) { this.LoggedIn(this, args); }
            
            //get the initial state of all our ports we care about upon successful login
            if (Conversion.ConvertToBool(args.Payload) == true) 
            {
                if (this.Interfaces.Count != 0)
                {
                    this.Interfaces.ForEach(item => this.GetPortDetails(item.Port));
                }
            }
        }

        private void OnConnectionStatus(object sender, DigitalAnalogPayloadEventArgs args)
        {
            if (this.connectionStatus != args.Payload)
            {
                this.connectionStatus = args.Payload;
                if (this.ConnectionStatus != null) { this.ConnectionStatus(this, args); }
            }
        }

        private void OnConnected(object sender, DigitalAnalogPayloadEventArgs args)
        {
            if (this.Connected != null) { this.Connected(this, args); }
        }

        private void OnDeviceName(object sender, DeviceNamePayloadEventArgs args)
        {
            this.Name = args.Payload.Name;
            this.Location = args.Payload.Location;
        }

        private void OnDeviceInfo(object sender, DeviceInfoPayloadEventArgs args)
        {
            this.ActivePorts = args.Payload.ActivePorts;
            this.AdministratorPOEPowerWattage = args.Payload.AdministratorPOEWattage;
            this.BootVersion = args.Payload.BootVersion;
            this.CPUUsage = args.Payload.CPUUsage;
            this.FanState = args.Payload.FanState;
            this.LastReboot = args.Payload.LastReboot;
            this.MAC = args.Payload.MAC;
            this.MemoryInUse = args.Payload.MemoryInUse;
            this.MemoryUsage = args.Payload.MemoryUsage;
            this.ModelNumber = args.Payload.ModelNumber;
            this.POEState = args.Payload.POEState;
            this.PortCount = args.Payload.PortCount;
            this.ReceivedDataBytes = args.Payload.ReceivedDataBytes;
            this.RTSPState = args.Payload.RTSPState;
            this.TransmittedDataBytes = args.Payload.TransmittedDataBytes;
            this.SerialNumber = args.Payload.SerialNumber;
            this.SoftwareVersionNumber = args.Payload.SoftwareVersionNumber;
            this.Temperature = args.Payload.Temperature;
            this.Uptime = args.Payload.Uptime;
        }

        private void OnDeviceForwardingDatabase(object sender, DeviceForwardingDatabasePayloadEventArgs args)
        {
            NetgearSwitchInterface switchInterface = FindMatchingInterface(args.Interface);

            if (switchInterface != null)
            {
                lock (switchInterface)
                {
                    switchInterface.UpdateForwardingDatabaseEntries(args.Entries);
                }
                if (this.Client != null) this.Client.GetPortDetails(switchInterface.Port);
            }
        }

        private void OnDevicePortConfiguration(object sender, DevicePortConfigurationPayloadEventArgs args)
        {
            NetgearSwitchInterface switchInterface = FindMatchingInterface(args.Payload.Port);

            if (switchInterface != null)
            {
                lock (switchInterface)
                {
                    switchInterface.UpdatePortConfiguration(args.Payload);
                }
            }
        }

        private void OnDevicePortPOEConfiguration(object sender, DevicePortPOEConfigurationPayloadEventArgs args)
        {
            NetgearSwitchInterface switchInterface = FindMatchingInterface(args.Payload.Port);

            if (switchInterface != null)
            {
                lock (switchInterface)
                {
                    switchInterface.UpdatePortPOEConfiguration(args.Payload);
                }
            }
        }

        private void OnDevicePortStatistics(object sender, DevicePortStatisticsPayloadEventArgs args)
        {
            NetgearSwitchInterface switchInterface = FindMatchingInterface(args.Payload.Port);

            if (switchInterface != null)
            {
                lock (switchInterface)
                {
                    switchInterface.UpdatePortStatistics(args.Payload);
                }
            }
        }

        private NetgearSwitchInterface FindMatchingInterface(ushort port)
        {
            NetgearSwitchInterface result = null;

            try
            {
                lock (this.Interfaces)
                {

                    this.DebugMessage("Searching For Interface @ {0}", port);
                    result = this.Interfaces.Find(item => item.Port == port);

                    if (result != null) { this.DebugMessage("Found Interface @ {0}", port); }
                    else { this.DebugMessage("No Interface @ {0}", port); }
                }
            }
            catch (Exception e) { this.DebugMessage("Exception Encountered Within the Interfaces Lock {0}", e.Message); }
                
           return result;
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
            CrestronConsole.PrintLine(String.Format("Netgear Switch @ {0}: {1}", this.SymbolName, msg), objects);
            Console.WriteLine(String.Format("Netgear Switch @ {0}: {1}", this.SymbolName, msg), objects);
        }

        /// <summary>
        /// handles firing the property changed event and checking if the subscribers are null first
        /// </summary>
        /// <param name="name">the property that changed</param>
        private void Notify(string name)
        {
            if (this.PropertyChanged != null) { this.PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        }

        /// <summary>
        /// allows SIMPL to enable debug printing
        /// </summary>
        /// <param name="debug">a ushot representing whether or not we should print debug messages</param>
        public void EnableDebug(ushort debug)
        {
            this.IsDebug = Conversion.ConvertToBool(debug);
            this.Client.IsDebug = this.IsDebug;
            if (this.Debug != null) this.Debug(this, new DigitalAnalogPayloadEventArgs(this.IsDebug));
        }

        internal void GetPortDetails(ushort port)
        {
            if (this.Client != null) this.Client.GetPortDetails(port);
        }

        internal bool RegisterSwitchInterface(NetgearSwitchInterface newInterface)
        {
            bool success = false;

            lock (this.Interfaces)
            {
                NetgearSwitchInterface result = this.Interfaces.Find(child => child.Port == newInterface.Port);

                if (result == null)
                {
                    this.Interfaces.Add(newInterface);
                    success = true;
                    this.DebugMessage("Switch Interface @ Port {0} Added", newInterface.Port);
                }
                else { this.DebugMessage("Switch Interface @ Port {0} Already Exists", newInterface.Port); }

                this.DebugMessage("Register Switch Interface Result: {0}", success == true ? "Success" : "Failure");
            }

            return success;
        }

        public void Login()
        {
            this.Client.Login();
        }

        public void Logout()
        {
            this.Client.Logout();
        }

        public void PortEnablePOE(NetgearSwitchInterface switchInterface, bool poeEnable)
        {
            if (this.Client != null)
            {
                this.DebugMessage("Attempt To {0} on Port: {1}", poeEnable == true ? "Enable" : "Disable", switchInterface.Port);
                this.Client.PortEnablePOE(switchInterface, poeEnable);
                this.Client.GetPortDetails(switchInterface.Port);
            }
            else { this.DebugMessage("No Valid Client To Create Enable POE Request"); }
        }

        public void PortResetPOE(NetgearSwitchInterface switchInterface)
        {
            if (this.Client != null)
            {
                this.DebugMessage("Attempt To Reset POE on Port: {0}", switchInterface.Port);
                this.Client.PortResetPOE(switchInterface);
                this.Client.GetPortDetails(switchInterface.Port);
            }
            else { this.DebugMessage("No Valid Client To Create Enable POE Request"); }
        }

        public void EnablePoll(ushort enable)
        {
            if (this.Client != null) this.Client.EnablePoll(Conversion.ConvertToBool(enable));
        }
    }
}