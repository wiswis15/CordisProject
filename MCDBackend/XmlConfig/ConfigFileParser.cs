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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using log4net;

namespace MCDBackend.XmlConfig
{
    public static class ConfigFileParser
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConfigFileParser));
        private static readonly string XSDFILE = "BackendConfig.xsd";

        public static Configuration ParseConfigFile(string filename)
        {
            return DeserializeAndValidate(filename, XSDFILE);
        }

        private static Configuration DeserializeAndValidate(string filename, String validationFilename)
        {
            log.Info("Validating and reading xml file");

            Configuration result = null;
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("http://www.w3.org/2001/XMLSchema-instance", validationFilename);
            var settings = new XmlReaderSettings
            {
                Schemas = schema,
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ProcessIdentityConstraints |
                                  XmlSchemaValidationFlags.ReportValidationWarnings |
                                  XmlSchemaValidationFlags.ProcessSchemaLocation
            };
            settings.ValidationEventHandler += XmlValidationEventHandler;
            try
            {
                XmlReader configurationReader = XmlReader.Create(filename, settings);
                //XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                //if (serializer.CanDeserialize(configurationReader))
                //{
                    XmlSerializer ser = new XmlSerializer(typeof(Configuration));
                    result = (Configuration)ser.Deserialize(configurationReader);
                //}
            }
            catch (Exception ex)
            {
                HelperUtilities.LogException("Reading file " + filename + " failed.", log, ex);
            }
            return result;
        }

        private static void XmlValidationEventHandler(Object sender, ValidationEventArgs e)
        {
            if (e.Severity.Equals(XmlSeverityType.Error))
            {
                log.Info(e.Severity.ToString() + ": " + e.Message);
            }
        }
    }
}
