using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QAction_1.Models
{
    public class TransportStream
    {
        [JsonProperty("ts_id")]
        public int TsId { get; set; }

        [JsonProperty("ts_name")]
        public string TsName { get; set; }

        [JsonProperty("multicast")]
        public string Multicast { get; set; }

        [JsonProperty("sourceIp")]
        public string SourceIp { get; set; }

        [JsonProperty("network_id")]
        public int NetworkId { get; set; }

        [JsonProperty("services")]
        public List<Service> Services { get; set; }
    }

}
