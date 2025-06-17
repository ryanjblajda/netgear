using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Netgear.Communications;
using Netgear.Events;

namespace Netgear
{
    public static class NetgearDevices
    {
        internal static readonly BindingList<NetgearSwitch> Devices;
        public static event NetgearDevicePayload DeviceAdded;

        public static string SymbolName;

        static NetgearDevices()
        {
            Devices = new BindingList<NetgearSwitch>();
            NetgearDevices.SymbolName = Crestron.SimplSharp.CrestronEnvironment.SystemInfo.SerialNumber;
        }

        internal static bool RegisterDevice(NetgearSwitch device)
        {
            bool result = false;

            List<NetgearSwitch> devices = Devices.ToList().FindAll(dev => dev.ID == device.ID);
            //NetgearDevices.Message("Devices Count: {0}", devices.Count);

            if (devices.Count == 0)
            {
                NetgearDevices.Message("Netgear Device @ {0} Added", device.SymbolName);
                Devices.Add(device);
                if (NetgearDevices.DeviceAdded != null) NetgearDevices.DeviceAdded(device);
                result = true;
            }

            return result;
        }

        public static NetgearSwitch FindNetgearSwitchDevice(ushort deviceID)
        {
            NetgearSwitch device = null;
            
            NetgearSwitch potentialDevice = Devices.ToList().Find(dev => dev.ID == device.ID);

            if (potentialDevice != null) { device = potentialDevice; }
            
            return device;
        }

        /// <summary>
        /// prints a message to the console
        /// </summary>
        /// <param name="msg">a format string</param>
        /// <param name="objects">the array of objects to print inside the format string</param>
        private static void Message(string msg, params object[] objects)
        {
            CrestronConsole.PrintLine(String.Format("Netgear System @ {0}: {1}", SymbolName, msg), objects);
            Console.WriteLine(String.Format("Netgear System @ {0}: {1}", SymbolName, msg), objects);
        }
    }
}