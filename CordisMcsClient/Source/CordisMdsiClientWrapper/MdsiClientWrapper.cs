using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using FestaJsonConnectorSupport;
using log4net;
using Cordis.MdsiClientWrapper.Helpers;
using Cordis.Mdsi.Client.Dtos.Controller;
using Cordis.Mdsi.Client.Dtos.MachinePart;
using Cordis.Mdsi.Client.JsonConnectors;
using Cordis.Mdsi.Client.JsonConnectors.Controller;
using Cordis.Mdsi.Client.JsonConnectors.Logging;
using Cordis.Mdsi.Client.JsonConnectors.MachinePart;
using Cordis.Mdsi.Client.JsonStubs;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]

namespace Cordis.MdsiClientWrapper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class MdsiClientWrapper
    {
        private static readonly log4net.ILog LOG = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public readonly IMachinePartJsonConnector MachinePartConnector;
        public readonly IControllerJsonConnector ControllerConnector;
        public readonly ILoggingJsonConnector LoggingConnector;
        public readonly ClientConfiguration ClientConfiguration;
        private readonly MdsiClientWrapperImpl _clientWrapperImpl;
        private readonly Dictionary<string, bool> _savedConnectionDictionary = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _connectionDictionary = new Dictionary<string, bool>();
        public ServerConnectionStates _serverConnectionState = ServerConnectionStates.NotConnected;

        public List<ControllerRecord> Controllers { get; set; } = new List<ControllerRecord>();
        public enum ServerConnectionStates { NotConnected, Connected, Refreshing }
        public ServerConnectionStates ServerConnectionState
        {
            get => _serverConnectionState;
            set => _serverConnectionState = value;
        }

        // Timer settings
        private const int UpdateIntervalMili = 1000;    // Number of miliseconds between timer ticks
        private const int RefreshInterval = 7;          // Number of cycles before checking connection to server by rereading list of controllers
        private const int RefreshTimeoutInterval = 17;  // Number of cycles to wait before deciding the request timed out
        private const int ConnectInterval = 11;         // Number of cycles before testing connections to controllers
        private DispatcherTimer _timer;
        private int _refreshCounter = RefreshInterval;
        private int _refreshTimeoutCounter = RefreshTimeoutInterval;
        private int _connectCounter = ConnectInterval;

        class McsVersion
        {
            public readonly int major = 1;
            public readonly int minor = 0;
            public readonly int build = 55;
        }

        readonly McsVersion _expectedMcsVersion = new McsVersion();
        private string _serverVersion = "";
        private bool _serverVersionReported = false;

        public MdsiClientWrapper()
        {
            _clientWrapperImpl = new MdsiClientWrapperImpl();

            LOG.Warn("---------- MdsiClientWrapper started ----------");

            string clientConfigurationAddress = Directory.GetCurrentDirectory() + "\\" + "ClientConfiguration.xml";
            string errorMessage;
            ClientConfiguration = ClientConfiguration.LoadFromFile(clientConfigurationAddress, out errorMessage);

            try
            {
                JsonConnectorFactory.Instance = new JsonConnectorFactoryImpl(ClientConfiguration.CordisServerConfiguration.ServerAddress, HandleJsonFaultGlobal);
                JsonConnectorFactory.Instance.SecurityToken = ClientConfiguration.CordisServerConfiguration.SecurityToken;

                // Set up the machine part connector for querying the machine parts
                MachinePartConnector = JsonConnectorFactory.Instance.GetMachinePartConnector();

                // Set up the controller connector, so that the server can be queried for available controllers.
                ControllerConnector = JsonConnectorFactory.Instance.GetControllerConnector();

                // Set up the logging part connector for logging on the server
                LoggingConnector = JsonConnectorFactory.Instance.GetLoggingConnector();
            }
            catch (System.Exception ex)
            {
                LOG.Error("Error occured while connecting to the server (" + ex.Message + ")");
            }

            // Set the connectors on the tab controls
            // Initialize the tab with the connectors and controllers
            if (_clientWrapperImpl != null)
            {
                _clientWrapperImpl.SetConnector(MachinePartConnector);
            }

            // Start the query on the server
            try
            {
                // Validate the server version first, after that the controllers will be queried.
                ControllerConnector.GetServerVersion(OnGetServerVersion);
            }
            catch (System.Exception ex)
            {
                LOG.Error("Error reading controllers (" + ex.Message + ")");
            }

            // Start updating the tabs
            StartTimerUpdates();
        }

        private void OnGetServerVersion(string text)
        {
            bool invalidVersion = false;
            string newVersion = text;

            try
            {
                string[] splitText = text.Split('.');
                int major = Convert.ToInt32(splitText[0]);
                int minor = Convert.ToInt32(splitText[1]);
                int build = Convert.ToInt32(splitText[2]);

                newVersion = String.Format("v{0}.{1}.{2}", major, minor, build);

                if ((major < _expectedMcsVersion.major)
                    || ((major == _expectedMcsVersion.major)
                        && (minor < _expectedMcsVersion.minor))
                    || ((major == _expectedMcsVersion.major)
                        && (minor == _expectedMcsVersion.minor)
                        && (build < _expectedMcsVersion.build))
                    )
                {
                    invalidVersion = true;
                }
                else
                {
                    if (!_serverVersionReported)
                    {
                        _serverVersionReported = true;
                        LOG.Warn("Connected to MCS " + _serverVersion);
                    }
                }
            }
            catch (Exception exc)
            {
                LOG.Warn(String.Format("MCS {0}, Exception {1}\n{2}", _serverVersion, exc.Message, exc.StackTrace));
                invalidVersion = true;
            }

            // If we reconnect to another server, show the dialog once more even though we have already shown it
            if (invalidVersion && _serverVersion != "" && _serverVersion != newVersion)
            {
                _serverVersionReported = false;
            }

            if (invalidVersion && !_serverVersionReported)
            {
                _serverVersionReported = true;

                // The server apparently doesn't support the PerformAnyUseCase("GetServerVersion")
                LOG.Warn(String.Format("Invalid MCS version [{0}]", text));
            }

            _serverVersion = newVersion;

            // If user didn't decide to shutdown the dashboard after a potential incompatible MCS version, 
            // continue with checking the connected controllers.
            ControllerConnector.GetControllers(OnGetControllers);
        }

        /// <summary>
        /// Called when the server returns with the list of controllers
        /// </summary>
        /// <param name="obj"></param>
        private void OnGetControllers(IList<ControllerRecord> obj)
        {
            // In the callback, so there is a connection
            _serverConnectionState = ServerConnectionStates.Connected;
            LOG.Debug("Connected to: " + ClientConfiguration.CordisServerConfiguration.Description + " " + _serverVersion +
                " (" + ClientConfiguration.CordisServerConfiguration.ServerAddress + ")");

            if (obj == null) return;
            
            // Check if the list has changed
            bool listsAreEqual = true;

            if (Controllers.Count != obj.Count)
            {
                listsAreEqual = false;
            }
            else
            {
                List<ControllerRecord> temp = obj.ToList();
                
                foreach (ControllerRecord cr in Controllers)
                {
                    if (!temp.Contains(cr))
                    {
                        listsAreEqual = false;
                    }
                }
            }

            // If the list changed, update the combo box and tab controls
            if (!listsAreEqual)
            {
                Controllers.Clear();
                Controllers = obj.ToList();

                // Refresh the combo box and reset the list of saved statuses
                foreach (ControllerRecord cr in Controllers)
                {
                    CordisControllerRecord displayCR = new CordisControllerRecord();
                    displayCR.ControllerRecord = cr;
                    displayCR.MachineName = cr.Name;    // Temporary name

                    // Request the machine part tree with the server for displaying the machine name in the dropdown list
                    GetMachinePartTree(cr);
                }

                if (_clientWrapperImpl == null) return;
                
                // Update the wrapper implementation with the new list of controllers
                _clientWrapperImpl.SetControllers(Controllers);
                // Use the first one as current
                _clientWrapperImpl.CurrentControllerChanged(Controllers[0]);
            }
            else
            {
                // Although the list hasn't changed, still the name of the top-level machine part may have
            }
        }

        /// <summary>
        /// Retrieves the Machine Part Tree for setting the names in the Controllers combo box.
        /// </summary>
        private void GetMachinePartTree(ControllerRecord cr)
        {
            MachinePartConnector.GetMachinePartTree(cr.Name + "/", OnGetMachinePartTree);
        }

        /// <summary>
        /// Callback method for GetMachinePartTree().
        /// Adds the retrieved MachinePartDto tree to a TreeView on the GUI.
        /// </summary>
        /// <param name="machinePart">A MachinePartDto that serves as root. It contains other MachinePartDto's as children.</param>
        private void OnGetMachinePartTree(MachinePartDto machinePart)
        {
            if (machinePart == null || String.IsNullOrEmpty(machinePart.Name))
            {
                return;
            }

            // Update the name of the controller in the list
            foreach (ControllerRecord cr in Controllers)
            {
                if (!machinePart.FullPath.Contains(cr.Name)) continue;
                
                int pos = machinePart.FullPath.IndexOf('/');
                if (pos <= -1) continue;
                    
                string controllerName = cr.Name.Trim();
                string tlMp = machinePart.FullPath.Substring(pos + 1);
                //cr.Name = machinePart.FullPath.Substring(pos + 1);

                if (_clientWrapperImpl.ControllerName.ContainsKey(tlMp))
                {
                    _clientWrapperImpl.ControllerName[tlMp] = controllerName;
                }
                else
                {
                    _clientWrapperImpl.ControllerName.Add(tlMp, controllerName);
                }
            }
        }

        public void StartTimerUpdates()
        {
            if (_timer != null && !_timer.IsEnabled)
            {
                _timer.Start();
            }
            else if (_timer == null)
            {
                _timer = new DispatcherTimer(DispatcherPriority.Background);
                _timer.Tick += new EventHandler(UpdateTimer_Tick);
                _timer.Interval = new TimeSpan(0, 0, 0, 0, UpdateIntervalMili);
                _timer.Start();
            }
        }

        public void StopTimerUpdates()
        {
            if (_timer != null && _timer.IsEnabled)
            {
                _timer.Stop();
            }
        }

        /// <summary>
        /// Call the TimerTick function on each tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            switch (_serverConnectionState)
            {
                case ServerConnectionStates.NotConnected:
                    // Once in a while, try to connect to the server by requesting the list of controllers
                    if (_refreshCounter <= 0)
                    {
                        try
                        {
                            // Start the query
                            _serverConnectionState = ServerConnectionStates.Refreshing;
                            ControllerConnector.GetServerVersion(OnGetServerVersion);
                        }
                        catch
                        {
                        }

                        _refreshCounter = RefreshInterval;
                    }
                    else
                    {
                        _refreshCounter--;
                    }

                    break;

                case ServerConnectionStates.Connected:

                    // Check if the status of a controller has changed. If yes, process that first.
                    List<string> changedControllers = new List<string>();

                    foreach (KeyValuePair<string, bool> kvp in _connectionDictionary)
                    {
                        if (_savedConnectionDictionary.ContainsKey(kvp.Key))
                        {
                            if (_savedConnectionDictionary[kvp.Key] != kvp.Value)
                            {
                                _savedConnectionDictionary[kvp.Key] = kvp.Value;
                                changedControllers.Add(kvp.Key);
                                Debug.WriteLine(String.Format("Controller {0} has {1}", kvp.Key, kvp.Value ? "connected" : "disconnected"));
                            }
                        }
                        else if (!string.IsNullOrEmpty(kvp.Key))
                        {
                            // key not yet in saved dictionary, add it
                            _savedConnectionDictionary[kvp.Key] = kvp.Value;
                            changedControllers.Add(kvp.Key);
                            Debug.WriteLine(String.Format("Controller {0} added, it is {1}", kvp.Key, kvp.Value ? "connected" : "disconnected"));
                        }
                    }

                    // If controllers have changed their connection status, don't check the connection now.
                    if (changedControllers.Count == 0)
                    {
                        // Once in a while, check if the server is still connected by checking if the list of controllers has changed.
                        if (_refreshCounter <= 0)
                        {
                            try
                            {
                                // Start the query
                                _serverConnectionState = ServerConnectionStates.Refreshing;
                                ControllerConnector.GetServerVersion(OnGetServerVersion);
                            }
                            catch
                            {
                            }

                            // Delay checking the connection with the controllers
                            _connectCounter += 1;

                            _refreshCounter = RefreshInterval;
                        }
                        else
                        {
                            _refreshCounter--;
                        }

                        // Check if the connections to the controllers are still current
                        if (_connectCounter <= 0)
                        {
                            _serverConnectionState = ServerConnectionStates.Refreshing;
                            foreach (ControllerRecord cr in Controllers)
                            {
                                // Get current status
                                CheckConnection(cr);
                            }

                            _connectCounter = ConnectInterval;
                        }
                        else
                        {
                            _connectCounter--;
                        }
                    }

                    if (changedControllers.Count > 0)
                    {
                        foreach (ControllerRecord cr in Controllers)
                        {
                            if (changedControllers.Contains(cr.Name))
                            {
                                // Goal is to retrieve the controller name in order to update the controller names in the controllers combobox
                                // OnGetMachinePartTree takes care of this.
                                // ToDo: it is quite some overhead to retrieve the whole MP tree, if we only need the toplevel machinepart... :(
                                // pls. substitute this with a more lightweight method.
                                //GetMachinePartTree(cr);
                            }
                        }
                    }

                    // Give the wrapper class a timer tick
                    if (_clientWrapperImpl != null)
                    {
                        if (changedControllers.Count > 0)
                        {
                            // The wrapper implementation needs to be informed on the connected status of all controllers,
                            // not just of 'the selected' one. Simply inform about all controllers here
                            foreach (ControllerRecord cr in Controllers)
                            {
                                if (changedControllers.Contains(cr.Name))
                                {
                                    Debug.WriteLine(String.Format("Update wrapper because controller {0} was {1}", cr.Name, _connectionDictionary[cr.Name] ? "connected" : "disconnected"));
                                    _clientWrapperImpl.ConnectionStatusChanged(cr, _connectionDictionary[cr.Name]);
                                }
                            }
                        }

                        // If the current tabItem is the selected tab, call its TimerTick.
                        _clientWrapperImpl.TimerTick();
                    }

                    break;

                case ServerConnectionStates.Refreshing:
                    // After trying the refresh for a while, decide we are not connected
                    if (_refreshTimeoutCounter <= 0)
                    {
                        LOG.Debug(String.Format("Connected to: None (configured: {0})",
                            ClientConfiguration.CordisServerConfiguration.ServerAddress));
                        _serverConnectionState = ServerConnectionStates.NotConnected;

                        // Reset the saved connection status to force ConnectionStatusChanged and thus redraw on all controllers
                        _savedConnectionDictionary.Clear();
                        Controllers.Clear();

                        // Tell the tabs that there is no connection to the server
                        if (_clientWrapperImpl != null)
                        {
                            _clientWrapperImpl.ConnectionStatusChanged(null, false);
                        }

                        _refreshTimeoutCounter = RefreshTimeoutInterval;
                    }
                    else
                    {
                        _refreshTimeoutCounter--;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Checks if a specified controller is connected to the server.
        /// </summary>
        private void CheckConnection(ControllerRecord cr)
        {
            // Save the current connection status
            if (_connectionDictionary.TryGetValue(cr.Name, out var connected))
            {
                _savedConnectionDictionary[cr.Name] = connected;
            }

            MachinePartConnector.PerformAnyUseCase("CheckConnection," + cr.Name, OnCheckConnection);
        }

        /// <summary>
        /// Callback method for CheckConnection</summary>().
        /// </summary>
        private void OnCheckConnection(string text)
        {
            // A callback is being made, so we are connected
            _serverConnectionState = ServerConnectionStates.Connected;

            // The method CheckConnection on the server returns true if the Controller is connected and false if there is no connection.
            char[] delimiter = { '/' };
            string[] splitText = text.Split(delimiter, 2);

            string controllerName = splitText[0];
            bool controllerConnectedStatus = (String.Equals(splitText[1], true.ToString(), StringComparison.OrdinalIgnoreCase));

            if (splitText.Length == 2)
            {
                _connectionDictionary[controllerName] = controllerConnectedStatus;
            }
        }

        /// <summary>
        /// Callback function for Festa library
        /// </summary>
        /// <param name="faults"></param>
        private void HandleJsonFaultGlobal(IList<ExceptionInfo> faults)
        {
            string faultText = "";

            foreach (ExceptionInfo f in faults)
            {
                faultText += "\n" + f.Code + ": ";
                foreach (string arg in f.Args) faultText += ", " + arg;
            }

            LOG.Error("Server threw an exception:" + faultText);
        }

        public MdsiClientWrapperImpl GetClientWrapper()
        {
            return _clientWrapperImpl;
        }

        public delegate void UpdateTextCallback(IList<ExceptionInfo> faults);
    }
}
