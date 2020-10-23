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
using System.Timers;
using log4net;
using MCDBackend.Database;
using MCDBackend.XmlConfig;
using MCDBackend.Notificaitons;
using Cordis.Mdsi.Client.Dtos.MachinePart.Observer;
using FestaJsonConnectorSupport;
using Cordis.MdsiClientWrapper;
using System.IO;

namespace MCDBackend
{
    public class VariableObject : IDisposable
    {
        // Constants
        private static readonly ILog log = LogManager.GetLogger(typeof(VariableObject));

        // Member variables
        private readonly System.Timers.Timer _timer;
        private readonly Variable _variable;

        private readonly INotificationServer _notificationServer;
        private readonly MdsiClientWrapper _wrapper;
        private PartVariableDao _mcsPartVariable = null;
        private PartValueDao _mcsPartValue = null;

        private IDataStorage _databaseWrapper = null;
        private ValueTypeDao _valueType;
        private PartVariableDao _partVariable;
        private PartValueDao _lastValue;

        // Constructor
        public VariableObject(Server server, Variable variable, MdsiClientWrapper wrapper)
        {
            _variable = variable;
            _wrapper = wrapper;

            _notificationServer = new UdpNotificationServer();
            _databaseWrapper = new MySqlWrapper();
            _valueType = null;
            _partVariable = null;
            _lastValue = null;

            _timer = new System.Timers.Timer
            {
                Interval = _variable.interval,
                AutoReset = true,
            };
            _timer.Elapsed += OnTimerEvent;
        }

        // Methods
        public void StartTimer()
        {
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public bool Initialize()
        {
            RequestVariableFromMachinePartConnector();
            if (_mcsPartVariable == null || _mcsPartValue == null)
            {
                log.Fatal("Unable to get partvariable and/or partvalue values from MCS.");
                return false;
            }

            if (_mcsPartValue.part_type == null || string.IsNullOrEmpty(_mcsPartValue.part_type.name))
            {
                log.Fatal("Unable to get ValueType from MCS.");
                return false;
            }

            _valueType = _databaseWrapper.GetValueTypeByName(_mcsPartValue.part_type.name);
            if (_valueType == null)
            {
                log.Fatal("Unable to get ValueType from database.");
                return false;
            }

            _partVariable = _databaseWrapper.GetPartVariableByNameAndPath(_mcsPartVariable.name, _mcsPartVariable.path);
            if (_partVariable == null)
            {
                _partVariable = _mcsPartVariable;
                _partVariable.id = _databaseWrapper.AddPartVariableByNameAndPath(_mcsPartVariable.name,
                    _mcsPartVariable.machine_ip, _mcsPartVariable.machine_part, _mcsPartVariable.path,
                    _mcsPartVariable.unit);
            }
            else if ((_partVariable.machine_ip != _mcsPartVariable.machine_ip)
                || (_partVariable.machine_part != _mcsPartVariable.machine_part)
                || (_partVariable.unit != _mcsPartVariable.unit))
            {
                _mcsPartVariable.id = _partVariable.id;
                _partVariable = _mcsPartVariable;
                _databaseWrapper.UpdatePartVariableById(_partVariable.id, _mcsPartVariable.name,
                    _mcsPartVariable.machine_ip, _mcsPartVariable.machine_part, _mcsPartVariable.path,
                    _mcsPartVariable.unit);
            }
            if (_partVariable == null)
            {
                log.Fatal("Unable to add or update PartVariable in database.");
                return false;
            }

            _lastValue = _databaseWrapper.GetLastPartValueByPartVariableId(_partVariable.id);
            if (_lastValue == null)
            {
                log.Warn("No variable available for type: " + _valueType.name);
            }
            // TODO: Add initial value to database?
            return true;
        }

        private void OnTimerEvent(Object sender, ElapsedEventArgs e)
        {
            log.Info("------------------- Timer elapsed --------------------");
            System.Threading.Thread thread = System.Threading.Thread.CurrentThread;
            System.Diagnostics.StackFrame frame = new System.Diagnostics.StackTrace().GetFrame(0);
            log.Debug(frame.GetMethod() + " runs in thread: " + thread.Name + " - " + thread.ManagedThreadId);

            RequestVariableFromMachinePartConnector();
            if (_mcsPartVariable == null || _mcsPartValue == null)
            {
                log.Fatal("Unable to get partvariable and/or partvalue values from MCS.");
                return;
            }

            // Display result
            log.Debug(" * PartValue from MCS: '" + _mcsPartValue.ToString() + "'");

            if (_lastValue == null)
            {
                _lastValue = _databaseWrapper.AddPartValue(_partVariable, _valueType, _mcsPartValue);
            }
            else
            {
                // Compare responsevalue to lastvalue, if value differs, add value
                if (!_lastValue.part_type.id.Equals(_valueType.id))
                {
                    log.Fatal("Value type name " + _lastValue.part_type.name + " does not match with last value for this variable " + _valueType.name);
                    return;
                }
                bool parseResult = Enum.TryParse<ObserverType>(_valueType.name, out ObserverType observerType);
                if (_valueType.name == null || !parseResult)
                {
                    log.Error("Observer type " + _valueType.name + " not recognized");
                    return;
                }

                try
                {
                    switch (observerType)
                    {
                        case ObserverType.BOOLEAN_OBSERVER:
                            if (_lastValue.value_bool.Equals(_mcsPartValue.value_bool))
                            {
                                log.Warn("Bool value hasn't changed, no need to update value");
                                return;
                            }
                            break;
                        case ObserverType.FLOAT_OBSERVER:
                            if (_lastValue.value_float.Equals(_mcsPartValue.value_float))
                            {
                                log.Warn("Float value hasn't changed, no need to update value");
                                return;
                            }
                            break;
                        case ObserverType.ENUM_OBSERVER:
                            if (_lastValue.value_enum.Equals(_mcsPartValue.value_enum))
                            {
                                log.Warn("Enum value hasn't changed, no need to update value");
                                return;
                            }
                            break;
                        case ObserverType.INT_OBSERVER:
                            if (_lastValue.value_int.Equals(_mcsPartValue.value_int))
                            {
                                log.Warn("Int value hasn't changed, no need to update value");
                                return;
                            }
                            break;
                        case ObserverType.STRING_OBSERVER:
                            if (_lastValue.value_string.Equals(_mcsPartValue.value_string))
                            {
                                log.Warn("String value hasn't changed, no need to update value");
                                return;
                            }
                            break;
                        case ObserverType.WORD_OBSERVER:
                            if (_lastValue.value_word.Equals(_mcsPartValue.value_word))
                            {
                                log.Warn("Word value hasn't changed, no need to update value");
                                return;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error comparing " + observerType.ToString() + " value " + _mcsPartValue.ToString()
                        + " with lastValue" + _lastValue.ToString() + ", continue with inserting value in database.", log, ex);
                    return;
                }

                _lastValue = _databaseWrapper.AddPartValue(_partVariable, _valueType, _mcsPartValue);
                _notificationServer.SendNotification(_partVariable.path);
            }
        }

        private void RequestVariableFromMachinePartConnector()
        {
            log.Debug("Requesting variable from MCS server using MachinePartConnector.");
            string machinePartTree = _variable.controllername + Path.DirectorySeparatorChar + _variable.machinename
                + Path.DirectorySeparatorChar + _variable.machinepart;
            _mcsPartVariable = null;
            _mcsPartValue = null;

            ObserverValueCriteria criteria = new ObserverValueCriteria
            {
                MachinePartRegEx = machinePartTree,
                ObserverNameRegEx = _variable.observername,
                TreeSearch = null
            };
            _wrapper.MachinePartConnector.GetObserverValues(criteria, OnGetObserverValues);
        }

        private void OnGetObserverValues(IObservableList<ObserverDto> obj)
        {
            foreach (ObserverDto observerDto in obj)
            {
                ValueTypeDao valueType = new ValueTypeDao();
                valueType.name = observerDto.Type.ToString();

                _mcsPartVariable = new PartVariableDao();
                _mcsPartVariable.name = observerDto.Name;
                _mcsPartVariable.path = observerDto.MachinePart.FullPath;
                _mcsPartVariable.unit = observerDto.Unit;

                _mcsPartValue = new PartValueDao();
                _mcsPartValue.event_timestamp = DateTime.Now;
                _mcsPartValue.part_type = valueType;
                switch (observerDto)
                {
                    case BooleanObserverDto boolDto:
                        _mcsPartValue.value_bool = boolDto.CurrentValue;
                        break;
                    case FloatObserverDto floatDto:
                        _mcsPartValue.value_float = floatDto.CurrentValue;
                        break;
                    case EnumObserverDto enumDto:
                        _mcsPartValue.value_enum = enumDto.CurrentValue;
                        break;
                    case IntObserverDto intDto:
                        _mcsPartValue.value_int = (int)intDto.CurrentValue;
                        break;
                    case StringObserverDto stringDto:
                        _mcsPartValue.value_string = stringDto.CurrentValue;
                        break;
                    case WordObserverDto wordDto:
                        _mcsPartValue.value_word = wordDto.CurrentValue;
                        break;
                }
            }
        }

        public bool IsVariable(string path)
        {
            if (_partVariable != null && !string.IsNullOrEmpty(_partVariable.path))
            {
                return path.Equals(_partVariable.path);
            }
            return false;
        }

        public void AddClient(string address, int port)
        {
            ClientObject c = new ClientObject(address, port);
            _notificationServer.AddClient(c);
        }

        public void Dispose()
        {
            if (_timer.Enabled)
            {
                StopTimer();
            }
            if (_databaseWrapper != null)
            {
                _databaseWrapper.Dispose();
                _databaseWrapper = null;
            }
        }
    }
}
