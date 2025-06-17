using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace Netgear.Resources
{
    public static class Conversion
    {
        public static ushort ConvertToUshort(bool state)
        {
            return (ushort)(state == true ? 1 : 0);
        }

        public static bool ConvertToBool(ushort state)
        {
            return state == (ushort)(1) ? true : false;
        }
    }
}