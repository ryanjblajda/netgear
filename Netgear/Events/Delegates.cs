using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Netgear.Events
{
    public delegate void DigitalAnalogPayload(object sender, DigitalAnalogPayloadEventArgs args);
    public delegate void DigitalAnalogArrayPayload(object sender, DigitalAnalogArrayPayloadEventArgs args);

    public delegate void StringPayload(object sender, StringPayloadEventArgs args);
    public delegate void StringArrayPayload(object sender, StringArrayPayloadEventArgs args);

    public delegate void NetgearDevicePayload(NetgearSwitch device);
    public delegate void NetgearDeviceInterfacePayload(object sender, NetgearSwitchInterface switchInterface);

    public delegate void DeviceInfoPayload(object sender, DeviceInfoPayloadEventArgs args);
    public delegate void DeviceNamePayload(object sender, DeviceNamePayloadEventArgs args);

    public delegate void DeviceFowardingDatabasePayload(object sender, DeviceForwardingDatabasePayloadEventArgs args);
    public delegate void DevicePortStatisticsPayload(object sender, DevicePortStatisticsPayloadEventArgs args);
    public delegate void DevicePortConfigurationPayload(object sender, DevicePortConfigurationPayloadEventArgs args);
    public delegate void DevicePortPOEConfigurationPayload(object sender, DevicePortPOEConfigurationPayloadEventArgs args);

    public delegate void DeviceInterfaceNeighborPayload(object sender, DeviceInterfaceNeighborPayloadEventArgs args);
}