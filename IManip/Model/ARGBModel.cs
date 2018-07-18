using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IManip.Model
{
    class ARGBModel
    {
        [JsonProperty("a")]
        public byte A { get; set; }

        [JsonProperty("r")]
        public byte R { get; set; }

        [JsonProperty("g")]
        public byte G { get; set; }

        [JsonProperty("b")]
        public byte B { get; set; }
    }
}
