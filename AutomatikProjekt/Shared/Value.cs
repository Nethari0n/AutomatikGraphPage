using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public class Value
    {
        private long _timestamp;
        public long timestamp
        {
            get { return _timestamp; }
            set
            {
                _timestamp = value;
                TimeConverter(_timestamp);
            }
        }
        public double value { get; set; }

        public DateTime Timestamp { get; set; }



        public void TimeConverter(long timestamp) => Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

        public Value()
        {

        }
    }
}

