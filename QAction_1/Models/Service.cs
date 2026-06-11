using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QAction_1.Models
{
    public class Service
    {
        [JsonProperty("service_id")]
        public int ServiceId { get; set; }

        [JsonProperty("service_name")]
        public string ServiceName { get; set; }

        [JsonProperty("service_type")]
        public string ServiceType { get; set; }

        [JsonProperty("service_provider")]
        public string ServiceProvider { get; set; }
    }

}
