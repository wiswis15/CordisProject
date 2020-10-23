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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using log4net;
using MCDBackend.McsClient;
using Newtonsoft.Json;
using WebApplication1;

// Code from: https://gist.github.com/aksakalli/9191056
namespace MCDBackend
{
    class McdClientListener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(McdClientListener));

        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        private readonly MainClass.McdClientDelegate _mcdClientHandler;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="port">Port of the server.</param>
        /// <param name="mcdClientHandler">Handle to MCD client delegate method.</param>
        public McdClientListener(int port, MainClass.McdClientDelegate mcdClientHandler)
        {
            _mcdClientHandler = mcdClientHandler;
            this.Initialize(port);
        }

        /// <summary>
        /// Construct server with suitable port.
        /// </summary>
        public McdClientListener()
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
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://*:" + _port.ToString() + "/");
            _listener.Start();
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Stopped listening to port " + _port.ToString(), log, ex);
                }
            }
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.IsBackground = true;
            _serverThread.Start();
        }

        private void Process(HttpListenerContext context)
        {
            string content = "";
            if (context.Request.HasEntityBody)
            {
                // Code from: https://gist.github.com/leggetter/769688
                using (Stream receiveStream = context.Request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, context.Request.ContentEncoding))
                    {
                        content = readStream.ReadToEnd();
                        if (!string.IsNullOrEmpty(content))
                        {
                            JsonClientRequest responseValue = JsonConvert.DeserializeObject<JsonClientRequest>(content);
                            if (responseValue != null)
                            {
                                log.Debug(responseValue.ToString());
                                _mcdClientHandler(responseValue);
                            }
                            else
                            {
                                log.Warn("Could not convert response payload: " + content);
                            }
                        }
                    }
                }
            }
            log.Debug("Received message from ip: " + context.Request.RemoteEndPoint.ToString());

            try
            {
                string payload = "Client is registered";
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
                HelperUtilities.LogException("Error sending response", log, ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            context.Response.OutputStream.Close();
        }
    }
}
