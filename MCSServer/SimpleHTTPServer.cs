using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MCDDatabase;
using Newtonsoft.Json;
using System.Linq;

// Code from: https://gist.github.com/aksakalli/9191056
namespace MCSServer
{
    class SimpleHTTPServer
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;
        private readonly Random rnd = new Random();

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="port">Port of the server.</param>
        public SimpleHTTPServer(int port)
        {
            this.Initialize(port);
        }

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        public SimpleHTTPServer()
        {
            //get an empty port
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            this.Initialize(port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            if (_listener != null)
            {
                _listener.Stop();
            }
        }

        private void Listen()
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
                _listener.Start();
            }
            catch (HttpListenerException ex)
            {
                Console.Out.WriteLine("MCSServer HttpListenerException: " + ex.Message + "\nErrorCode: " + ex.ErrorCode + "\nDetails: " + ex.InnerException);
                _listener = null;
                return;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine("MCSServer Error: " + ex.GetType().ToString() + ": " + ex.Message + "\nDetails: " + ex.InnerException);
                _listener = null;
                return;
            }
            Console.Out.WriteLine("MCSServer: Listening...");
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("MCSServer Warning: " + ex.Message);
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string content = "";
            string method = "";
            List<JsonRequest> jsonRequest = null;

            Console.Out.WriteLine("MCSServer Received: \n" + context.Request.RawUrl);

            int posLastSlash = context.Request.Url.AbsolutePath.LastIndexOf('/');
            if (posLastSlash != -1)
            {
                method = context.Request.Url.AbsolutePath.Substring(posLastSlash + 1);
            }
            Console.Out.WriteLine("MCSServer Method: " + method);

            if (method.Equals("getObserverValues"))
            {
                if (context.Request.HasEntityBody)
                {
                    // Code from: https://gist.github.com/leggetter/769688
                    using (Stream receiveStream = context.Request.InputStream)
                    {
                        using (StreamReader readStream = new StreamReader(receiveStream, context.Request.ContentEncoding))
                        {
                            content = readStream.ReadToEnd();
                            Console.Out.WriteLine("MCSServer content received: " + content);
                            jsonRequest = JsonConvert.DeserializeObject<List<JsonRequest>>(content);
                        }
                    }
                }

                try
                {
                    JsonResponse jrs = new JsonResponse();
                    JsonCriteria jcr = null;
                    string fullPath = null;
                    jrs.type = "MULTI_OBJECT";
                    if (jsonRequest != null && jsonRequest[1] != null && jsonRequest[1].InputArgs != null
                        && jsonRequest[1].InputArgs.Any())
                    {
                        jcr = jsonRequest[1].InputArgs[0].criteria;
                        if (jcr == null)
                        {
                            jcr = new JsonCriteria();
                            jcr.observerNameRegEx = "Int32Var";
                        }
                        fullPath = jsonRequest[1].InputArgs[0].machinePartFullPath;
                        if (fullPath != null)
                        {
                            Console.Out.WriteLine("Received full path: " + fullPath);
                        }
                    }
                    else
                    {
                        /*
                         JsonCriteria:
                            "machinePartRegEx":"BeckhoffOPC/Machine/ObserverTesterB",
                            "observerNameRegEx":"Int16Var",
                            "timeStampFrom":"",
                            "timeStampTo":"",
                            "rowLimit":0
                         */
                        jcr = new JsonCriteria();
                        jcr.observerNameRegEx = "EnumVar";
                    }

                    JsonValue jv = new JsonValue();
                    if (jcr != null && !string.IsNullOrEmpty(jcr.observerNameRegEx))
                    {
                        jv.name = jcr.observerNameRegEx;
                        jv.unit = null;
                        jv.trueText = null;
                        jv.falseText = null;
                        if (jv.name.Equals("BoolVar"))
                        {
                            jv.type = "BOOLEAN_OBSERVER";
                            jv.currentValue = (Object)((rnd.NextDouble() >= 0.5));
                            jv.trueText = "Open";
                            jv.falseText = "Closed";
                        }
                        else if (jv.name.Equals("FloatVar"))
                        {
                            jv.type = "FLOAT_OBSERVER";
                            jv.currentValue = (Object)((float)Math.Round(rnd.NextDouble() * 10.0, 1));
                            jv.unit = "m/s";
                        }
                        else if (jv.name.Equals("EnumVar"))
                        {
                            String[] enumList = { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten" };
                            jv.type = "ENUM_OBSERVER";
                            jv.currentValue = (Object)enumList[(rnd.Next() % 10)];
                        }
                        else if (jv.name.Equals("Int32Var"))
                        {
                            jv.type = "INT_OBSERVER";
                            jv.currentValue = (Object)(rnd.Next() % 10);
                            jv.unit = "steps";
                        }
                        else if (jv.name.Equals("StringVar"))
                        {
                            String[] stringList = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
                            jv.type = "STRING_OBSERVER";
                            jv.currentValue = (Object)stringList[(rnd.Next() % 10)];
                        }
                        else if (jv.name.Equals("Int64Var"))
                        {
                            jv.type = "WORD_OBSERVER";
                            jv.currentValue = (Object)((long)rnd.Next() % 10L);
                        }

                        JsonMachinePart jo = new JsonMachinePart();
                        jo.fullPath = "BeckhoffOPC/Machine/SystemController";
                        jo.name = null; // not used

                        jv.machinePart = jo;
                    }
                    jrs.value = new JsonValue[1];
                    jrs.value[0] = jv;

                    // Convert object into JSON
                    string payload = JsonConvert.SerializeObject(jrs);
                    Console.Out.WriteLine("Response Payload: '" + payload + "'\n");

                    //Adding permanent http response headers
                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength64 = payload.Length;
                    context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
                    context.Response.StatusCode = (int)HttpStatusCode.OK;

                    byte[] buffer = Encoding.UTF8.GetBytes(payload);
                    context.Response.ContentLength64 = buffer.Length;
                    Stream st = context.Response.OutputStream;
                    st.Write(buffer, 0, buffer.Length);

                    context.Response.OutputStream.Flush();
                }
                catch (Exception ex)
                {
                    Console.Out.WriteLine("MCSServer Error: " + ex.Message);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            context.Response.OutputStream.Close();
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }
    }
}
