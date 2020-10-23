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

namespace MCDBackend.McsClient
{
    /*
        {
            "address":"127.0.0.1",
            "port":"8080",
            "registervars":[
                {
                    "path":"BeckhoffOPC/Machine/ObserverTesterA"
                },
                {
                    "path":"BeckhoffOPC/Machine/ObserverTesterB"
                }
            "unregistervars":[
                {
                    "path":"BeckhoffOPC/Machine/ObserverTesterC"
                },
                {
                    "path":"BeckhoffOPC/Machine/ObserverTesterD"
                },
                {
                    "path":"BeckhoffOPC/Machine/ObserverTesterE"
                }
            ]
        }
    */

    public class JsonClientRequest
    {
        public string address { get; set; }
        public int port { get; set; }
        public JsonVariables[] registervars { get; set; }
        public JsonVariables[] unregistervars { get; set; }

        override
        public string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("JsonClientRequest: [Address: ").Append(address);
            sb.Append(", Port: ").Append(port);
            sb.Append(", Registervars: [");
            if (registervars != null)
            {
                foreach (JsonVariables variable in registervars)
                {
                    sb.Append("\n\t").Append(variable.path);
                }
            }
            else
            {
                sb.Append("None");
            }
            sb.Append("\n], Unregistervars: [");
            if (unregistervars != null)
            {
                foreach (JsonVariables variable in unregistervars)
                {
                    sb.Append("\n\t").Append(variable.path);
                }
            }
            else
            {
                sb.Append("None");
            }
            sb.Append("\n]");
            return sb.ToString();
        }
    }

    public class JsonVariables
    {
        public string path { get; set; }
    }
}
