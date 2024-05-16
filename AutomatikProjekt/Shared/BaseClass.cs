using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public abstract class BaseClass
    {
        [JsonProperty("data.values.timestamp")]
        public long UnixTime { get => UnixTime; set => TimeConverter(UnixTime); }
        public DateTime TimeStamp {  get; set; }

        public BaseClass()
        {

        }

        public void TimeConverter(long timestamp) => TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }
}
