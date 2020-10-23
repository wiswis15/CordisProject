using System;
using System.IO;
using System.Net;
using log4net;
using MCDBackend.XmlConfig;
using Newtonsoft.Json;
using System.Text;
using MCDBackend.Database;
using System.Linq;

namespace MCDBackend.McsClient
{
    public class McsClientHandler
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(VariableObject));

        private string _url;
        private Variable _variable;
        private Server _server;

        public Object CurrentValue { get; private set; }
        public string VariableType { get; private set; }
        public PartVariableDao PartVariable { get; private set; }

        public McsClientHandler(Server server, Variable variable)
        {
            _server = server;
            _url = BuildUrl(server, "getObserverValues");
            log.Debug("Using url for MCS communication: " + _url);
            _variable = variable;
            VariableType = null;
            PartVariable = null;
        }

        public bool RequestVariableFromMCS()
        {
            bool success;
            log.Debug("Sending request to MCS server.");
            try
            {
                string payload = GetRequestPayload();
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.ContentLength = payload.Length;
                StreamWriter requestWriter = new StreamWriter(request.GetRequestStream(), System.Text.Encoding.ASCII);
                requestWriter.Write(payload);
                requestWriter.Close();

                log.Debug("Getting response form MCS server.");
                WebResponse webResponse = request.GetResponse();
                Stream webStream = webResponse.GetResponseStream();
                StreamReader responseReader = new StreamReader(webStream);
                string responsePayload = responseReader.ReadToEnd();
                responseReader.Close();

                success = UpdatePartVariableAndTypeFromPayload(responsePayload);
            }
            catch (Exception ex)
            {
                HelperUtilities.LogException("Sending or getting request failed.", log, ex);
                success = false;
            }
            return success;
        }

        private string BuildUrl(Server server, string command)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("http://{0}:{1}/{2}", server.address, server.port, server.path);
            if (!server.path.EndsWith("/", StringComparison.CurrentCulture))
            {
                sb.Append("/");
            }
            sb.Append(command);
            return sb.ToString();
        }

        private string GetRequestPayload()
        {
            JsonRequest jrq = new JsonRequest();
            JsonInputArgs ji = new JsonInputArgs();
            JsonCriteria jc = new JsonCriteria();
            string machinePartTree = _variable.controllername + Path.DirectorySeparatorChar + _variable.machinename
                + Path.DirectorySeparatorChar + _variable.machinepart;
            jc.machinePartRegEx = machinePartTree;
            jc.observerNameRegEx = _variable.observername;
            jc.timeStampFrom = "";
            jc.timeStampTo = "";
            ji.criteria = jc;
            JsonSecutiryToken jst = new JsonSecutiryToken();
            jst.SecurityToken = "ADMIN";
            jrq.InputArgs = new JsonInputArgs[1];
            jrq.InputArgs[0] = ji;

            // Convert object into JSON
            string payload = JsonConvert.SerializeObject(new[] { (Object)jst, (Object)jrq }, Formatting.Indented);
            //log.Debug("Request Payload: '" + payload + "'\n");
            return payload;
        }

        private bool UpdatePartVariableAndTypeFromPayload(string payload)
        {
            PartVariable = null;
            VariableType = null;
            CurrentValue = null;
            JsonResponse responseValue = JsonConvert.DeserializeObject<JsonResponse>(payload);

            if (responseValue == null || responseValue.value == null || responseValue.value.Length == 0)
            {
                log.Error("Unable to get PartVariable from payload:");
                log.Error(payload);
                return false;
            }

            // TODO: Implement for multiple values
            if (responseValue.value[0].machinePart == null)
            {
                log.Error("Machine part of response value not set.");
                return false;
            }
            if (string.IsNullOrEmpty(responseValue.value[0].machinePart.fullPath))
            {
                log.Error("Full path of response value not set.");
                return false;
            }
            
            // Processing fullPath to get machine_ip and machine_part
            string[] mp = responseValue.value[0].machinePart.fullPath.Split('|');
            if (mp.Count() != 2)
            {
                mp = new string[2];
                mp[0] = null;
                int posLastSlash = responseValue.value[0].machinePart.fullPath.LastIndexOf('/');
                if (posLastSlash != -1)
                {
                    mp[1] = responseValue.value[0].machinePart.fullPath.Substring(posLastSlash + 1);
                }
                else
                {
                    mp[1] = null;
                }
            }
            else
            {
                mp[0] = mp[0].Trim();
                mp[1] = mp[1].Trim();
            }

            VariableType = responseValue.value[0].type;
            CurrentValue = responseValue.value[0].currentValue;
            PartVariable = new PartVariableDao
            {
                name = responseValue.value[0].name,
                machine_ip = mp[0],
                machine_part = mp[1],
                path = responseValue.value[0].machinePart.fullPath,
                unit = responseValue.value[0].unit
            };
            return true;
        }
    }
}
