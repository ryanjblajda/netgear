using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Netgear.Communications;
using Netgear.Resources;

namespace Netgear.Events
{
    public class DeviceInterfaceNeighborPayloadEventArgs : EventArgs
    {
        public NeighborInfo Payload { get; private set; }

        public DeviceInterfaceNeighborPayloadEventArgs()
        {
        }

        public DeviceInterfaceNeighborPayloadEventArgs(NeighborInfo payload)
        {
            this.Payload = payload;
        }
    }

    public class DeviceNamePayloadEventArgs : EventArgs
    {
        public DeviceNameResponsePayload Payload { get; private set; }

        public DeviceNamePayloadEventArgs()
        {
        }

        public DeviceNamePayloadEventArgs(DeviceNameResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class DeviceInfoPayloadEventArgs : EventArgs
    {
        public DeviceInfoResponsePayload Payload { get; private set; }

        public DeviceInfoPayloadEventArgs()
        {
        }

        public DeviceInfoPayloadEventArgs(DeviceInfoResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class DeviceForwardingDatabasePayloadEventArgs : EventArgs
    {
        public List<ForwardingDatabaseEntry> Entries { get; private set; }
        public ushort Interface { get; private set; }

        public DeviceForwardingDatabasePayloadEventArgs()
        {
            this.Entries = new List<ForwardingDatabaseEntry>();
        }

        public DeviceForwardingDatabasePayloadEventArgs(ushort interfaceID, List<ForwardingDatabaseEntry> entries) : this()
        {
            this.Entries = entries;
            this.Interface = interfaceID;
        }
    }

    public class DevicePortStatisticsPayloadEventArgs : EventArgs
    {
        public DevicePortStatisticsResponsePayload Payload { get; private set; }

        public DevicePortStatisticsPayloadEventArgs()
        {
            this.Payload = new DevicePortStatisticsResponsePayload();
        }

        public DevicePortStatisticsPayloadEventArgs(DevicePortStatisticsResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class DevicePortPOEConfigurationPayloadEventArgs : EventArgs
    {
        public DevicePortPOEConfigurationRequestResponsePayload Payload { get; private set; }

        public DevicePortPOEConfigurationPayloadEventArgs()
        {
            this.Payload = new DevicePortPOEConfigurationRequestResponsePayload();
        }

        public DevicePortPOEConfigurationPayloadEventArgs(DevicePortPOEConfigurationRequestResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class DevicePortConfigurationPayloadEventArgs : EventArgs
    {
        public DevicePortConfigurationRequestResponsePayload Payload { get; private set; }

        public DevicePortConfigurationPayloadEventArgs()
        {
            this.Payload = new DevicePortConfigurationRequestResponsePayload();
        }

        public DevicePortConfigurationPayloadEventArgs(DevicePortConfigurationRequestResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class DigitalAnalogPayloadEventArgs : EventArgs
    {
        public ushort Payload;

        public DigitalAnalogPayloadEventArgs() 
        {
        }

        public DigitalAnalogPayloadEventArgs(bool payload) : this()
        {
            this.Payload = Conversion.ConvertToUshort(payload);
        }

        public DigitalAnalogPayloadEventArgs(ushort payload) : this()
        {
            this.Payload = payload;
        }

        public DigitalAnalogPayloadEventArgs(int payload) : this()
        {
            this.Payload = (ushort)payload;
        }
    }

    public class DigitalAnalogArrayPayloadEventArgs : EventArgs
    {
        public ushort[] Payload;
        public ushort PayloadLength;

        public DigitalAnalogArrayPayloadEventArgs() 
        {
        }

        public DigitalAnalogArrayPayloadEventArgs(ushort[] payload, int length)
        {
            this.Payload = payload;
            this.PayloadLength = (ushort)length;
        }
    }

    public class StringPayloadEventArgs : EventArgs
    {
        public string Payload;
        
        public StringPayloadEventArgs() 
        {
            this.Payload = String.Empty;
        }

        public StringPayloadEventArgs(string payload)
        {
            this.Payload = payload;
        }
    }

    public class StringArrayPayloadEventArgs : EventArgs
    {
        public string[] Payload;
        public ushort PayloadLength;

        public StringArrayPayloadEventArgs() 
        {
        }

        public StringArrayPayloadEventArgs(string[] payload, int length)
        {
            this.Payload = payload;
            this.PayloadLength = (ushort)length;
        }
    }
}