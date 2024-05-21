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
        //private long _unixTime;
        //[JsonProperty("timestamp")]
        //public long UnixTime { get => _unixTime; set => TimeConverter(UnixTime); }
        public DateTime TimeStamp {  get; set; }

        public BaseClass()
        {

        }

        public void TimeConverter(long timestamp) => TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }
}
