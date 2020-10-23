using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cordis.MachineControlDashboard.Helpers
{
    class Setting
    {
        public string CsvString { get; set; }
        public string DefaultValue { get; set; }
        public string LowerLimit { get; set; }
        public string MachinepartValue { get; set; }
        public string SettingType { get; set; }
        public string Unit { get; set; }
        public string UpperLimit { get; set; }
        public string Value { get; set; }
    }
}
