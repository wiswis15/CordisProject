using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using log4net;
using Cordis.Mdsi.Client.Dtos.Controller;
using Cordis.Mdsi.Client.Dtos.MachinePart;
using Cordis.Mdsi.Client.Dtos.MachinePart.Command;
using Cordis.Mdsi.Client.Dtos.MachinePart.Message;
using Cordis.Mdsi.Client.Dtos.MachinePart.Observer;
using Cordis.Mdsi.Client.Dtos.MachinePart.Setting;
using Cordis.Mdsi.Client.Dtos.MachinePart.TypedValue;
using Cordis.Mdsi.Client.Enums;
using Cordis.Mdsi.Client.JsonConnectors.MachinePart;
using Cordis.MdsiClientWrapper.Helpers;

//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]

namespace Cordis.MdsiClientWrapper
{
    public enum ArgumentType { Bool, Int, Double };

    // Type used for transporting command arguments over ClientWrapper functions
    public class Argument
    {
        public string name;
        public ArgumentType type;
        public bool boolValue;
        public long longValue;
        public double doubleValue;
    }

    /// <summary>
    /// A custom class for caching MachinePart data.
    /// </summary>
    public class MachinePartTreeItem
    {
        public string ItemName { get; set; }
        public string FullPath { get; set; }
        public object Dto { get; set; }
        public bool CacheData { get; set; }   // Defines if we cache the data of the MachinePart
        public ObservableCollection<MachinePartTreeItem> ChildItems { get; set; }
        public List<ObserverDto> Observers { get; set; }
        public List<CommandDto> Commands { get; set; }
        public List<SettingDto> Settings { get; set; }

        public MachinePartTreeItem()
        {
            this.ChildItems = new ObservableCollection<MachinePartTreeItem>();
        }
    }


    /// <summary>
    /// Interaction logic for BrowserTab.xaml
    /// </summary>
    public partial class MdsiClientWrapperImpl
    {
        private static readonly log4net.ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IMachinePartJsonConnector MachinePartConnector { get { return _connector; } set { _connector = value; } }
        public bool ShowHistoricalMessages { get; set; }

        // Fields related to controllers.
        private IMachinePartJsonConnector _connector;
        private List<ControllerRecord> _controllers;
        private string _currentControllerName = "";
        private int _numConnectedControllers;
        private Dictionary<string, bool> _connectionDictionary = new Dictionary<string, bool>();
        MachinePartTreeItem _machinePartTreeRootItem;
        public Dictionary<string, string> ControllerName = new Dictionary<string, string>();

        // Fields related to arguments.
        private bool _argumentValueIsValid = false;
        private bool _argumentValueIsSame = true;
        private string _currentArguments = "";

        // Fields related to messages
        private readonly List<string> _formattedServerMessages = new List<string>();
        private readonly List<string> _formattedControllerMessages = new List<string>();
        private static readonly Object _getMessagesLock = new Object();
        private int? _messagesHash = -1;
        private bool _messagesChanged = false;

        private readonly Dictionary<string, string> _pluginResultsDictionary = new Dictionary<string, string>();
        private int? _machPartPropHash = null;
        private bool _machPartPropChanged = false;

        public MdsiClientWrapperImpl()
        {
            FileInfo file = new FileInfo("log4net.xml");
            if (!file.Exists)
            {
                //MessageBox.Show("Log4net.xml not found");
            }
            else
            {
                log4net.Config.XmlConfigurator.Configure(file);
            }
        }

        #region Methods an MCD tab must implement

        public void SetConnector(IMachinePartJsonConnector machinePartJsonConnector)
        {
            _connector = machinePartJsonConnector;
        }

        public void SetControllers(List<ControllerRecord> controllers)
        {
            ControllerRecord currentCr = new ControllerRecord();

            _controllers = controllers;

            try
            {
                foreach (ControllerRecord controller in _controllers)
                {
                    // Remember the cr for the log message
                    currentCr = controller;

                    //GetMachinePartTree(controller);

                    // The first one will become the active one
                    if (_numConnectedControllers == 0)
                    {
                        _currentControllerName = controller.Name;
                    }

                    // Do the administration
                    _connectionDictionary[controller.Name] = false;
                    _numConnectedControllers++;
                }
            }
            catch
            {
                Logger.WarnFormat("Error occured while receiving data on controller: " + currentCr.Name);
            }
        }

        public void CurrentControllerChanged(ControllerRecord currentController)
        { 
            if (currentController == null || string.IsNullOrEmpty(currentController.Name)) return;
            _currentControllerName = currentController.Name;
            Debug.WriteLine("Controller changed to: " + _currentControllerName);
            // Refresh the tree
            GetMachinePartTree(currentController);
        }

        public void ConnectionStatusChanged(ControllerRecord controller, bool connected)
        {
            if (controller == null) return;
            
            // Do the administration
            _connectionDictionary[controller.Name] = connected;

            if (controller.Name != _currentControllerName) return;
            
            // If the currently selected Controller goes offline, we want to clear the lists.
            if (connected)
            {
                // Refresh the tree (also when disconnected, to show that)
                GetMachinePartTree(controller);
            }
            else
            {
                _machinePartTreeRootItem = null;
            }
        }

        public void PerformPluginUseCase(string plugin, string fullPath, string useCase, string arguments, string token)
        {
            _connector.PerformAnyUseCase(
                plugin + "," +
                token + "," +
                useCase + "," +
                fullPath + "," +
                arguments,
                OnPerformPluginUseCase);
        }

        private void OnPerformPluginUseCase(string input)
        {
            // Find the token on the first position
            char[] delimiters = { ',' };
            const int maxNumOfSplitStrings = 2;
            string[] splitInput = input.Split(delimiters, maxNumOfSplitStrings);

            if (splitInput.Length == 2)
            {
                _pluginResultsDictionary[splitInput[0]] = splitInput[1];
            }
            else
            {
                MessageBox.Show("Callback from unidentified function. Return value: " + input);
            }
        }

        public bool GetPluginUseCaseResult(string token, out string result)
        {
            bool success = _pluginResultsDictionary.TryGetValue(token, out var value);
            
            result = value;

            return success;
        }

        public void TimerTick()
        {
            // Check if any machine part properties changed
            if (_machPartPropChanged)
            {
                _machPartPropChanged = false;

                // Update Commands, Settings, Observers
                RefreshMachinePartProperties();
            }
            else
            {
                // Get the hash
                _connector.PerformAnyUseCase("GetMachinePartPropertiesHash," + "/", OnGetMachinePartPropertiesHash);
            }

            // Update the observers separately from the rest of the MachinePartProperties (because changes in observers don't change the hash)
            UpdateObserverValues();

            // Check if any messages changed
            if (_messagesChanged)
            {
                _messagesChanged = false;

                GetAllMessages();
            }
            else
            {
                // Get the hash. The server expects as input: GetMessagesHash,<MachinePartRegEx>,<TreeSearch>,<Cleared>
                _connector.PerformAnyUseCase("GetMessagesHash," + _currentControllerName + "/,True,False", OnGetMessagesHash);
            }
        }

        #endregion

        /// <summary>
        /// Retrieves the Machine Part Tree for the User Interface.
        /// </summary>
        private void GetMachinePartTree(ControllerRecord cr)
        {
            _connector.GetMachinePartTree(cr.Name + "/", OnGetMachinePartTree);
        }

        /// <summary>
        /// Callback method for GetMachinePartTree().
        /// Adds the retrieved MachinePartDto tree to a tree of MachinePartTreeItems.
        /// </summary>
        /// <param name="machinePart">A MachinePartDto that serves as root. It contains other MachinePartDto's as children.</param>
        private void OnGetMachinePartTree(MachinePartDto machinePart)
        {
            if (machinePart == null || string.IsNullOrEmpty(machinePart.Name))
            {
                return;
            }

            _machinePartTreeRootItem = AddMachinePartDtoToMachinePartTree(machinePart);
        }

        /// <summary>
        /// Recursive method that converts a MachinePartDto to a MachinePartTreeItem (for adding it to the MachinePartsTreeView later).
        /// </summary>
        /// <param name="machinePart">The Machine Part that has to be added to the MachinePartsTreeView.</param>
        /// <returns>A MachinePartTreeItem that can be added to the MachinePartsTreeView.</returns>
        private MachinePartTreeItem AddMachinePartDtoToMachinePartTree(MachinePartDto machinePart)
        {
            var result = new MachinePartTreeItem { ItemName = machinePart.Name, Dto = machinePart, FullPath = machinePart.FullPath, CacheData = false };

            foreach (var child in machinePart.Children)
            {
                result.ChildItems.Add(AddMachinePartDtoToMachinePartTree(child));
            }

            return result;
        }

        private void OnGetMessages(IList<MessageRecord> records)
        {
            Monitor.TryEnter(_getMessagesLock);
            try
            {
                _formattedServerMessages.Clear();
                _formattedControllerMessages.Clear();

                List<MessageRecord> sortedRecords = new List<MessageRecord>(records.OrderByDescending(o => o.TimeStamp).ToList());

                foreach (var messageRecord in sortedRecords)
                {
                    string messageText = "";

                    if (messageRecord.TimeStamp != null)
                    {
                        DateTime dt = (DateTime)messageRecord.TimeStamp;
                        messageText += string.Format("{0:D4}-{1:D2}-{2:D2} {3:D2}:{4:D2}:{5:D2}.{6:D3}",
                            dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
                    }

                    messageText += " | " + messageRecord.Category;
                    messageText += " | " + messageRecord.MachinePartFullPath;
                    messageText += " | " + messageRecord.MessageText;
                    messageText += " | (" + messageRecord.Parameter + ")\r\n";

                    if (messageRecord.Cleared.HasValue && messageRecord.Cleared == true)
                    {
                        _formattedControllerMessages.Add(messageText);
                    }
                    else
                    {
                        switch (messageRecord.Category)
                        {
                            case MessageCategoryEnum.ERROR:
                            case MessageCategoryEnum.WARNING:
                                _formattedControllerMessages.Add(messageText);
                                break;
                            case MessageCategoryEnum.INFO:
                                _formattedServerMessages.Add(messageText);
                                break;
                            case MessageCategoryEnum.UNDEFINED:
                            case MessageCategoryEnum.STOP_CYCLE:
                            case MessageCategoryEnum.LOG:
                            case MessageCategoryEnum.LOG_DETAILED:
                            case MessageCategoryEnum.IGNORE:
                            default:
                                _formattedControllerMessages.Add(messageText);
                                break;
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                Monitor.Exit(_getMessagesLock);
            }
        }

        /// <summary>
        /// Callback method for GetMachinePartPropertiesHash
        /// </summary>
        /// <param name="input">The hash retrieved from the server.</param>
        private void OnGetMessagesHash(string input)
        {
            // Check if the hash changed. If yes, get the MachinePartProperties from the server
            int messagesHash = 0;
            try
            {
                messagesHash = Convert.ToInt32(input);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception: ", ex);
            }

            if (messagesHash != _messagesHash || input == "")
            {
                _messagesHash = messagesHash;

                _messagesChanged = true;
            }
        }

        /// <summary>
        /// Callback method for GetMachinePartProperties(string fullname, Action resultHandler).
        /// Adds the Commands, Settings, Observers and StateMachines to their respective TreeViews.
        /// </summary>
        /// <param name="machinePartProperties">A MachinePartPropertiesDto object that containing lists of Commands, Settings, Observers and StateMachines.</param>
        private void OnGetMachinePartProperties(MachinePartPropertiesDto machinePartProperties)
        {
            if (machinePartProperties == null || machinePartProperties.MachinePart == null)
            {
                return;
            }

            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, UtilityFunctions.GetMachinePartPath(machinePartProperties.MachinePart.FullPath));

            if (item == null)
            {
                return;
            }

            // Copy the commands from the MachinePartPropertiesDto to the MachinePartTreeItem
            if (item.Commands == null)
            {
                item.Commands = new List<CommandDto>();
            }
            else
            {
                item.Commands.Clear();
            }

            foreach (var command in machinePartProperties.Commands)
            {
                item.Commands.Add(command);
            }

            // Copy the settings from the MachinePartPropertiesDto to the MachinePartTreeItem
            if (item.Settings == null)
            {
                item.Settings = new List<SettingDto>();
            }
            else
            {
                item.Settings.Clear();
            }

            foreach (var setting in machinePartProperties.Settings)
            {
                item.Settings.Add(setting);
            }
        }

        /// <summary>
        /// Callback method for GetMachinePartPropertiesHash
        /// </summary>
        /// <param name="input">The hash retrieved from the server.</param>
        private void OnGetMachinePartPropertiesHash(string input)
        {
            // Check if the hash changed. If yes, get the MachinePartProperties from the server
            int machPartPropHash = 0;
            try
            {
                machPartPropHash = Convert.ToInt32(input);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception: ", ex);
            }

            if (machPartPropHash != _machPartPropHash)
            {
                _machPartPropHash = machPartPropHash;

                _machPartPropChanged = true;
            }
        }

        private void UpdateObserverValues()
        {
            UpdateObserverValues(_machinePartTreeRootItem);
        }

        private void UpdateObserverValues(MachinePartTreeItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.CacheData)
            {
                var input = new ObserverValueCriteria();
                input.MachinePartRegEx = item.FullPath;
                input.RowLimit = 0;
                input.TreeSearch = false;
                _connector.GetObserverValues(input, OnGetObserverValues);
            }
            
            // Refresh the children
            foreach (var childItem in item.ChildItems)
            {
                UpdateObserverValues(childItem);
            }
        }

        private void OnGetObserverValues(IList<ObserverDto> observerDtos)
        {
            MachinePartTreeItem item = null;

            foreach (var observerDto in observerDtos)
            {
                // The first cycle we look for the correct MachinePartTreeItem and initialize it
                if (item == null)
                {
                    item = FindItem(_machinePartTreeRootItem, UtilityFunctions.GetMachinePartPath(observerDto.MachinePart.FullPath));
                    if (item == null)
                    {
                        return;
                    }

                    if (item.Observers == null)
                    {
                        item.Observers = new List<ObserverDto>();
                    }
                    else
                    {
                        item.Observers.Clear();
                    }
                }

                item.Observers.Add(observerDto);
            }
        }

        private void GetAllMessages()
        {
            if (ControllerName.ContainsValue(_currentControllerName))
            {
                var messageCriteria = new MessageCriteria
                {
                    MachinePartRegEx = _currentControllerName,
                    TreeSearch = true,
                    Cleared = false,
                    Source = MessageSourceEnum.All,
                };

                _connector.GetMessages(messageCriteria, OnGetMessages);
            }
        }

        /// <summary>
        /// Helper method to force the Server to update the values of the machinepart that was selected in MachinePartTreeList.
        /// </summary>
        private void RefreshMachinePartProperties()
        {
            RefreshMachinePartProperties(_machinePartTreeRootItem);
        }

        private void RefreshMachinePartProperties(MachinePartTreeItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item.CacheData)
            {
                _connector.GetMachinePartProperties(item.FullPath, OnGetMachinePartProperties);
            }
            
            // Refresh the children
            foreach (var childItem in item.ChildItems)
            {
                RefreshMachinePartProperties(childItem);
            }
        }
        
        #region Commands and Arguments

        private void SaveArgumentValue(CommandDto commandDto, ArgumentDto argumentDto)
        {
            var argumentKeyDto = new ArgumentKeyDto();
            argumentKeyDto.ArgumentName = argumentDto.Name;
            argumentKeyDto.CommandName = commandDto.Name;
            argumentKeyDto.MachinePartFullPath = commandDto.MachinePart.FullPath;
            _connector.SaveCurrentValueForArgument(argumentKeyDto, OnSaveValueArgument);
        }

        private void OnSaveValueArgument()
        {
            RefreshMachinePartProperties();
        }

        private void ExecuteCommand(CommandDto commandDto)
        {
            if (commandDto is CommandDto)
            {
                _connector.ExecuteCommand(commandDto, OnExecuteCommand);

                Logger.WarnFormat("{0} Cmd.[{1}] executed (Args: {2})", UtilityFunctions.GetMachinePartPath(commandDto.MachinePart.FullPath), commandDto.Name, _currentArguments);
            }
        }

        private void OnExecuteCommand(CommandKeyDto commandKeyDto)
        {

        }

        private void SetArgumentValue(CommandDto commandDto, ArgumentDto argumentDto, string value)
        {
            if (_argumentValueIsSame) return;

            if (_argumentValueIsValid)
            {
                try
                {
                    if (argumentDto is BooleanArgumentDto)
                    {
                        var specificArgumentDto = argumentDto as BooleanArgumentDto;

                        if (value.Equals(specificArgumentDto.FalseText))
                        {
                            specificArgumentDto.CurrentValue = false;

                            var newValue = new BooleanTypedValueDto();
                            newValue.BooleanValue = specificArgumentDto.CurrentValue;
                            newValue.Type = TypedValueTypeEnum.BOOLEAN_TYPED_VALUE;
                            SetTypedArgumentValue(commandDto, argumentDto, newValue);
                        }
                        else if (value.Equals(specificArgumentDto.TrueText))
                        {
                            specificArgumentDto.CurrentValue = true;

                            var newValue = new BooleanTypedValueDto();
                            newValue.BooleanValue = specificArgumentDto.CurrentValue;
                            newValue.Type = TypedValueTypeEnum.BOOLEAN_TYPED_VALUE;
                            SetTypedArgumentValue(commandDto, argumentDto, newValue);
                        }
                        else
                        {
                            specificArgumentDto.CurrentValue = Boolean.Parse(value);

                            var newValue = new BooleanTypedValueDto();
                            newValue.BooleanValue = specificArgumentDto.CurrentValue;
                            newValue.Type = TypedValueTypeEnum.BOOLEAN_TYPED_VALUE;
                            SetTypedArgumentValue(commandDto, argumentDto, newValue);
                        }
                    }
                    else if (argumentDto is IntArgumentDto)
                    {
                        var specificArgumentDto = argumentDto as IntArgumentDto;
                        specificArgumentDto.CurrentValue = long.Parse(value);

                        if (specificArgumentDto.IntType == IntTypeEnum.INT16
                            || specificArgumentDto.IntType == IntTypeEnum.INT32 
                            || specificArgumentDto.IntType == IntTypeEnum.INT64)
                        {
                            var newValue = new IntTypedValueDto();
                            newValue.IntValue = specificArgumentDto.CurrentValue;
                            newValue.Type = TypedValueTypeEnum.INT_TYPED_VALUE;
                            SetTypedArgumentValue(commandDto, argumentDto, newValue);
                        }
                        //else if ()
                        //{
                        //    Int64TypedValueDto newValue = new Int64TypedValueDto();
                        //    newValue.Int64Value = specificArgumentDto.CurrentValue;
                        //    newValue.Type = TypedValueTypeEnum.INT_64T_YPED_VALUE;
                        //    SetTypedArgumentValue(commandDto, argumentDto, newValue);
                        //}
                    }
                    else if (argumentDto is FloatArgumentDto)
                    {
                        var specificArgumentDto = argumentDto as FloatArgumentDto;
                        specificArgumentDto.CurrentValue = Double.Parse(value, NumberStyles.Float, CultureInfo.CurrentCulture);

                        var newValue = new FloatTypedValueDto();
                        newValue.FloatValue = specificArgumentDto.CurrentValue;
                        newValue.Type = TypedValueTypeEnum.FLOAT_TYPED_VALUE;
                        SetTypedArgumentValue(commandDto, argumentDto, newValue);

                    }
                    else if (argumentDto is StringArgumentDto)
                    {
                        var specificArgumentDto = argumentDto as StringArgumentDto;
                        specificArgumentDto.CurrentValue = value;

                        var newValue = new StringTypedValueDto();
                        newValue.StringValue = specificArgumentDto.CurrentValue;
                        newValue.Type = TypedValueTypeEnum.STRING_TYPED_VALUE;
                        SetTypedArgumentValue(commandDto, argumentDto, newValue);
                    }
                }
                catch (FormatException ex)
                {
                    Logger.WarnFormat(string.Format("{0} '{1}'", ex.Message, value), "Client Wrapper");
                }
                catch (Exception ex)
                {
                    Logger.WarnFormat(string.Format("Exception: '{0}'", ex.Message), "Client Wrapper");
                }
            }
            else
            {
                _machPartPropChanged = true; // force a refresh of all machinepartproperties
            }

            _argumentValueIsSame = true;
            _argumentValueIsValid = false;
        }

        private void SetTypedArgumentValue(CommandDto commandDto, ArgumentDto argumentDto, TypedValueDto newValue)
        {
            var argumentKey = new ArgumentKeyDto
            {
                ArgumentName = argumentDto.Name,
                CommandName = commandDto.Name,
                MachinePartFullPath = commandDto.MachinePart.FullPath
            };

            _connector.SetCurrentValueForArgument(argumentKey, newValue, OnUpdateArgumentValues);
            Logger.WarnFormat("Argument {0}/{1}/{2} set to {3} [{4}]",
                commandDto.MachinePart.FullPath, commandDto.Name, argumentDto.Name, UtilityFunctions.Value(newValue), argumentDto.Unit);
        }

        private void OnUpdateArgumentValues()
        {
            RefreshMachinePartProperties();
        }

        #endregion

        // Handlers and methods for checking and changing ActualValue for a Setting.
        #region Settings

        public void SetSettingValue(SettingDto settingDto, string valueText)
        {
            try
            {
                if (settingDto is BooleanSettingDto)
                {
                    var specificSettingDto = (BooleanSettingDto)settingDto;
                    if (valueText.Equals(specificSettingDto.FalseText))
                    {
                        specificSettingDto.CurrentValue = false;

                        var newValue = new BooleanTypedValueDto();
                        newValue.BooleanValue = specificSettingDto.CurrentValue;
                        newValue.Type = TypedValueTypeEnum.BOOLEAN_TYPED_VALUE;
                        SetTypedSettingValue(settingDto, newValue);
                    }
                    else if (valueText.Equals(specificSettingDto.TrueText))
                    {
                        specificSettingDto.CurrentValue = true;

                        var newValue = new BooleanTypedValueDto();
                        newValue.BooleanValue = specificSettingDto.CurrentValue;
                        newValue.Type = TypedValueTypeEnum.BOOLEAN_TYPED_VALUE;
                        SetTypedSettingValue(settingDto, newValue);
                    }
                    else
                    {
                        specificSettingDto.CurrentValue = Boolean.Parse(valueText);

                        var newValue = new BooleanTypedValueDto();
                        newValue.BooleanValue = specificSettingDto.CurrentValue;
                        newValue.Type = TypedValueTypeEnum.BOOLEAN_TYPED_VALUE;
                        SetTypedSettingValue(settingDto, newValue);
                    }
                }
                else if (settingDto is IntSettingDto)
                {
                    var specificSettingDto = (IntSettingDto)settingDto;
                    specificSettingDto.CurrentValue = long.Parse(valueText);

                    if (specificSettingDto.IntType == IntTypeEnum.INT16
                        || specificSettingDto.IntType == IntTypeEnum.INT32 
                        || specificSettingDto.IntType == IntTypeEnum.INT64)
                    {
                        IntTypedValueDto newValue = new IntTypedValueDto();
                        newValue.IntValue = specificSettingDto.CurrentValue;
                        newValue.Type = TypedValueTypeEnum.INT_TYPED_VALUE;
                        SetTypedSettingValue(settingDto, newValue);
                    }
                    //else if ()
                    //{
                    //    Int64TypedValueDto newValue = new Int64TypedValueDto();
                    //    newValue.Int64Value = specificSettingDto.CurrentValue;
                    //    newValue.Type = TypedValueTypeEnum.INT_64T_YPED_VALUE;
                    //    SetTypedSettingValue(settingDto, newValue);
                    //}
                }
                else if (settingDto is FloatSettingDto)
                {
                    var specificSettingDto = (FloatSettingDto)settingDto;
                    specificSettingDto.CurrentValue = Double.Parse(valueText, NumberStyles.Float,
                        CultureInfo.CurrentCulture);

                    var newValue = new FloatTypedValueDto();
                    newValue.FloatValue = specificSettingDto.CurrentValue;
                    newValue.Type = TypedValueTypeEnum.FLOAT_TYPED_VALUE;
                    SetTypedSettingValue(settingDto, newValue);
                }
                else if (settingDto is StringSettingDto)
                {
                    var specificSettingDto = (StringSettingDto)settingDto;
                    specificSettingDto.CurrentValue = valueText;

                    var newValue = new StringTypedValueDto();
                    newValue.StringValue = specificSettingDto.CurrentValue;
                    newValue.Type = TypedValueTypeEnum.STRING_TYPED_VALUE;
                    SetTypedSettingValue(settingDto, newValue);
                }
            }
            catch (FormatException ex)
            {
                Logger.WarnFormat(string.Format("{0} '{1}'", ex.Message, valueText), "Client Wrapper");
            }
            catch (Exception ex)
            {
                Logger.WarnFormat(string.Format("Exception: '{0}'", ex.Message), "Client Wrapper");
            }
        }

        private void SetTypedSettingValue(SettingDto settingDto, TypedValueDto newValue)
        {
            var settingKey = new SettingKeyDto
            {
                SettingName = settingDto.Name,
                MachinePartFullPath = settingDto.MachinePart.FullPath
            };

            _connector.SetCurrentValueForSetting(settingKey, newValue, OnSetCurrentValueSetting);
            Logger.WarnFormat("Setting {0}/{1} set to {2} [{3}]",
                settingDto.MachinePart.FullPath, settingDto.Name, UtilityFunctions.Value(newValue), settingDto.Unit);
        }

        private void OnSetCurrentValueSetting()
        {
            RefreshMachinePartProperties();
        }

        #endregion

        #region Wrapper methods for labView <--> MCS connection

        public bool GetBoolObserverValue(string machinePart, string observer, out bool value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null)
            {
                // Check if we are caching the MachinePart
                if (!item.CacheData)
                {
                    item.CacheData = true;

                    // Refresh the tree, but don't wait for an answer from the server
                    RefreshMachinePartProperties(item);
                    value = false;
                    return false;
                }

                if (item.Observers != null)
                {
                    foreach (var observerDto in item.Observers)
                    {
                        if (observerDto.Name == observer)
                        {
                            if (observerDto is BooleanObserverDto)
                            {
                                var castObserver = (BooleanObserverDto)observerDto;
                                if (castObserver.CurrentValue.HasValue)
                                {
                                    value = castObserver.CurrentValue.Value;
                                    return true;
                                }
                                else
                                {
                                    value = false;
                                    return false;
                                }
                            }
                            else
                            {
                                value = false;
                                return false;
                            }
                        }
                    }
                }
            }

            value = false;
            return false;
        }

        public bool GetIntObserverValue(string machinePart, string observer, out long value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null)
            {
                // Check if we are caching the MachinePart
                if (!item.CacheData)
                {
                    item.CacheData = true;

                    // Refresh the tree, but don't wait for an answer from the server
                    RefreshMachinePartProperties(item);
                    value = 0;
                    return false;
                }

                if (item.Observers != null)
                {
                    foreach (var observerDto in item.Observers)
                    {
                        if (observerDto.Name == observer)
                        {
                            if (observerDto is IntObserverDto)
                            {
                                var castObserver = (IntObserverDto)observerDto;
                                if (castObserver.CurrentValue.HasValue)
                                {
                                    value = castObserver.CurrentValue.Value;
                                    return true;
                                }
                                else
                                {
                                    value = 0;
                                    return false;
                                }
                            }
                            else
                            {
                                value = 0;
                                return false;
                            }
                        }
                    }
                }
            }

            value = 0;
            return false;
        }

        public bool GetDoubleObserverValue(string machinePart, string observer, out double value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null)
            {
                // Check if we are caching the MachinePart
                if (!item.CacheData)
                {
                    item.CacheData = true;

                    // Refresh the tree, but don't wait for an answer from the server
                    RefreshMachinePartProperties(item);
                    value = 0;
                    return false;
                }

                if (item.Observers != null)
                {
                    foreach (var observerDto in item.Observers)
                    {
                        if (observerDto.Name == observer)
                        {
                            if (observerDto is FloatObserverDto)
                            {
                                var castObserver = (FloatObserverDto)observerDto;
                                if (castObserver.CurrentValue.HasValue)
                                {
                                    value = (double)castObserver.CurrentValue.Value;
                                    return true;
                                }
                                else
                                {
                                    value = 0;
                                    return false;
                                }
                            }
                            else
                            {
                                value = 0;
                                return false;
                            }
                        }
                    }
                }
            }

            value = 0;
            return false;
        }

        public bool GetBoolSettingValue(string machinePart, string setting, out bool value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null)
            {
                // Check if we are caching the MachinePart
                if (!item.CacheData)
                {
                    item.CacheData = true;

                    // Refresh the tree, but don't wait for an answer from the server
                    RefreshMachinePartProperties(item);
                    value = false;
                    return false;
                }

                if (item.Settings != null)
                {
                    foreach (var settingDto in item.Settings)
                    {
                        if (settingDto.Name == setting)
                        {
                            if (settingDto is BooleanSettingDto)
                            {
                                var castSetting = (BooleanSettingDto)settingDto;
                                if (castSetting.CurrentValue.HasValue)
                                {
                                    value = castSetting.CurrentValue.Value;
                                    return true;
                                }
                                else
                                {
                                    value = false;
                                    return false;
                                }
                            }
                            else
                            {
                                value = false;
                                return false;
                            }
                        }
                    }
                }
            }

            value = false;
            return false;
        }

        public bool GetLongSettingValue(string machinePart, string setting, out long value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null)
            {
                // Check if we are caching the MachinePart
                if (!item.CacheData)
                {
                    item.CacheData = true;

                    // Refresh the tree, but don't wait for an answer from the server
                    RefreshMachinePartProperties(item);
                    value = 0;
                    return false;
                }

                if (item.Settings != null)
                {
                    foreach (var settingDto in item.Settings)
                    {
                        if (settingDto.Name == setting)
                        {
                            if (settingDto is IntSettingDto)
                            {
                                var castSetting = (IntSettingDto)settingDto;
                                if (castSetting.CurrentValue.HasValue)
                                {
                                    value = castSetting.CurrentValue.Value;
                                    return true;
                                }
                                else
                                {
                                    value = 0;
                                    return false;
                                }
                            }
                            else
                            {
                                value = 0;
                                return false;
                            }
                        }
                    }
                }
            }

            value = 0;
            return false;
        }

        public bool GetDoubleSettingValue(string machinePart, string setting, out double value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null)
            {
                // Check if we are caching the MachinePart
                if (!item.CacheData)
                {
                    item.CacheData = true;

                    // Refresh the tree, but don't wait for an answer from the server
                    RefreshMachinePartProperties(item);
                    value = 0;
                    return false;
                }

                if (item.Settings != null)
                {
                    foreach (var settingDto in item.Settings)
                    {
                        if (settingDto.Name == setting)
                        {
                            if (settingDto is FloatSettingDto)
                            {
                                var castSetting = (FloatSettingDto)settingDto;
                                if (castSetting.CurrentValue.HasValue)
                                {
                                    value = (double)castSetting.CurrentValue.Value;
                                    return true;
                                }
                                else
                                {
                                    value = 0;
                                    return false;
                                }
                            }
                            else
                            {
                                value = 0;
                                return false;
                            }
                        }
                    }
                }
            }

            value = 0;
            return false;
        }

        public bool SetBoolSettingValue(string machinePart, string setting, bool value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item == null || item.Settings == null) return false;
            
            foreach (var settingDto in item.Settings)
            {
                if (settingDto.Name == setting)
                {
                    if (settingDto is BooleanSettingDto)
                    {
                        SetSettingValue(settingDto, value.ToString());
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool SetLongSettingValue(string machinePart, string setting, long value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item?.Settings == null) return false;
            
            foreach (var settingDto in item.Settings)
            {
                if (settingDto.Name != setting) continue;
                
                if (settingDto is IntSettingDto)
                {
                    SetSettingValue(settingDto, value.ToString());
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public bool SetDoubleSettingValue(string machinePart, string setting, double value)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item?.Settings == null) return false;

            foreach (var settingDto in item.Settings)
            {
                if (settingDto.Name != setting) continue;

                if (!(settingDto is FloatSettingDto)) return false;
                SetSettingValue(settingDto, value.ToString(CultureInfo.InvariantCulture));
                return true;
            }

            return false;
        }

        public void ExecuteCommand(string machinePart, string command, List<Argument> arguments)
        {
            MachinePartTreeItem item = FindItem(_machinePartTreeRootItem, machinePart);

            if (item != null && item.Commands != null && arguments != null && arguments.Count > 0)
            {
                foreach (var commandDto in item.Commands)
                {
                    if (commandDto.Name == command)
                    {
                        // Initialize the arguments
                        if (commandDto.Arguments != null)
                        {
                            foreach (var argumentDto in commandDto.Arguments)
                            {
                                foreach (var argument in arguments)
                                {
                                    if (argumentDto.Name == argument.name)
                                    {
                                        switch (argument.type)
                                        {
                                            case ArgumentType.Bool:
                                                SetArgumentValue(commandDto, argumentDto, argument.boolValue.ToString());
                                                break;
                                            case ArgumentType.Int:
                                                SetArgumentValue(commandDto, argumentDto, argument.longValue.ToString());
                                                break;
                                            case ArgumentType.Double:
                                                SetArgumentValue(commandDto, argumentDto, argument.doubleValue.ToString());
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        // Call the command
                        ExecuteCommand(commandDto);
                    }
                }
            }
        }

        public void GetMessages(out List<string> serverMessages, out List<string> controllerMessages)
        {
            Monitor.TryEnter(_getMessagesLock);
            try
            {
                serverMessages = _formattedServerMessages;
                controllerMessages = _formattedControllerMessages;
            }
            finally
            {
                Monitor.Exit(_getMessagesLock);
            }
        }

        #endregion

        /// <summary>
        /// Find a specific MachinePart in the tree
        /// </summary>
        /// <param name="item"></param>
        /// <param name="machinePart"></param>
        /// <returns></returns>
        private MachinePartTreeItem FindItem(MachinePartTreeItem item, string machinePart)
        {
            if (item == null)
            {
                return null;
            }

            if (UtilityFunctions.GetMachinePartPath(item.FullPath) == machinePart)
            {
                return item;
            }
            else
            {
                // Try the children
                foreach (var childItem in item.ChildItems)
                {
                    var child = FindItem(childItem, machinePart);
                    if (child != null)
                    {
                        return child;
                    }
                }

                return null;
            }
        }
    }
}
