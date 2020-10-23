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
    public class ValueTypeDao
    {
        public int id { get; set; }
        public string name { get; set; }

        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ValueType: [Id: ").Append(id);
            sb.Append(", Name: ").Append(name);
            sb.Append("]");
            return sb.ToString();
        }
    }
}
