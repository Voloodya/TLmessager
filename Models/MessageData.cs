using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLmessanger.Models
{
    public class MessageData
    {
        [JsonProperty("userName")]
        public string UserName { get; set; }
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("textMessage")]
        public string TextMessage { get; set; }

    }
}
