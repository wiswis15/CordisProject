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
using System.Linq;
using Cordis.MdsiClientWrapper;
using log4net;
using MCDBackend.McsClient;
using MCDBackend.XmlConfig;

namespace MCDBackend
{
    class MainClass
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MainClass));

        private static readonly List<VariableObject> _variableList = new List<VariableObject>();
        private static Configuration _config = null;
        private static string _inputFilename = "BackendConfig.xml";
        private static MdsiClientWrapper _wrapper;

        public delegate void McdClientDelegate(JsonClientRequest request);

        public static void Main(string[] args)
        {
            // Instantiate the delegate.
            McdClientDelegate mcdClientHandler = DelegateMethod;
            _wrapper = new MdsiClientWrapper();

            // Read command line arguments (input filename)
            if (args.Count() == 1 && !string.IsNullOrEmpty(args[0]))
            {
                _inputFilename = args[0];
            }

            //Number of variables
            int numberOfVariables = 0;

            // Read XML file
            log.Info("Loading configuration from " + _inputFilename);
            _config = ConfigFileParser.ParseConfigFile(_inputFilename);
            if (_config != null)
            {
                log.Info("MCS Server: " + _config.server.address + ":" + _config.server.port);
                log.Info(_config.variable.Count.ToString() + " variable(s) found");
                foreach (Variable var in _config.variable)
                {
                    log.Info("------------------------------------");
                    log.Info("ObserverName:   " + var.observername);
                    log.Info("ControllerName: " + var.controllername);
                    log.Info("MachineName:    " + var.machinename);
                    log.Info("MachinePart:    " + var.machinepart);
                    log.Info("Interval:       " + var.interval);

                    VariableObject variableObject = new VariableObject(_config.server, var, _wrapper);
                    if (variableObject.Initialize())
                    {
                        _variableList.Add(variableObject);
                        numberOfVariables++;
                    }
                    else
                    {
                        log.Fatal("Unable to initialize variable " + "/" + var.controllername + "/" + var.machinename
                         + "/" + var.machinepart + "/" + var.observername + " correctly, removing variable from list.");
                        variableObject.Dispose();
                        variableObject = null;
                    }
                }
                log.Info("------------------------------------");
            }
            else
            {
                log.Fatal("Error loading configuration, stopping backend.");
                System.Environment.Exit(-1);
            }

            // Start frontend server (listening to frontend requests)
            McdClientListener myServer = new McdClientListener(8090, mcdClientHandler);
            log.Info("ClientListener is running on this port: " + myServer.Port.ToString());

            // Start timer(s)
            log.Info("Start timers.");
            foreach (VariableObject variableObject in _variableList)
            {
                variableObject.StartTimer();
            }

            // variable list is empty -->something went wrong, mostly the server has not started yet or bad connection
            if (numberOfVariables==0)
            {
                log.Fatal("No variables initiated, is the MCSServer up and running??");
                log.Fatal("Application needs to shut down!");
                log.Info("Please Press any key");
                Console.ReadKey();
                System.Environment.Exit(-1);
            }

              

            // Keep backend running (till quit command is given)
            ConsoleKeyInfo name;
            log.Info("Press 'Q' to quit");
            do
            {
                name = Console.ReadKey();
            } while (!name.KeyChar.Equals('Q') && !name.KeyChar.Equals('q'));
            log.Info("\n'" + name.KeyChar + "' pressed, stopping application.");

            // Clean up objects (stop timers and close database connections)
            foreach (VariableObject variableObject in _variableList)
            {
                variableObject.Dispose();
            }
           _variableList.Clear();
           myServer.Stop();
        }

        // Create a method for a delegate.
        public static void DelegateMethod(JsonClientRequest request)
        {
            Console.WriteLine("Delegate method called with message: " + request.ToString());
            foreach (JsonVariables variable in request.registervars)
            {
                string path = variable.path;
                foreach (VariableObject vo in _variableList)
                {
                    if (vo.IsVariable(path))
                    {
                        vo.AddClient(request.address, request.port);
                    }
                }
            }
        }
    }
}
