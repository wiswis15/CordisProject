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
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MCDBackend.XmlConfig
{
    [XmlRoot(ElementName="configuration"), XmlType("configuration")]
    public class Configuration
    {
        [XmlAttribute("noNamespaceSchemaLocation", Namespace=System.Xml.Schema.XmlSchema.InstanceNamespace)]
        public string noNamespaceSchemaLocation="BackendConfig.xsd";

        [XmlElement("server")]
        public Server server;
        [XmlElement("variable")]
        public List<Variable> variable;
    }

    public class Server
    {
        [XmlElement("address")]
        public string address;
        [XmlElement("port")]
        public int port;
        [XmlElement("path")]
        public string path;
    }

    public class Variable
    {
        [XmlElement("controllername")]
        public string controllername;
        [XmlElement("machinename")]
        public string machinename;
        [XmlElement("machinepart")]
        public string machinepart;
        [XmlElement("observername")]
        public string observername;
        [XmlElement("interval")]
        public int interval;
    }
}
