using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public class TemperatureSensor : BaseClass
    {
        public double Temperature { get; set; }
        public TemperatureSensor(long timestamp) : base(timestamp)
        {
            TimeConverter(timestamp);
        }
    }
}
