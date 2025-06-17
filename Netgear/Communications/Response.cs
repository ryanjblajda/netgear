using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;

namespace Netgear.Communications
{
    /// <summary>
    /// the parent class of all messages that can be recieved from the netgear device
    /// </summary>
    public class Response
    {
        [JsonProperty("resp")]
        public ResponseDetails Details { get; set; }
    }

    /// <summary>
    /// the response received from the netgear api, which will be a json object that matches the below format
    /// </summary>
    public class ResponseDetails
    {
        [JsonProperty("status")]
        public string Status { get; private set; }

        [JsonProperty("respMsg")]
        public string ResponseMessage { get; private set; }

        [JsonProperty("respCode")]
        public int ResponseCode { get; private set; }

        public ResponseDetails()
        {
            this.ResponseCode = 400;
            this.ResponseMessage = "Initial Response Constructor Message";
            this.Status = "Bad Request";
        }
    }

    public class LoginResponse : Response
    {
        [JsonProperty("login")]
        public LoginResponsePayload Payload { get; private set; }

        public LoginResponse() : base()
        {
            this.Payload = new LoginResponsePayload();
        }

        public LoginResponse(LoginResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class DeviceNameResponse : Response
    {
        [JsonProperty("deviceName")]
        public DeviceNameResponsePayload Payload { get; private set; }

        public DeviceNameResponse():base()
        {
            this.Payload = new DeviceNameResponsePayload();
        }

        public DeviceNameResponse(DeviceNameResponsePayload payload): this()
        {
            this.Payload = payload;
        }
    }

    public class DeviceInfoResponse : Response
    {
        [JsonProperty("deviceInfo")]
        public DeviceInfoResponsePayload Payload { get; private set; }

        public DeviceInfoResponse(): base()
        {
            this.Payload = new DeviceInfoResponsePayload();
        }

        public DeviceInfoResponse(DeviceInfoResponsePayload payload): this()
        {
            this.Payload = payload;
        }
    }

    public class ForwardingDatabaseResponse : Response
    {
        [JsonProperty("fdb_entries")]
        public List<ForwardingDatabaseEntry> Payload { get; private set; }

        public ForwardingDatabaseResponse(): base()
        {
            this.Payload = new List<ForwardingDatabaseEntry>();
        }

        public ForwardingDatabaseResponse(List<ForwardingDatabaseEntry> entries) : this()
        {
            this.Payload = entries;
        }
    }

    public class PortConfigurationResponse : Response
    {
        [JsonProperty("switchPortConfig")]
        public DevicePortConfigurationRequestResponsePayload Payload { get; private set; }

        public PortConfigurationResponse()   : base()
        {
            this.Payload = new DevicePortConfigurationRequestResponsePayload();
        }
    }

    public class PortStatisticsReponse : Response
    {
        [JsonProperty("switchStatsPort")]
        public DevicePortStatisticsResponsePayload Payload { get; private set; }

        public PortStatisticsReponse() : base()
        {
            this.Payload = new DevicePortStatisticsResponsePayload();
        }

        public PortStatisticsReponse(DevicePortStatisticsResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }

    public class PortPOEConfigurationResponse : Response
    {
        [JsonProperty("poePortConfig")]
        public DevicePortPOEConfigurationRequestResponsePayload Payload { get; private set; }

        public PortPOEConfigurationResponse() : base()
        {
            this.Payload = new DevicePortPOEConfigurationRequestResponsePayload();
        }

        public PortPOEConfigurationResponse(DevicePortPOEConfigurationRequestResponsePayload payload) : this()
        {
            this.Payload = payload;
        }
    }
}