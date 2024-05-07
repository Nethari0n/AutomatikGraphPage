using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public class DistanceSensor : BaseClass
    {
        public double Distance { get; set; }

        public DistanceSensor(long timestamp) : base(timestamp)
        {
            TimeConverter(timestamp);
        }
    }
}
