using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using InfluxDB.Client.Core;

namespace AutomatikProjekt.Shared
{
    [Measurement("temperature")]
    public class TemperatureSensor : BaseClass
    {
        [JsonProperty("value")]
        public double Temperature { get; set; }
    }
}
