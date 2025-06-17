using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Netgear.Resources;
using Netgear.Communications;

namespace Netgear.Communications
{
    /// <summary>
    /// the payload received from a netgear device when a login request is made successfully
    /// </summary>
    public class LoginResponsePayload
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("expire")]
        public long Expiration { get; private set; }

        public LoginResponsePayload()
        {
            this.Expiration = 0;
            this.Token = String.Empty;
        }

        public LoginResponsePayload(string token, long expires) : this()
        {
            this.Token = token;
            this.Expiration = expires;
        }
    }

    /// <summary>
    /// the payload that a netgear device must receive when attempting to log in so data can be retrieved
    /// </summary>
    public class LoginRequestPayload
    {
        [JsonProperty("login")]
        public LoginRequestCredentialsPayload Credentials;

        public LoginRequestPayload(string user, string pass)
        {
            this.Credentials = new LoginRequestCredentialsPayload(user, pass);
        }
    }

    /// <summary>
    /// a payload used to login to the device
    /// </summary>
    public class LoginRequestCredentialsPayload
    {
        [JsonProperty("username")]
        public string Username {get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }

        public LoginRequestCredentialsPayload(string user, string pass)
        {
            this.Username = user;
            this.Password = pass;
        }
    }

    /// <summary>
    /// this payload will contain device name and location information
    /// </summary>
    public class DeviceNameResponsePayload
    {
        [JsonProperty("name")]
        public string Name { get; private set; }     
        
        [JsonProperty("location")]
        public string Location { get; private set; }

        public DeviceNameResponsePayload()
        {
            this.Name = String.Empty;
            this.Location = String.Empty;
        }

        public DeviceNameResponsePayload(string name, string location)
        {
            this.Name = name;
            this.Location = Location;
        }
    }

    /// <summary>
    /// this payload contains port configuration details
    /// </summary>
    public class DevicePortConfigurationRequestResponsePayload
    {
        [JsonProperty("ID")]
        public ushort Port { get; private set; }

        [JsonProperty("description")]
        public string Description { get; private set; }

        [JsonProperty("portType")]
        public PortMode Type { get; private set; }

        [JsonProperty("adminMode")]
        public ushort AdministratorMode { get; private set; }

        [JsonProperty("portSpeed")]
        public PortLinkSpeed Speed { get; private set; }

        [JsonProperty("duplexMode")]
        public DuplexMode Duplex { get; private set; }

        [JsonProperty("linkStatus")]
        public PortStatus Status { get; private set; }

        [JsonProperty("linkTrap")]
        public ushort Trap { get; private set; }

        [JsonProperty("maxFrameSize")]
        public ushort MaxFrameSize { get; private set; }

        [JsonProperty("isPoE")]
        public bool POECapable { get; set; }

        [JsonProperty("txRate")]
        public ushort TransmitRate { get; private set; }

        [JsonProperty("rtlimitUcast")]
        public RateLimit RateLimitUnicast { get; private set; }
        
        [JsonProperty("rtlimitMcast")]
        public RateLimit RateLimitMulticast { get; private set; }

        [JsonProperty("rtlimitBcast")]
        public RateLimit RateLimitBroadcast { get; private set; }

        [JsonProperty("portVlanId")]
        public ushort VLAN { get; private set; }

        [JsonProperty("defVlanPrio")]
        public ushort DefaultVLANPriority { get; private set; }

        [JsonProperty("scheduleName")]
        public string ScheduleName { get; private set; }

        public DevicePortConfigurationRequestResponsePayload()
        {
            this.RateLimitBroadcast = new RateLimit();
            this.RateLimitMulticast = new RateLimit();
            this.RateLimitUnicast = new RateLimit();
            this.ScheduleName = String.Empty;
        }

        public DevicePortConfigurationRequestResponsePayload(NetgearSwitchInterface port)
        {
            this.AdministratorMode = port.AdministratorMode;
            this.DefaultVLANPriority = port.DefaultVLANPriority;
            this.Description = port.Description;
            this.Duplex = port.Duplex;
            this.MaxFrameSize = port.MaxFrameSize;
            this.POECapable = Conversion.ConvertToBool(port.POEEnabled);
            this.Port = port.Port;
            this.RateLimitBroadcast = port.RateLimitBroadcast;
            this.RateLimitMulticast = port.RateLimitMulticast;
            this.RateLimitUnicast = port.RateLimitUnicast;
            this.ScheduleName = port.ScheduleName;
            this.Speed = port.LinkSpeed;
            this.TransmitRate = port.TransmitRate;
            this.Trap = port.LinkTrap;
            this.Type = port.PortMode;
            this.VLAN = port.NativeVLAN;
        }
    }

    /// <summary>
    /// this payload contains port statistics for a specific port
    /// </summary>
    public class DevicePortStatisticsResponsePayload
    {
        [JsonProperty("portId")]
        public ushort Port { get; private set; }
        
        [JsonProperty("name")]
        public string Name { get; private set; }
        
        [JsonProperty("myDesc")]
        public string Description { get; private set; }
        
        [JsonProperty("adminMode")]
        public ushort AdministratorMode { get; private set; }
        
        [JsonProperty("status")]
        public PortStatus Status { get; private set; }
        
        [JsonProperty("poeStatus")]
        public PortPOEStatus PowerOverEthernetStatus { get; private set; }
        
        [JsonProperty("mode")]
        public PortMode PortMode { get; private set; }

        [JsonProperty("vlans")]
        public List<ushort> VLANMembership { get; private set; }

        [JsonProperty("trafficRx")]
        public int TransmittedDataBytes { get; private set; }

        [JsonProperty("trafficTx")]
        public int ReceivedDataBytes { get; private set; }
        
        [JsonProperty("rxMbps")]
        public string ReceiveSpeed { get; private set; }

        [JsonProperty("txMbps")]
        public string TransmitSpeed { get; private set; }

        [JsonProperty("crcErrorsRx")]
        public int ErrorsReceived { get; private set; }

        [JsonProperty("errorsRxTx")]
        public int ErrorPacketsReceived { get; private set; }

        [JsonProperty("dropsRxTx")]
        public int DroppedPackets { get; private set; }

        [JsonProperty("portMacAddress")]
        public string MAC { get; private set; }

        [JsonProperty("speed")]
        public PortInterfaceSpeed Speed { get; private set; }

        [JsonProperty("duplex")]
        public DuplexMode Duplex { get; private set; }

        [JsonProperty("frameSize")]
        public int FrameSize { get; private set; }

        [JsonProperty("flowControl")]
        public ushort FlowControl { get; private set; }

        [JsonProperty("lacpMode")]
        public ushort LACP { get; private set; }
        
        [JsonProperty("mirrored")]
        public ushort Mirrored { get; private set; }

        [JsonProperty("stpStatus")]
        public ushort STP { get; private set; }

        [JsonProperty("portState")]
        public PortSTPState STPStatus { get; private set; }

        [JsonProperty("oprState")]
        public PortOperationalStatus OperationalStatus { get; private set; }

        [JsonProperty("powerLimitClass")]
        public PortPOEPowerClass POEClass { get; private set; }

        [JsonProperty("portAuthState")]
        public PortAuthorizationStatus Authorization { get; private set; }

        [JsonProperty("neighborInfo")]
        public NeighborInfo Neighbor { get; private set; }

        public DevicePortStatisticsResponsePayload()
        {
            this.VLANMembership = new List<ushort>();
            this.Neighbor = new NeighborInfo();
        }
    }

    /// <summary>
    /// this payload contains the device info provided by a netgear device
    /// </summary>
    public class DeviceInfoResponsePayload
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        
        [JsonProperty("serialNumber")]
        public string SerialNumber { get; private set; }
        
        [JsonProperty("macAddr")]
        public string MAC { get; private set; }
        
        [JsonProperty("model")]
        public string ModelNumber { get; private set; }
        
        [JsonProperty("lanIpAddress")]
        public string IPAddress { get; private set; }
        
        [JsonProperty("swVer")]
        public string SoftwareVersionNumber { get; private set; }
        
        [JsonProperty("lastReboot")]
        public string LastReboot { get; private set; }
        
        [JsonProperty("numOfPorts")]
        public int PortCount { get; private set; }
        
        [JsonProperty("numOfActivePorts")]
        public int ActivePorts { get; private set; }
        
        [JsonProperty("rstpState")]
        public ushort RTSPState { get; private set; }
        
        [JsonProperty("memoryUsed")]
        public string MemoryInUse { get; private set; }
        
        [JsonProperty("memoryUsage")]
        public string MemoryUsage { get; private set; }
        
        [JsonProperty("cpuUsage")]
        public string CPUUsage { get; private set; }
        
        [JsonProperty("fanState")]
        public List<Dictionary<string, string>> FanState { get; private set; }
        
        [JsonProperty("poeState")]
        public ushort POEState { get; private set; }
        
        [JsonProperty("upTime")]
        public string Uptime { get; private set; }
        
        [JsonProperty("temperatureSensors")]
        public List<TemperatureSensors> Temperature { get; private set; }
        
        [JsonProperty("bootVersion")]
        public string BootVersion { get; private set; }
        
        [JsonProperty("rxData")]
        public long rxData { get; private set; }

        public string ReceivedDataBytes
        {
            get { return String.Format("{0}", this.rxData); }
        }
        
        [JsonProperty("txData")]
        public long txData { get; private set; }

        public string TransmittedDataBytes
        {
            get { return String.Format("{0}", this.txData); }
        }

        [JsonProperty("adminPoePower")]
        public long adminPoeWattage { get; private set; }

        public string AdministratorPOEWattage
        {
            get { return String.Format("{0}", this.adminPoeWattage); }
        }

        public DeviceInfoResponsePayload()
        {
            this.Temperature = new List<TemperatureSensors>();
            this.FanState = new List<Dictionary<string, string>>();
        }

        public DeviceInfoResponsePayload(string serial, string mac, string model, string ip, string swVer, string lastReboot, int ports, int activePorts, bool rtsp, string memoryInUse, string memoryUsage, string cpu, List<Dictionary<string, string>> fans, bool poeEnabled, string uptime, List<TemperatureSensors> temp, string bootVer, long rx, long tx, long poeWattage) : this()
        {
            this.SerialNumber = serial;
            this.MAC = mac;
            this.ModelNumber = model;
            this.IPAddress = ip;
            this.SoftwareVersionNumber = swVer;
            this.LastReboot = lastReboot;
            this.PortCount = ports;
            this.ActivePorts = activePorts;
            this.RTSPState = Conversion.ConvertToUshort(rtsp);
            this.MemoryInUse = memoryInUse;
            this.MemoryUsage = memoryUsage;
            this.CPUUsage = cpu;
            this.FanState = fans;
            this.POEState = Conversion.ConvertToUshort(poeEnabled);
            this.Uptime = uptime;
            this.Temperature = temp;
            this.BootVersion = bootVer;
            this.txData = tx;
            this.rxData = rx;
            this.adminPoeWattage = poeWattage;
        }

        public DeviceInfoResponsePayload(string name, string serial, string mac, string model, string ip, string swVer, string lastReboot, int ports, int activePorts, bool rtsp, string memoryInUse, string memoryUsage, string cpu, List<Dictionary<string, string>> fans, bool poeEnabled, string uptime, List<TemperatureSensors> temp, string bootVer, long rx, long tx, long poeWattage) : this()
        {
            this.Name = name;
            this.SerialNumber = serial;
            this.MAC = mac;
            this.ModelNumber = model;
            this.IPAddress = ip;
            this.SoftwareVersionNumber = swVer;
            this.LastReboot = lastReboot;
            this.PortCount = ports;
            this.ActivePorts = activePorts;
            this.RTSPState = Conversion.ConvertToUshort(rtsp);
            this.MemoryInUse = memoryInUse;
            this.MemoryUsage = memoryUsage;
            this.CPUUsage = cpu;
            this.FanState = fans;
            this.POEState = Conversion.ConvertToUshort(poeEnabled);
            this.Uptime = uptime;
            this.Temperature = temp;
            this.BootVersion = bootVer;
            this.txData = tx;
            this.rxData = rx;
            this.adminPoeWattage = poeWattage;
        }
    }

    public class DevicePortPOEConfigurationRequestResponsePayload
    {
        [JsonProperty("portid")]
        public ushort Port { get; private set; }

        [JsonProperty("enable")]
        public bool Enable { get; set; }

        [JsonProperty("powerLimitMode")]
        public PortPOEPowerLimit PowerLimit { get; private set; }

        [JsonProperty("classification")]
        public PortPOEPowerClass Class { get; private set; }

        [JsonProperty("powerLimit")]
        public ushort PowerLimitMilliwatts { get; private set; }

        [JsonProperty("status")]
        public PortPOEStatus Status { get; private set; }

        [JsonProperty("currentPower")]
        public ushort CurrentPowerMilliwatts { get; private set; }

        [JsonProperty("reset")]
        public bool Reset { get; set; }

        public DevicePortPOEConfigurationRequestResponsePayload()
        {
            this.PowerLimitMilliwatts = 3000;
        }

        public DevicePortPOEConfigurationRequestResponsePayload(NetgearSwitchInterface switchInterface)
        {
            this.Port = switchInterface.Port;
            this.Enable = Conversion.ConvertToBool(switchInterface.POEEnabled);
            this.PowerLimit = switchInterface.POEPowerLimit;
            this.PowerLimitMilliwatts = switchInterface.POEPowerLimitMilliwatts;
            this.Class = switchInterface.POEClass;
        }
    }
}