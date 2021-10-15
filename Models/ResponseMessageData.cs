using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLmessanger.Models
{
    public class ResponseMessageData
    {
        [JsonProperty("userName")]
        public string userName { get; set; }
        [JsonProperty("phoneNumber")]
        public string phoneNumber { get; set; }
        [JsonProperty("textMessage")]
        public string textMessage { get; set; }
        [JsonProperty("status")]
        public string status { get; set; }
        [JsonProperty("byteQrcode")]
        public byte[] byteQrcode { get; set; }
    }
}
