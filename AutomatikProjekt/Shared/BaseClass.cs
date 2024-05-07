using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public abstract class BaseClass
    {
        public DateTime TimeStamp { get; set; }

        public BaseClass(long timestamp)
        {

        }

        public void TimeConverter(long timestamp) => TimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
    }
}
