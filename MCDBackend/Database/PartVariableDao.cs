/*
 *  Copyright (c) 2020 Topic Embedded Systems
 *  All rights reserved.
 *
 *  The copyright to the computer program(s) herein is the property of
 *  Topic Embedded Systems. The program(s) may be used and/or copied
 *  only with the written permission of the owner or in accordance with
 *  the terms and conditions stipulated in the contract under which the
 *  program(s) have been supplied.
 */

using System.Text;

namespace MCDBackend.Database
{
    public class PartVariableDao
    {
        public int id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string machine_ip { get; set; }
        public string machine_part { get; set; }
        public string unit { get; set; }

        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PartVariable: [Id: ").Append(id);
            sb.Append(", Name: ").Append(name);
            sb.Append(", Path: ").Append(path);
            sb.Append(", Machine IP: ").Append(machine_ip != null ? machine_ip : "Null");
            sb.Append(", Machine Part: ").Append(machine_part != null ? machine_part : "Null");
            sb.Append(", Unit: ").Append(unit != null ? unit : "Null");
            sb.Append("]");
            return sb.ToString();
        }
    }
}
