using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Netgear.Resources
{
    public static class Strings 
    {
        public const string DefaultSymbolName = "Not Set";

        //urls
        public const string DeviceBaseURL = "/api/v1";
        public const string DeviceLogin = "/login";
        public const string DeviceLogout = "/logout";
        public const string DeviceInfo = "/device_info";
        public const string DeviceName = "/device_name";
        public const string DeviceForwardingDatabase = "/fdbs";
        public const string DevicePortStatistics = "/sw_portstats?portid={0}";
        public const string DevicePortConfig = "/swcfg_port?portid={0}";
        public const string DevicePortPOEConfig = "/swcfg_poe?portid={0}";

        //http headers
        public const string AuthorizationHeader = "Authorization";

        //property strings
        public const string PropertyName = "name";
        public const string PropertySerialNumber = "serialnumber";
        public const string PropertyMACAddress = "mac";
        public const string PropertyModelNumber = "model";
        public const string PropertySoftwareVersionNumber = "softwarever";
        public const string PropertyLastReboot = "lastreboot";
        public const string PropertyPortCount = "portcount";
        public const string PropertyActivePortCount = "activeports";
        public const string PropertyRTSPState = "rtsp";
        public const string PropertyMemoryInUse = "memoryinuse";
        public const string PropertyMemoryUsage = "memoryusage";
        public const string PropertyCPUUsage = "cpu";
        public const string PropertyFanState = "fans";
        public const string PropertyFanStateDetails = "fandetails";
        public const string PropertyPOEState = "poe";
        public const string PropertyUptime = "uptime";
        public const string PropertyTemperatureSensors = "temperature";
        public const string PropertyTemperatureSensorsDetails = "temperaturedetails";
        public const string PropertyBootVersion = "bootversion";
        public const string PropertyRxBytes = "rxbytes";
        public const string PropertyTxBytes = "txbytes";
        public const string PropertyAdminPOEWattage = "poewattage";
        public const string PropertyPorts = "ports";
        public const string PropertyLocation = "location";
    }
}