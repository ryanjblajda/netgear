using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Net.Https;

namespace Netgear.Communications
{
    public class Request
    {
        public string Type { get; private set; }
        public HttpsClientRequest HttpsRequest { get; private set; }

        public Request(HttpsClientRequest request, string type)
        {
            this.HttpsRequest = request;
            this.Type = type;
        }

        public override bool Equals(object obj)
        {
            if (typeof(Request) == obj.GetType())
            {
                if (((Request)obj).Type == this.Type) { return true; }
                else { return false; }
            }
            else { return false; }
        }
    }
}