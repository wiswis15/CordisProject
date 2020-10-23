using System;

namespace Cordis.MdsiClientWrapper.Helpers
{

    [Serializable]
    public class CordisServerConfiguration
    {
        public string Description = "";
        public string ServerAddress = "";
        public string SecurityToken = "";
    }

    [Serializable]
    public class ClientConfiguration : XmlConfiguration
    {

        public ClientConfiguration()
        {
            CordisServerConfiguration = new CordisServerConfiguration();
        }

        /// <summary>
        /// Load from xml file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="errorDetails"></param>
        /// <returns></returns>
        public static ClientConfiguration LoadFromFile(string filename, out string errorDetails)
        {
            ClientConfiguration configuration = null;
            XmlConfiguration result = LoadFromFile(filename, typeof(ClientConfiguration), out errorDetails);
            if ((result != null) && (result is ClientConfiguration))
            {
                configuration = (ClientConfiguration)(result);
            }
            return configuration;
        }

        public CordisServerConfiguration CordisServerConfiguration;

    }
}
