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
using System.Data;
using log4net;
using MySql.Data.MySqlClient;

namespace MCDBackend.Database
{
    public class MySqlWrapper : IDataStorage
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MySqlWrapper));
        private MySqlConnection _connection;

        public MySqlWrapper()
        {
            Connect();
        }

        public void Connect()
        {
            _connection = new MySqlConnection(System.Configuration.ConfigurationManager.
                ConnectionStrings["DatabaseContext"].ConnectionString);
            try
            {
                log.Info("Opening database connection");
                _connection.Open();
            }
            catch (Exception ex)
            {
                _connection = null;
                HelperUtilities.LogException("Failed to connect to database.", log, ex);
            }
        }

        public void Disconnect()
        {
            if (_connection != null)
            {
                log.Info("Closing database connection");
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            log.Debug("Disposing MySqlWrapper");
            Disconnect();
        }

        public ValueTypeDao GetValueTypeByName(string name)
        {
            log.Info("Querying for ValueType with name: " + name);

            ValueTypeDao valueType = null;
            const string sql = "SELECT id, name FROM CordisVariableDb.ValueType WHERE name = @name";

            if (name == null || name.Length == 0)
            {
                log.Error("Name cannot be null or empty");
                return null;
            }

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                // Define the parameters and set their value.
                mySqlCommand.Parameters.AddWithValue("@name", name);

                try
                {
                    // Run the query by calling ExecuteReader().
                    using (MySqlDataReader dataReader = mySqlCommand.ExecuteReader())
                    {
                        // Create a data table to hold the retrieved data.
                        DataTable dataTable = new DataTable();
                        // Load the data from MySqlDataReader into the data table.
                        dataTable.Load(dataReader);
                        // Close the MySqlDataReader.
                        dataReader.Close();

                        // Display the data from the data table in the data grid view.
                        if (dataTable.Rows.Count == 1)
                        {
                            valueType = new ValueTypeDao();
                            valueType.id = dataTable.Rows[0].Field<int>("id");
                            valueType.name = dataTable.Rows[0].Field<string>("name");
                            log.Debug("Found ValueType record with id: " + valueType.id);
                        }
                        else
                        {
                            // Write value of first field as integer.
                            log.Warn("No ValueType record found.");
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error getting ValueType from database.", log, ex);
                    return null;
                }
            }
            return valueType;
        }

        public PartVariableDao GetPartVariableByNameAndPath(string name, string path)
        {
            log.Info("Querying for PartVariable: " + path + "/" + name);

            PartVariableDao partVariable = null;
            if (string.IsNullOrEmpty(name))
            {
                log.Error("NAme cannot be null or empty");
                return null;
            }
            if (string.IsNullOrEmpty(path))
            {
                log.Error("Path cannot be null or empty");
                return null;
            }

            const string sql = "SELECT * FROM PartVariable WHERE name = @name AND path = @path";

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                // Define the parameters and set their value.
                mySqlCommand.Parameters.AddWithValue("@name", name);
                mySqlCommand.Parameters.AddWithValue("@path", path);

                try
                {
                    // Run the query by calling ExecuteReader().
                    using (MySqlDataReader dataReader = mySqlCommand.ExecuteReader())
                    {
                        // Create a data table to hold the retrieved data.
                        DataTable dataTable = new DataTable();
                        // Load the data from MySqlDataReader into the data table.
                        dataTable.Load(dataReader);
                        // Close the MySqlDataReader.
                        dataReader.Close();

                        if (dataTable.Rows.Count == 1)
                        {
                            partVariable = new PartVariableDao();
                            partVariable.id = dataTable.Rows[0].Field<int>("id");
                            partVariable.name = dataTable.Rows[0].Field<string>("name");
                            partVariable.path = dataTable.Rows[0].Field<string>("path");
                            if (!dataTable.Rows[0].IsNull("machine_ip")) { partVariable.machine_ip = dataTable.Rows[0].Field<string>("machine_ip"); }
                            if (!dataTable.Rows[0].IsNull("machine_part")) { partVariable.machine_part = dataTable.Rows[0].Field<string>("machine_part"); }
                            if (!dataTable.Rows[0].IsNull("unit")) { partVariable.unit = dataTable.Rows[0].Field<string>("unit"); }
                            log.Debug("Found PartVariable record with id: " + partVariable.id);
                        }
                        else
                        {
                            log.Warn("No PartVariable record found");
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error getting PartVariable from database.", log, ex);
                    return null;
                }
            }
            return partVariable;
        }

        public PartValueDao GetLastPartValueByPartVariableId(int id)
        {
            log.Info("Querying for PartValue with id: " + id.ToString());

            PartValueDao partValue = null;
            if (id <= 0)
            {
                log.Error("Invalid PartVariable id: " + id);
                return null;
            }

            const string sql = "SELECT vtyp.id as vtyp_id, vtyp.name as vtyp_name, " +
                "pvar.id as pvar_id, pvar.machine_ip as pvar_machine_ip, pvar.machine_part as pvar_machine_part, pvar.name as pvar_name, pvar.path as pvar_path, pvar.unit as pvar_unit, " +
                "pval.idPartValue, pval.event_timestamp, pval.value_bool, pval.value_float, pval.value_enum, pval.value_int, pval.value_string, pval.value_word " +
                "FROM CordisVariableDb.PartValue pval " +
                "INNER JOIN CordisVariableDb.ValueType vtyp ON pval.part_type = vtyp.id " +
                "INNER JOIN CordisVariableDb.PartVariable pvar ON pval.part_variable = pvar.id " +
                "WHERE part_variable = @part_variable_id " +
                "ORDER BY event_timestamp DESC LIMIT 1;";

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                // Define the parameters and set their value.
                mySqlCommand.Parameters.AddWithValue("@part_variable_id", id);

                try
                {
                    // Run the query by calling ExecuteReader().
                    using (MySqlDataReader dataReader = mySqlCommand.ExecuteReader())
                    {
                        // Create a data table to hold the retrieved data.
                        DataTable dataTable = new DataTable();
                        // Load the data from MySqlDataReader into the data table.
                        dataTable.Load(dataReader);
                        // Close the MySqlDataReader.
                        dataReader.Close();

                        // Display the data from the data table in the data grid view.
                        if (dataTable.Rows.Count == 1)
                        {
                            ValueTypeDao vt = new ValueTypeDao();
                            vt.id = dataTable.Rows[0].Field<int>("vtyp_id");
                            vt.name = dataTable.Rows[0].Field<string>("vtyp_name");

                            PartVariableDao pv = new PartVariableDao();
                            pv.id = dataTable.Rows[0].Field<int>("pvar_id");
                            pv.machine_ip = dataTable.Rows[0].Field<string>("pvar_machine_ip");
                            pv.machine_part = dataTable.Rows[0].Field<string>("pvar_machine_part");
                            pv.name = dataTable.Rows[0].Field<string>("pvar_name");
                            pv.path = dataTable.Rows[0].Field<string>("pvar_path");
                            pv.unit = dataTable.Rows[0].Field<string>("pvar_unit");

                            partValue = new PartValueDao();
                            partValue.id = dataTable.Rows[0].Field<int>("idPartValue");
                            partValue.event_timestamp = dataTable.Rows[0].Field<DateTime>("event_timestamp");
                            partValue.part_variable = pv;
                            partValue.part_type = vt;

                            if (!dataTable.Rows[0].IsNull("value_bool")) { log.Info("Get value_bool"); partValue.value_bool = (bool)dataTable.Rows[0].Field<bool?>("value_bool"); }
                            if (!dataTable.Rows[0].IsNull("value_float")) { log.Info("Get value_float"); partValue.value_float = (double)dataTable.Rows[0].Field<double?>("value_float"); }
                            if (!dataTable.Rows[0].IsNull("value_enum")) { log.Info("Get value_enum"); partValue.value_enum = dataTable.Rows[0].Field<string>("value_enum"); }
                            if (!dataTable.Rows[0].IsNull("value_int")) { log.Info("Get value_int"); partValue.value_int = (int)dataTable.Rows[0].Field<Int32>("value_int"); }
                            if (!dataTable.Rows[0].IsNull("value_string")) { log.Info("Get value_string"); partValue.value_string = dataTable.Rows[0].Field<string>("value_string"); }
                            if (!dataTable.Rows[0].IsNull("value_word")) { log.Info("Get value_word"); partValue.value_word = (long)dataTable.Rows[0].Field<Int64>("value_word"); }

                            log.Debug("Found PartValue record with id: " + partValue.id + " ---" + partValue.ToString());
                        }
                        else
                        {
                            // Write value of first field as integer.
                            log.Warn("No PartValue record found.");
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error getting PartValue from database.", log, ex);
                    return null;
                }
            }
            return partValue;
        }

        public bool UpdatePartVariableById(int id, string name, string machineIp, string machinePart, string path, string unit)
        {
            log.Info("Updating PartVariable: " + path + "/" + name);

            if (string.IsNullOrEmpty(path))
            {
                log.Error("Path cannot be null or empty");
                return false;
            }

            string sql = "UPDATE PartVariable SET name = @name, machine_ip = @machine_ip, machine_part = @machine_part, path = @path, unit = @unit WHERE id=@id";

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                // Define the parameters and set their value.
                try
                {
                    mySqlCommand.Parameters.AddWithValue("@id", id);
                    mySqlCommand.Parameters.AddWithValue("@name", name);
                    mySqlCommand.Parameters.AddWithValue("@machine_ip", machineIp);
                    mySqlCommand.Parameters.AddWithValue("@machine_part", machinePart);
                    mySqlCommand.Parameters.AddWithValue("@path", path);
                    mySqlCommand.Parameters.AddWithValue("@unit", unit);

                    // Run the query by calling ExecuteNonQuery().
                    int recordsUpdated = mySqlCommand.ExecuteNonQuery();
                    log.Info(recordsUpdated + " records updated");
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error updating PartVariable in database.", log, ex);
                    return false;
                }
            }
            return true;
        }

        public int AddPartVariableByNameAndPath(string name, string machineIp, string machinePart, string path, string unit)
        {
            log.Info("Adding PartVariable: " + path + "/" + name);
            int partVariableId = -1;

            if (string.IsNullOrEmpty(path))
            {
                log.Error("Path cannot be null or empty");
                return -1;
            }

            string sql = "INSERT INTO PartVariable (name, path";
            if (machineIp != null)
            {
                sql += ", machine_ip";
            }
            if (machinePart != null)
            {
                sql += ", machine_part";
            }
            if (unit != null)
            {
                sql += ", unit";
            }
            sql += ") VALUES (@name, @path";
            if (machineIp != null)
            {
                sql += ", @machine_ip";
            }
            if (machinePart != null)
            {
                sql += ", @machine_part";
            }
            if (unit != null)
            {
                sql += ", @unit";
            }
            sql += ")";

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                // Define the parameters and set their value.
                try
                {
                    mySqlCommand.Parameters.AddWithValue("@name", name);
                    mySqlCommand.Parameters.AddWithValue("@path", path);
                    if (machineIp != null)
                    {
                        mySqlCommand.Parameters.AddWithValue("@machine_ip", machineIp);
                    }
                    if (machinePart != null)
                    {
                        mySqlCommand.Parameters.AddWithValue("@machine_part", machinePart);
                    }
                    if (unit != null)
                    {
                        mySqlCommand.Parameters.AddWithValue("@unit", unit);
                    }

                    // Run the query by calling ExecuteNonQuery().
                    int recordsUpdated = mySqlCommand.ExecuteNonQuery();
                    partVariableId = (int)mySqlCommand.LastInsertedId;
                    log.Info(recordsUpdated + " records updated");
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error adding PartVariable in database", log, ex);
                    partVariableId = -1;
                }
            }
            return partVariableId;
        }

        /*
        public PartValueDao AddPartValue(PartVariableDao partVariable, ValueTypeDao valueType, Object value)
        {
            PartValueDao partValue = null;

            if (partVariable == null || partVariable.id <= 0)
            {
                log.Error("PartVariable may not be null and PartVariable id must be greater then 0");
                return null;
            }
            if (valueType == null)
            {
                log.Error("ValueType may not be null");
                return null;
            }
            if (value == null)
            {
                log.Error("Value may not be null");
                return null;
            }

            string sql = "INSERT INTO PartValue (event_timestamp, part_variable, part_type, value_bool, value_float, value_enum, value_int, value_string, value_word) " +
                "VALUES (@event_timestamp, @part_variable_id, @value_type_id, @value_bool, @value_float, @value_enum, @value_int, @value_string, @value_word)";

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                try
                {
                    // Define the parameters and set their value.
                    mySqlCommand.Parameters.AddWithValue("@event_timestamp", DateTime.Now.ToUniversalTime());
                    mySqlCommand.Parameters.AddWithValue("@part_variable_id", partVariable.id);
                    mySqlCommand.Parameters.AddWithValue("@value_type_id", valueType.id);

                    bool result = Enum.TryParse<ObserverType>(valueType.name, out ObserverType observerType);
                    if (valueType.name == null || !result)
                    {
                        log.Error("Observer type " + valueType.name + " not recognized");
                        return null;
                    }

                    partValue = new PartValueDao();
                    partValue.part_type = valueType;
                    partValue.part_variable = partVariable;

                    if (observerType.Equals(ObserverType.BOOLEAN_OBSERVER))
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_bool", (bool)value);
                        partValue.value_bool = (bool)value;
                    }
                    else
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_bool", null);
                        partValue.value_bool = null;
                    }

                    if (observerType.Equals(ObserverType.FLOAT_OBSERVER))
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_float", (double)value);
                        partValue.value_float = (double)value;
                    }
                    else
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_float", null);
                        partValue.value_float = null;
                    }

                    if (observerType.Equals(ObserverType.ENUM_OBSERVER))
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_enum", (string)value);
                        partValue.value_enum = (string)value;
                    }
                    else
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_enum", null);
                        partValue.value_enum = null;
                    }

                    if (observerType.Equals(ObserverType.INT_OBSERVER))
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_int", Convert.ToInt32(value));
                        partValue.value_int = Convert.ToInt32(value);
                    }
                    else
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_int", null);
                        partValue.value_int = null;
                    }

                    if (observerType.Equals(ObserverType.STRING_OBSERVER))
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_string", (string)value);
                        partValue.value_string = (string)value;
                    }
                    else
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_string", null);
                        partValue.value_string = null;
                    }

                    if (observerType.Equals(ObserverType.WORD_OBSERVER))
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_word", Convert.ToInt64(value));
                        partValue.value_word = Convert.ToInt64(value);
                    }
                    else
                    {
                        mySqlCommand.Parameters.AddWithValue("@value_word", null);
                        partValue.value_word = null;
                    }

                    // Run the query by calling ExecuteNonQuery().
                    int recordsUpdated = mySqlCommand.ExecuteNonQuery();
                    log.Info(recordsUpdated + " records updated");
                    partValue.id = (int)mySqlCommand.LastInsertedId;
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error adding PartValue: " + value.ToString() + " of type "
                        + value.GetType().ToString() + " to the database.", log, ex);
                    return null;
                }
            }

            return partValue;
        }
        */

        public PartValueDao AddPartValue(PartVariableDao partVariable, ValueTypeDao valueType, PartValueDao value)
        {
            PartValueDao partValue = null;

            if (partVariable == null || partVariable.id <= 0)
            {
                log.Error("PartVariable may not be null and PartVariable id must be greater then 0");
                return null;
            }
            if (valueType == null)
            {
                log.Error("ValueType may not be null");
                return null;
            }
            if (value == null)
            {
                log.Error("Value may not be null");
                return null;
            }

            string sql = "INSERT INTO PartValue (event_timestamp, part_variable, part_type, value_bool, value_float, value_enum, value_int, value_string, value_word) " +
                "VALUES (@event_timestamp, @part_variable_id, @value_type_id, @value_bool, @value_float, @value_enum, @value_int, @value_string, @value_word)";

            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                try
                {
                    // Define the parameters and set their value.
                    mySqlCommand.Parameters.AddWithValue("@event_timestamp", DateTime.Now.ToUniversalTime());
                    mySqlCommand.Parameters.AddWithValue("@part_variable_id", partVariable.id);
                    mySqlCommand.Parameters.AddWithValue("@value_type_id", valueType.id);

                    bool result = Enum.TryParse<ObserverType>(valueType.name, out ObserverType observerType);
                    if (valueType.name == null || !result)
                    {
                        log.Error("Observer type " + valueType.name + " not recognized");
                        return null;
                    }

                    mySqlCommand.Parameters.AddWithValue("@value_bool", value.value_bool);
                    mySqlCommand.Parameters.AddWithValue("@value_float", value.value_float);
                    mySqlCommand.Parameters.AddWithValue("@value_enum", value.value_enum);
                    mySqlCommand.Parameters.AddWithValue("@value_int", value.value_int);
                    mySqlCommand.Parameters.AddWithValue("@value_string", value.value_string);
                    mySqlCommand.Parameters.AddWithValue("@value_word", value.value_word);

                    // Run the query by calling ExecuteNonQuery().
                    int recordsUpdated = mySqlCommand.ExecuteNonQuery();
                    log.Info(recordsUpdated + " records updated");

                    partValue = new PartValueDao();
                    partValue.id = (int)mySqlCommand.LastInsertedId;
                    partValue.part_type = valueType;
                    partValue.part_variable = partVariable;
                    partValue.value_bool = value.value_bool;
                    partValue.value_float = value.value_float;
                    partValue.value_enum = value.value_enum;
                    partValue.value_int = value.value_int;
                    partValue.value_string = value.value_string;
                    partValue.value_word = value.value_word;
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error adding PartValue: " + value.ToString() + " of type "
                        + value.GetType().ToString() + " to the database.", log, ex);
                    return null;
                }
            }
            return partValue;
        }

        public bool DeleteFromTableById(string tableName, string idColumnName, int id)
        {
            log.Info("Deleting " + tableName + " with id: " + id.ToString());

            if (string.IsNullOrEmpty(tableName))
            {
                log.Error("Table name may not be null or empty");
                return false;
            }
            if (string.IsNullOrEmpty(idColumnName))
            {
                log.Error("Id column name may not be null or empty");
                return false;
            }
            if (id <= 0)
            {
                log.Error("Invalid " + tableName + " id: " + id);
                return false;
            }

            string sql = "DELETE FROM " + tableName + " WHERE " + idColumnName + " = @id";
            // Create a SqlCommand object.
            using (MySqlCommand mySqlCommand = new MySqlCommand(sql, _connection))
            {
                // Define the parameters and set their value.
                try
                {
                    mySqlCommand.Parameters.AddWithValue("@id", id);

                    // Run the query by calling ExecuteNonQuery().
                    int recordsUpdated = mySqlCommand.ExecuteNonQuery();
                    log.Info(recordsUpdated + " records deleted");
                }
                catch (Exception ex)
                {
                    HelperUtilities.LogException("Error deleting " + tableName + " in database.", log, ex);
                    return false;
                }
            }
            return true;
        }
    }
}
