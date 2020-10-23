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

using System;
using System.Text;

namespace MCDBackend.Database
{
    public class PartValueDao
    {
        public int id { get; set; }
        public DateTime event_timestamp { get; set; }
        public PartVariableDao part_variable { get; set; }
        public ValueTypeDao part_type { get; set; }
        public Nullable<bool> value_bool { get; set; }
        public Nullable<double> value_float { get; set; }
        public string value_enum { get; set; }
        public Nullable<int> value_int { get; set; }
        public string value_string { get; set; }
        public Nullable<long> value_word { get; set; }

        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("PartValue: [Id: ").Append(id);
            sb.Append(", Timestamp: ").Append(event_timestamp);
            sb.Append(", BoolValue: ").Append(value_bool.HasValue ? value_bool.ToString() : "Null");
            sb.Append(", FloatValue: ").Append(value_float.HasValue ? value_float.ToString() : "Null");
            sb.Append(", EnumValue: ").Append(value_enum != null ? value_enum.ToString() : "Null");
            sb.Append(", IntValue: ").Append(value_int.HasValue ? value_int.ToString() : "Null");
            sb.Append(", StringValue: ").Append(value_string != null ? value_string.ToString() : "Null");
            sb.Append(", WordValue: ").Append(value_word.HasValue ? value_word.ToString() : "Null");
            sb.Append(", ").Append(part_variable != null ? part_variable.ToString() : "Null");
            sb.Append(", ").Append(part_type != null ? part_type.ToString() : "Null");
            sb.Append("]");
            return sb.ToString();
        }
    }
}
