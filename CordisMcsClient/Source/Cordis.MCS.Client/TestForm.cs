using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Cordis.Mdsi.Client.Dtos.MachinePart;
using Cordis.Mdsi.Client.Dtos.Controller;
using Cordis.Mdsi.Client.Dtos.MachinePart.Command;
using Cordis.Mdsi.Client.Dtos.MachinePart.Observer;
using Cordis.Mdsi.Client.Dtos.MachinePart.Setting;
using Cordis.Mdsi.Client.Dtos.MachinePart.StateMachine;
using FestaJsonConnectorSupport;

namespace Cordis.EnSoTestGUI
{
    public partial class TestForm : Form
    {
        private readonly MdsiClientWrapper.MdsiClientWrapper _wrapper;
        private string _controllerName;
        private int _lastSelectedIndex = -1;
        private readonly List<string> mpTree = new List<string>();
        private int mpTreeIndentation = 0;
        private string _machinePartName;
        private MachinePartPropertiesDto _mpProperties = new MachinePartPropertiesDto();

        //-------------------------------------------------------------------
        // Some generic Form stuff...

        public TestForm()
        {
            InitializeComponent();

            _wrapper = new MdsiClientWrapper.MdsiClientWrapper();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Show connection status
            connectionTextBox.Text = _wrapper.ServerConnectionState.ToString();

            if (controllersComboBox.Items.Count == 0 && _wrapper.Controllers != null &&
                _wrapper.ServerConnectionState == MdsiClientWrapper.MdsiClientWrapper.ServerConnectionStates.Connected)
            {              
                foreach (var c in _wrapper.Controllers)
                {
                    controllersComboBox.Items.Add(c.Name);
                }
                if (controllersComboBox.Items.Count > 0)
                {
                    controllersComboBox.Items.Add("<All>");
                    if (_lastSelectedIndex >= 0 && _lastSelectedIndex < controllersComboBox.Items.Count)
                    {
                        controllersComboBox.SelectedIndex = _lastSelectedIndex;
                    }
                    else
                    {
                        controllersComboBox.SelectedIndex = controllersComboBox.Items.Count - 1;
                    }
                }
            }
            else
            {
                // Check if the collection changed
                bool collectionChanged = false;
                if (_wrapper.Controllers != null && _wrapper.Controllers.Count != controllersComboBox.Items.Count - 1)
                {
                    collectionChanged = true;
                }
                else
                {
                    if (_wrapper.Controllers != null)
                        foreach (var c in _wrapper.Controllers)
                        {
                            if (!controllersComboBox.Items.Contains(c.Name))
                            {
                                collectionChanged = true;
                            }
                        }
                }

                if (collectionChanged)
                {
                    _lastSelectedIndex = controllersComboBox.SelectedIndex;
                    controllersComboBox.Items.Clear();
                }
            }

            // Update the messages
            _wrapper.GetClientWrapper().GetMessages(out var serverMessages, out var controllerMessages);

            txtBoxServerMessages.Text = "";
            foreach (string line in serverMessages)
            {
                txtBoxServerMessages.Text += line;
            }

            txtBoxControllerMessages.Text = "";
            foreach (string line in controllerMessages)
            {
                txtBoxControllerMessages.Text += line;
            }
        }

        private void ControllersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            if (comboBox.SelectedItem == null) return;
            
            _controllerName = ((string)comboBox.SelectedItem);
            txtBoxControllerMessages.Clear();
            // Use the first one as current
            ControllerRecord cr = new ControllerRecord
            {
                Name = _controllerName
            };
            _wrapper.GetClientWrapper().CurrentControllerChanged(cr);
        }

        //-------------------------------------------------------------------
        // Retrieve from the selected controller it's machinepart tree

        private void btnGetMachinePartTree(object sender, EventArgs e)
        {
            mpTree.Clear();

            try
            {
                _wrapper.MachinePartConnector.GetMachinePartTree(
                    controllersComboBox.Text + "/",
                    OnGetMachinePartTree
                );
            }
            catch (Exception ex)
            {
                txtBoxMpTree.Text = ex.Message + "'" + controllersComboBox.Text + "'";
            }

            testGetMpTree.Checked = false;
        }

        private void OnGetMachinePartTree(MachinePartDto machinePart)
        {
            txtBoxMpTree.Text = "";
            AddMpToTreeRecursive(machinePart);

            foreach (var node in mpTree)
                txtBoxMpTree.Text += node + "\r\n";
        }

        private void AddMpToTreeRecursive(MachinePartDto mp)
        {
            string indentation = "";
            const int indent = 4;
            for (var i = 0; i < mpTreeIndentation; i++) indentation += " ";
            mpTree.Add(indentation + mp.FullPath);

            mpTreeIndentation += indent;
            foreach (var machinePartChild in mp.Children) AddMpToTreeRecursive(machinePartChild);
            mpTreeIndentation -= indent;
        }

        //-------------------------------------------------------------------
        // Retrieve machineparts' properties (commands, settings, observers, statemachines)

        private void btnGetMpPropertiesClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxMachinePart.Text)) txtBoxMachinePart.Text = txtBoxMpTree.SelectedText.Trim();

            if (txtBoxMachinePart.Text == "")
            {
                status.Text =
                    "ERROR: machinepart may not be empty, select a full machinepart name from the machinepart tree.";
                return;
            }

            _wrapper.MachinePartConnector.GetMachinePartProperties(txtBoxMachinePart.Text, OnGetMachinePartProperties);
        }

        private void OnGetMachinePartProperties(MachinePartPropertiesDto machinePartProperties)
        {
            if (machinePartProperties == null)
            {
                // This indicates that the MachineControlServer's cache is empty or too old...
                // Retrieve the MP Properties again.
                _wrapper.MachinePartConnector.GetMachinePartProperties(txtBoxMachinePart.Text, OnGetMachinePartProperties);
            }
            if (machinePartProperties.MachinePart == null)
            {
                // This indicates that the used machinepart's name is incorrect...
                status.Text = "ERROR: Machinepart '" + txtBoxMachinePart.Text + "' is unknown.";
                return;
            }
            else
            {
                _mpProperties = machinePartProperties;

                txtBoxMpProp.Text =
                    machinePartProperties.MachinePart.FullPath + ":\r\n" +
                    " * Commands: " + machinePartProperties.Commands.Count() + "\r\n";
                foreach (var cmd in machinePartProperties.Commands) txtBoxMpProp.Text += "   - " + cmd.Name + "\r\n";

                txtBoxMpProp.Text += " * Observers: " + machinePartProperties.Observers.Count() + "\r\n";
                foreach (var obs in machinePartProperties.Observers)
                    txtBoxMpProp.Text += "   - " + obs.Name + " (" + obs.Type + ")\r\n";

                txtBoxMpProp.Text += " * Settings: " + machinePartProperties.Settings.Count() + "\r\n";
                foreach (var sett in machinePartProperties.Settings) txtBoxMpProp.Text += "   - " + sett.Name + " (" + sett.Type + ")\r\n";

                txtBoxMpProp.Text += " * Statemachines: " + machinePartProperties.StateMachines.Count() + "\r\n";
                foreach (var stat in machinePartProperties.StateMachines) txtBoxMpProp.Text += "   - " + stat.Name + "\r\n";

                UpdateObservers(machinePartProperties.MachinePart.FullPath);
                UpdateSettings(machinePartProperties.MachinePart.FullPath);
            }
        }

        //-------------------------------------------------------------------
        // Update all machineparts' observers and settings...

        private void UpdateObservers(string mpFullPath)
        {
            ObserverValueCriteria crit = new ObserverValueCriteria()
            {
                MachinePartRegEx = txtBoxMachinePart.Text,
                ObserverNameRegEx = "",
                TreeSearch = false,
            };

            _wrapper.MachinePartConnector.GetObserverValues(crit, OnGetObserverValues);
        }

        private void OnGetObserverValues(IObservableList<ObserverDto> obj)
        {
            txtBoxObservers.Text = "";

            foreach (ObserverDto observerDto in obj)
            {
                txtBoxObservers.Text += observerDto.Name + " = ";

                switch (observerDto)
                {
                    case BooleanObserverDto boolDto:
                        txtBoxObservers.Text += boolDto.CurrentValue;
                        break;
                    case IntObserverDto intDto:
                        txtBoxObservers.Text += intDto.CurrentValue;
                        break;
                    case FloatObserverDto floatDto:
                        txtBoxObservers.Text += floatDto.CurrentValue;
                        break;
                }

                string unit = string.IsNullOrEmpty(observerDto.Unit) ? "-" : observerDto.Unit;
                txtBoxObservers.Text += " [" + unit + "]\r\n";
            }
        }

        private void UpdateSettings(string machinePartFullPath)
        {
            _machinePartName = txtBoxMachinePart.Text.Trim();
            
            // Delete the controller's name from the full machinepart name
            if (_machinePartName.StartsWith(_controllerName)) 
                _machinePartName = _machinePartName.Substring(_machinePartName.IndexOf('/') + 1);
            
            SettingsCriteria crit = new SettingsCriteria()
            {
                ControllerName = _controllerName,
                SettingNameRegEx = "",
                MachinePartRegEx = "",
                Recursive = true,
            };

            _wrapper.MachinePartConnector.GetSettings(crit, OnGetSettingValues);
        }

        private readonly List<SettingDto> _machinePartSettings = new List<SettingDto>();
        private void OnGetSettingValues(IObservableList<SettingDto> controllerSettings)
        {
            // Since GetSettings cannot retrieve settings from a single machinepart, 
            // the whole machinepart tree's setting list is retrieved, and filtered on the required machinepart name.
            // (Cordis issue #884)
            _machinePartSettings.Clear();
            _machinePartSettings.AddRange(controllerSettings.Where(s => s.MachinePart.FullPath.Equals(_machinePartName)));

            txtBoxSettings.Text = "";

            foreach (SettingDto setting in _machinePartSettings)
            {
                txtBoxSettings.Text += setting.Name + " = ";

                switch (setting)
                {
                    case BooleanSettingDto boolSetting:
                        txtBoxSettings.Text += boolSetting.CurrentValue;
                        break;
                    case IntSettingDto intSetting:
                        txtBoxSettings.Text += intSetting.CurrentValue;
                        break;
                    case FloatSettingDto floatSetting:
                        txtBoxSettings.Text += floatSetting.CurrentValue;
                        break;
                    default:
                        txtBoxSettings.Text += "Not supported setting type";
                        break;
                }

                string unit = string.IsNullOrEmpty(setting.Unit) ? "-" : setting.Unit;
                txtBoxSettings.Text += " [" + unit + "]\r\n";
            }
        }

        //-------------------------------------------------------------------
        // Execute command...

        private void btnExeCommandClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxCommand.Text)) txtBoxCommand.Text = txtBoxMpProp.SelectedText.Trim();

            if (txtBoxCommand.Text == "")
            {
                status.Text = "ERROR: Command name may not be empty, select a command from the machinepart properties.";
                return;
            }

            CommandDto cmdDto;

            try
            {
                cmdDto = _mpProperties.Commands.Single(c => c.Name.Equals(txtBoxCommand.Text));
            }
            catch
            {
                status.Text = "ERROR: Command '" + txtBoxCommand.Text + "' not found in '" + txtBoxMachinePart.Text + "'";
                return;
            }

            try
            {
                _wrapper.MachinePartConnector.ExecuteCommandAsync(cmdDto, false).ContinueWith(task =>
                {
                    try
                    {
                        //_machPartPropChanged = 2;
                        OnExecuteCommand(task.Result);
                    }
                    catch
                    {
                        //AggregateException
                    }
                });
            }
            catch
            {
            }
        }
        private void OnExecuteCommand(CommandKeyDto commandKeyDto)
        {
        }

        private SettingDto _singleSetting;

        //-------------------------------------------------------------------
        // Read / Write single setting...

        private void btnReadSingleSettingClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxSetting.Text)) txtBoxSetting.Text = txtBoxSettings.SelectedText.Trim();

            if (txtBoxSetting.Text == "")
            {
                status.Text =
                    "ERROR: Setting name may not be empty, select a setting from the machinepart setting list.";
                return;
            }

            try
            {
                _singleSetting = _mpProperties.Settings.Single(s => s.Name.Equals(txtBoxSetting.Text));
            }
            catch
            {
                status.Text =
                    "ERROR: Setting '" + txtBoxSetting.Text + "' not found in '" + txtBoxMachinePart.Text + "'";
                return;
            }

            try
            {
                SettingsCriteria crit = new SettingsCriteria()
                {
                    ControllerName = _controllerName,
                    SettingNameRegEx = "",
                    MachinePartRegEx = "",
                    Recursive = true,
                };

                _wrapper.MachinePartConnector.GetSettings(crit, OnGetSingleSetting);
            }
            catch (Exception ex)
            {
                status.Text =
                    "ERROR: GetSettings reported '" + ex.Message + "'";
            }
        }

        private void OnGetSingleSetting(IObservableList<SettingDto> controllerSettings)
        {
            SettingDto readSetting = controllerSettings.Single(
                s => _singleSetting.MachinePart.FullPath.Contains(s.MachinePart.FullPath) &&
                     s.Name.Equals(_singleSetting.Name));

            switch (readSetting)
            {
                case BooleanSettingDto boolSetting:
                    txtBoxSettingValue.Text = boolSetting.CurrentValue.ToString();
                    break;
                case IntSettingDto intSetting:
                    txtBoxSettingValue.Text = intSetting.CurrentValue.ToString();
                    break;
                case FloatSettingDto floatSetting:
                    txtBoxSettingValue.Text = floatSetting.CurrentValue.ToString();
                    break;
                default:
                    txtBoxSettingValue.Text = "unsupported setting type " + readSetting.Type;
                    break;
            }
        }

        private void btnWriteSingleSettingClick(object sender, EventArgs e)
        {
            _wrapper.GetClientWrapper().SetSettingValue(_singleSetting, txtBoxSettingValue.Text);
            _wrapper.MachinePartConnector.GetMachinePartProperties(txtBoxMachinePart.Text, OnGetMachinePartProperties);
        }

        //-------------------------------------------------------------------
        // Retrieve Statemachine history

        private void btnStatemachineHistoryClick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtBoxStatemachineName.Text)) txtBoxStatemachineName.Text = txtBoxMpProp.SelectedText.Trim();
            string statemachineName = txtBoxStatemachineName.Text;

            if (statemachineName == "")
            {
                status.Text = "ERROR: Statemachine name may not be empty, select a Statemachine from the machinepart properties.";
                return;
            }

            StateMachineDto statemachineDto;

            try
            {
                statemachineDto = _mpProperties.StateMachines.Single(c => c.Name.Equals(statemachineName));
            }
            catch
            {
                status.Text = "ERROR: Statemachine '" + statemachineName + "' not found in '" + statemachineName + "'";
                return;
            }

            var criteria = new StateMachineEventCriteria
            {
                MachinePartRegEx = statemachineDto.MachinePart.FullPath,
                StateMachineNameRegEx = statemachineDto.Name
            };

            _wrapper.MachinePartConnector.GetStateMachineEvents(criteria, OnGetStateMachineEvents);
        }

        private void OnGetStateMachineEvents(IObservableList<StateMachineEventRecord> stateMachineEventRecords)
        {
            txtBoxStatemachineHistory.Text = "";

            foreach (StateMachineEventRecord record in stateMachineEventRecords)
            {
                TimeSpan duration = record.Duration != null
                    ? TimeSpan.FromMilliseconds((long) record.Duration)
                    : TimeSpan.Zero;
                DateTime entryTime = record.TimeStamp != null
                    ? record.TimeStamp.Value
                    : DateTime.Now;

                string recordText = record.StateName + " / " + record.SubStateName + " - " + duration + " - " + entryTime + "\r\n";
                txtBoxStatemachineHistory.Text += recordText;
            }
        }

        //-------------------------------------------------------------------
        // Plug-in commands

        private void btnPlugInCommand(object sender, EventArgs e)
        {
            _wrapper.MachinePartConnector.PerformPluginUseCase(
                "MyCordisServerPlugIn",
                txtBoxMachinePart.Text,
                comBoxPluginCmd.SelectedItem.ToString(),
                txtBoxPlugInCmdArguments.Text,
                "YOUniQueT0ken",
                OnPerformPluginUseCase,
                PerformPluginUseCaseFaultHandler);
        }

        private void PerformPluginUseCaseFaultHandler(IList<ExceptionInfo> faults)
        {
            status.Text = "Plugin command exception: " + faults[0].Code;
        }

        private void OnPerformPluginUseCase(string PlugInResponse)
        {
            status.Text = "PlugIn command returned: " + PlugInResponse;
        }


        //-------------------------------------------------------------------
        // Textbox trim methods

        private void resultsTextBox_TextChanged(object sender, EventArgs e)
        {

        }
        private void txtBoxMachinePart_TextChanged(object sender, EventArgs e)
        {
            txtBoxMachinePart.Text = txtBoxMachinePart.Text.Trim();
        }
        private void txtBoxProperty_TextChanged(object sender, EventArgs e)
        {
            txtBoxCommand.Text = txtBoxCommand.Text.Trim();
        }
        private void txtBoxSetting_TextChanged(object sender, EventArgs e)
        {
            txtBoxSetting.Text = txtBoxSetting.Text.Trim();
        }
        private void txtBoxSettingValue_TextChanged(object sender, EventArgs e)
        {
            txtBoxSettingValue.Text = txtBoxSettingValue.Text.Trim();
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            comBoxPluginCmd.Items.Add("GetControllers");
            comBoxPluginCmd.Items.Add("GetFullMachinePartName");
            comBoxPluginCmd.Items.Add("GetMachinePartInterface");
            comBoxPluginCmd.Items.Add("GetRadInterface");
            comBoxPluginCmd.Items.Add("GetRadDictionary");
            comBoxPluginCmd.Items.Add("SubscribeRadUpdates");
            comBoxPluginCmd.Items.Add("UnsubscribeRadUpdates");
            comBoxPluginCmd.SelectedItem = comBoxPluginCmd.Items[0];
        }
    }
}
