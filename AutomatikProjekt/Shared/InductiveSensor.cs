using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatikProjekt.Shared
{
    public class InductiveSensor : BaseClass
    {
        public bool IsMetal { get; set; }
        public InductiveSensor(long timestamp) : base(timestamp)
        {
            TimeConverter(timestamp);
        }
    }
}
