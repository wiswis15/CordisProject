using System;
namespace MCDDatabase
{
    /***
        [
            {
                "SecurityToken":"ADMIN"
            },
            {
                "InputArgs":[
                {
                    "criteria":{
                        "machinePartRegEx":"BeckhoffOPC/Machine/ObserverTesterB",
                        "observerNameRegEx":"Int16Var",
                        "timeStampFrom":"",
                        "timeStampTo":"",
                        "rowLimit":0
                    },
                    "machinePartFullPath":"/",
                }
                ]
            }
        ]
     */

    public class JsonSecutiryToken
    {
        public String SecurityToken { get; set; }
    }

    public class JsonRequest
    {
        public JsonInputArgs[] InputArgs { get; set; }
    }

    public class JsonInputArgs
    {
        public JsonCriteria criteria { get; set; }
        public string machinePartFullPath { get; set; }
    }

    public class JsonCriteria
    {
        public string machinePartRegEx { get; set; }
        public string observerNameRegEx { get; set; }
        public string timeStampFrom { get; set; }
        public string timeStampTo { get; set; }
        public Nullable<long> rowLimit { get; set; }
    }
}
