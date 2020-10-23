using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Cordis.MdsiClientWrapper.Helpers
{
    [Serializable]
    public class XmlConfiguration
    {
        #region Public methods

        /// <summary>
        /// Load from xml file.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="type"></param>
        /// <param name="errorDetails"></param>
        /// <returns></returns>
        static public XmlConfiguration LoadFromFile(string filename, Type type, out string errorDetails)
        {
            XmlConfiguration rv = null;
            errorDetails = "";
            if (File.Exists(filename))
            {
                try
                {

                    using (XmlTextReader txtRdr = new XmlTextReader(new StreamReader(filename)))
                    {
                        XmlSerializer xml = new XmlSerializer(type);
                        rv = (XmlConfiguration)xml.Deserialize(txtRdr);
                    }
                }
                catch (Exception ex)
                {
                    errorDetails = ex.Message + " in " + filename;
                    rv = null;
                }
            }
            else
            {
                errorDetails = "Could not find file '" + filename + "'";
            }
            return rv;
        }

        /// <summary>
        /// Save into an xml file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="append"></param>
        /// <param name="errorMsg"></param>
        /// <returns>
        /// true: save succesfull
        /// false: save failed
        /// </returns>
        public bool Save(string fileName, bool append, out string errorMsg)
        {
            errorMsg = "";
            bool succeeded = true;
            try
            {
                using (XmlTextWriter txtWrt = new XmlTextWriter(new StreamWriter(fileName, append)))
                {
                    txtWrt.Formatting = Formatting.Indented;
                    XmlSerializer xml = new XmlSerializer(GetType());
                    xml.Serialize(txtWrt, this);
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                succeeded = false;
            }
            return succeeded;
        }

        #endregion
    }
}
