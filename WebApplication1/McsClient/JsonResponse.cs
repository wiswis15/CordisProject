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

namespace MCDBackend.McsClient
{
    /*
        {
            "type":"MULTI_OBJECT",
            "value":[
                {
                    "name":"ErrorActive",
                    "unit":"",
                    "type":"BOOLEAN_OBSERVER",
                    ”ObserverTesterB”:{
                        “fullPath”:"IP 192.168.10.1.1.1 | /TestProject/Main",
                        "name":null
                    },
                    "currentValue":false,
                    "trueText":null,
                    "falseText":null
                }
            ]
        }
    */

    public class JsonResponse
    {
        public string type { get; set; }
        public JsonValue[] value { get; set; }
    }

    public class JsonValue
    {
        public string name { get; set; }
        public string unit { get; set; }
        public string type { get; set; }
        public JsonMachinePart machinePart { get; set; }
        public Object currentValue { get; set; }
        public string trueText { get; set; }
        public string falseText { get; set; }
    }

    public class JsonMachinePart
    {
        public string fullPath { get; set; }
        public string name { get; set; }
    }
}
