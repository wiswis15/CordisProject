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
using MCDBackend.McsClient;
using Newtonsoft.Json;
using System.Linq;

namespace MCDClient
{
    class MainClass
    {
        private static HttpWebResponse httpResponse = null;
        private static HttpWebRequest httpRequest = null;
        private static Stream requestStream = null;
        private static int listenPort = 11000;

        public static void Main(string[] args)
        {
            try
            {
                if (args.Count() > 0)
                {
                    if (int.TryParse(args[0], out int port))
                    {
                        listenPort = port;
                    }
                }
                Console.WriteLine("Listening port set to " + listenPort);

                string url = "http://localhost:8090/";
                httpRequest = WebRequest.Create(url) as HttpWebRequest;

                // POST request
                httpRequest.Method = WebRequestMethods.Http.Post;
                httpRequest.ContentType = "application/json";
                string data = GetRequestPayload();
                httpRequest.ContentLength = data.Length;

                requestStream = httpRequest.GetRequestStream();
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                requestStream.Write(buffer, 0, buffer.Length);

                // GET request
                //httpRequest = WebRequest.Create(url) as HttpWebRequest;
                //httpRequest.Method = WebRequestMethods.Http.Get;

                httpResponse = httpRequest.GetResponse() as HttpWebResponse;
                Console.WriteLine("Response status: " + httpResponse.StatusCode.ToString());
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    using (Stream responseStream = httpResponse.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            data = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(data))
                            {
                                Console.WriteLine("Response data:   " + data);
                            }
                            reader.Close();
                        }
                        responseStream.Close();
                    }
                }
                StartListener();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (requestStream != null)
                {
                    requestStream.Close();
                }
                if (httpResponse != null)
                {
                    httpResponse.Close();
                }
            }
        }

        private static string GetRequestPayload()
        {
            JsonClientRequest jcr = new JsonClientRequest();
            jcr.address = "127.0.0.1";
            jcr.port = 11000;
            JsonVariables[] regv = new JsonVariables[1];
            regv[0] = new JsonVariables();
            regv[0].path = "BeckhoffOPC/Machine/SystemController";
            jcr.registervars = regv;
            jcr.unregistervars = null;

            return JsonConvert.SerializeObject(jcr, Formatting.Indented);
        }

        private static void StartListener()
        {
            UdpClient listener = new UdpClient(listenPort);
            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, listenPort);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");
                    byte[] bytes = listener.Receive(ref groupEP);

                    Console.WriteLine($"Received broadcast from {groupEP} :");
                    Console.WriteLine($" {Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                listener.Close();
            }
        }
    }
}
