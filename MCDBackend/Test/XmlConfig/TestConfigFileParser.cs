using System;
using NUnit.Framework;
using MCDBackend.XmlConfig;

namespace MCDBackend.Test.XmlConfig
{
    [TestFixture]
    public class TestConfigFileParser
    {
        [SetUp]
        protected void SetUp()
        {
        }
        
        [Test]
        public void TestParseXmlFile()
        {
            Configuration config = ConfigFileParser.ParseConfigFile("TestBackendConfig.xml");
            Assert.NotNull(config);
            /*
            <address>123.45.67.89</address>
            <port>321</port>
            <path>Test/path.with.dots/</path>
            <controllername>TestControllerName</controllername>
            <machinename>TestMachineName</machinename>
            <machinepart>TestMachinePart</machinepart>
            <observername>TestObserverName</observername>
            <interval>12345</interval>
            */
            Assert.AreEqual(1, config.variable.Count);
            Assert.AreEqual("123.45.67.89", config.server.address);
            Assert.AreEqual(321, config.server.port);
            Assert.AreEqual("Test/path.with.dots/", config.server.path);
            Assert.AreEqual("TestControllerName", config.variable[0].controllername);
            Assert.AreEqual("TestMachineName", config.variable[0].machinename);
            Assert.AreEqual("TestMachinePart", config.variable[0].machinepart);
            Assert.AreEqual("TestObserverName", config.variable[0].observername);
            Assert.AreEqual(12345, config.variable[0].interval);
        }
    }
}
