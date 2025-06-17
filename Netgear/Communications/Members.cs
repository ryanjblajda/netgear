using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace Netgear.Communications
{
    /// <summary>
    /// a class representing the temp sensor data that a netgear switch is capable of providing
    /// </summary>
    public class TemperatureSensors
    {
        [JsonProperty("sensorNum")]
        public int SensorNumber;
        [JsonProperty("sensorDesc")]
        public string SensorDescription;
        [JsonProperty("sensorTemp")]
        public string SensorTemperature;
        [JsonProperty("sensorState")]
        public TemperatureSensorState SensorState;
    }

    public class NeighborInfo
    {
        [JsonProperty("name")]
        public string Name { get; private set; }
        
        [JsonProperty("description")]
        public string Description { get; private set; }
        
        [JsonProperty("capabilities")]
        public string Capabilities { get; private set; }
        
        [JsonProperty("chassisId")]
        public string ChassisID { get; private set; }
        
        [JsonProperty("chassisIdSubtype")]
        public ChassisIDSubtype ChassisSubtype { get; private set; }
        
        [JsonProperty("portId")]
        public string PortID { get; private set; }
        
        [JsonProperty("portIdSubtype")]
        public PortIDSubtype PortSubtype { get; private set; }
        
        [JsonProperty("portDescription")]
        public string PortDescription { get; private set; }
        
        [JsonProperty("mgmtIpAddress")]
        public string ManagementAddress { get; private set; }
        
        [JsonProperty("portAuthState")]
        public PortAuthorizationStatus Authorization { get; private set; }

        public override string ToString()
        {
            return String.Format("Neighbor: {0}, {1} - {2}", this.Name, this.Description, this.Capabilities);
        }

        public override bool Equals(object obj)
        {
            bool result = false;

            if (typeof(NeighborInfo) == obj.GetType())
            {
                //CrestronConsole.PrintLine("OBJECT TYPE CORRECT");

                NeighborInfo toCheck = (NeighborInfo)obj;

                if (this.Name == toCheck.Name && this.Description == toCheck.Description)
                {
                    //CrestronConsole.PrintLine("EQUAL");
                    result = true;
                }
                //else { CrestronConsole.PrintLine("NOT EQUAL"); }
            }
            //else { CrestronConsole.PrintLine("NO TYPE INCORRECT"); }

            return result;
        }

        public NeighborInfo()
        {
        }
    }

    public enum TemperatureSensorState : int
    {
        NONE = 0,
        NORMAL = 1,
        WARNING = 2,
        CRITICAL = 3, 
        SHUTDOWN = 4,
        NOT_PRESENT = 5,
        NOT_OPERATIONAL = 6
    }

    public enum PortAuthorizationStatus : int
    {
        AUTHORIZED = 1,
        UNAUTHORIZED  = 2,
        NOT_APPLICABLE = 3
    }

    public enum ChassisIDSubtype : int
    {
        SUBTYPE_INTF_ALIAS = 1,
        SUBTYPE_PORT_COMP = 2,
        SUBTYPE_MAC_ADDR = 3,
        SUBTYPE_NET_ADDR = 4,
        SUBTYPE_INTF_ADDR = 5,
        SUBTYPE_AGENT_ID = 6,
        SUBTYPE_LOCAL = 7
    }

    public enum PortIDSubtype : int
    {
        SUBTYPE_INTF_ALIAS = 1,
        SUBTYPE_PORT_COMP = 2,
        SUBTYPE_MAC_ADDR = 3,
        SUBTYPE_NET_ADDR = 4,
        SUBTYPE_INTF_ADDR = 5,
        SUBTYPE_AGENT_ID = 6,
        SUBTYPE_LOCAL = 7
    }

    public enum PortStatus : int 
    {
        UNKNOWN = -1,
        LINK_UP = 0,
        LINK_DOWN = 1
    }

    public enum PortPOEStatus : int
    {
        INVALID = -1,
        DISABLED = 0,
        SEARCHING = 1, 
        DELIVERING_POWER = 2,
        TEST = 3,
        FAULT = 4,
        OTHER_FAULT = 5,
        REQUESTING_POWER = 6,
        STATUS_OVERLOAD = 7
    }

    public enum PortMode : int
    {
        NONE = 0,
        GENERAL = 1,
        ACCESS = 2,
        TRUNK = 3,
        PRIVATE_HOST = 4,
        PRIVATE_HOST_PROMISCUOUS = 5
    }

    public enum PortInterfaceSpeed : int
    {
        AUTO_NEG = 1,
        HALF_100TX = 2,
        FULL_100TX = 3,
        HALF_10T = 4,
        FULL_10T = 5,
        FULL_100FX = 6,
        FULL_1000SX = 7,
        FULL_10GSX = 8,
        FULL_20GSX = 9,
        FULL_40GSX = 10,
        FULL_25GSX = 11,
        FULL_50GSX = 12,
        FULL_100GSX = 13,
        AAL5_155 = 14,
        FULL_5FX = 15,
        FULL_2P5FX = 128,
        LAG = 129,
        UNKNOWN = 130
    }

    public enum PortLinkSpeed : int
    {
        AUTO = 0,
        SP10 = 1,
        SP100 = 2,
        SP1000 = 3,
        SP10G = 4
    }

    public enum DuplexMode : int 
    {
        HALF = 0,
        FULL = 1,
        AUTO = 65535
    }

    public enum PortSTPState
    {
        DISCARDING = 1,
        LEARNING = 2, 
        FORWARDING = 3,
        DISABLED = 4,
        MANUAL_FORWARDING = 5,
        NOT_PARTICIPATING = 6
    }

    public enum PortOperationalStatus : int
    {
        DIAG_PORT_DISABLE = 0,
        DIAG_PORT_ENABLE = 1,
        DIAG_PORT_D_DISABLE = 2
    }

    public enum PortPOEPowerClass : int
    {
        INVALID = 0,
        CLASS0 = 1,
        CLASS1 = 2,
        CLASS2 = 3,
        CLASS3 = 4,
        CLASS4 = 5
    }

    public enum PortPOEPowerLimit : int
    {
        INVALID = 0,
        DOT3AF = 1,
        USER = 2, 
        NONE = 3,
        COUNT = 4
    }

    /// <summary>
    /// an enum representing the entry type integer provided by the netgear device, allowing for easier comparison and debug printing
    /// </summary>
    public enum EntryType : int
    {
        Static = 0,
        Learned = 1,
        Management = 2,
        GMRPLearned = 3,
        Self = 4,
        Dot1XStatic = 5,
        Dot1AGStatic = 6,
        RoutingInterfaceAddress = 7,
        LearnedNotGuaranteed = 8,
        FIPSnoopingLearned = 9,
        CPClientMACAddress = 10,
        ETHCFMStatic = 11,
        Y1731Static = 12
    }

    /// <summary>
    /// a class representing an entry in the forwarding database
    /// </summary>
    public class ForwardingDatabaseEntry
    {
        [JsonProperty("interface")]
        public int Interface;
        
        [JsonProperty("vlanId")]
        public int VLAN;
        
        [JsonProperty("mac")]
        public string MAC;
        
        [JsonProperty("entryType")]
        public EntryType DatabaseEntryType;

        public ForwardingDatabaseEntry()
        {
        }

        public ForwardingDatabaseEntry(int deviceInterface, int vlanID, string mac, int type)
        {
            this.Interface = deviceInterface;
            this.VLAN = vlanID;
            this.MAC = mac;
            this.DatabaseEntryType = (EntryType)type;
        }
    }

    public class RateLimit
    {
        [JsonProperty("status")]
        public ushort Status { get; private set; }

        [JsonProperty("threshold")]
        public ushort Threshold { get; private set; }

        public RateLimit()
        {
            this.Status = 0;
            this.Threshold = 5;
        }

        public RateLimit(ushort status, ushort threshold)
        {
            this.Status = status;
            this.Threshold = threshold;
        }
    }
}