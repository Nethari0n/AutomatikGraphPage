using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public class TemperatureSensor : BaseClass
    {
        [JsonProperty("value")]
        public double Temperature { get; set; }
    }
}
