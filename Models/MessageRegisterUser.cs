using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLmessanger.Models
{
    public class MessageRegisterUser
    {
        [JsonProperty("userNameMessanger")]
        public string userNameMessanger { get; set; }
        [JsonProperty("famileName")]
        public string famileName { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("patronimicName")]
        public string patronimicName { get; set; }
        [JsonProperty("phoneNumber")]
        public string phoneNumber { get; set; }
        [JsonProperty("fieldActivityName")]
        public string fieldActivityName { get; set; }
        [JsonProperty("organization")]
        public string organization { get; set; }
        [JsonProperty("group")]
        public string group { get; set; }
        [JsonProperty("user")]
        public string user { get; set; }

    }
}
