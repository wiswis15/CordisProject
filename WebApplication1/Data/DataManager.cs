using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using MCDBackend.XmlConfig;
using Cordis.MdsiClientWrapper;

namespace WebApplication1.Data
{
    public class DataManager
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DataManager));

        private List<VariableObject> SystemVariables = new List<VariableObject>();
        private MdsiClientWrapper MdsiClientWrapper;

        private bool updateRequired = false;

        public DataManager()
        { 
        }

        public bool Initialize(string configfile)
        {
            MdsiClientWrapper = new MdsiClientWrapper();

            // Read XML file
            log.Info("Loading configuration from " + configfile);
            Configuration _config = null;
           _config = ConfigFileParser.ParseConfigFile(configfile);
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

                    VariableObject variableObject = new VariableObject(_config.server, var, MdsiClientWrapper);
                    if (variableObject.Initialize())
                    {
                        SystemVariables.Add(variableObject);
                        variableObject.ValueUpdated += OnVariableUpdated;
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
                return false;
            }
            return true;
        }

        private void OnVariableUpdated(object sender, EventArgs args)
        {
            // for now only store if an update is required for the frontend.
            // maby later we can make this more efficient.
            updateRequired = true;
        }

        public bool GetAndResetUpdateRequired()
        {
            var result = updateRequired;
            updateRequired = false;
            return result;
        }

        public List<CurrentVariableValue> GetCurrentValues()
        {
            var result = new List<CurrentVariableValue>();
            foreach ( var variable in SystemVariables )
            {
                result.Add(variable.FillCurrentValue());
            }
            return result;
        }

        public VariableObject getVariableObject(string name)
        {
            return SystemVariables.FirstOrDefault(e => e.IsVariable(name));
        }

        public void StartTimers()
        {
            // Start timer(s)
            log.Info("Start timers.");
            foreach (VariableObject variableObject in SystemVariables)
            {
                variableObject.StartTimer();
            }
        }

        public void Dispose()
        {
            foreach( var variable in SystemVariables)
            {
                variable.Dispose();
            }
            SystemVariables.Clear();
        }

    }
}
