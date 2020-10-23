using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.hubs
{
    public class CurrentVariableValue
    {
        public string VariablePath { get; set; }
        public DateTime LastUpdated { get; set; }
        public string CurrentValue { get; set; }

    }
}
