using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QAction_1.Models
{
    public class DeviceResponse
    {
        [JsonProperty("transport_streams")]
        public List<TransportStream> TransportStreams { get; set; }
    }

}
