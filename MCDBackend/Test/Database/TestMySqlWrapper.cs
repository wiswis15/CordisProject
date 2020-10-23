using System;
using MCDBackend.Database;
using NUnit.Framework;

namespace MCDBackend.Test.Database
{
    [TestFixture]
    public class TestMySqlWrapper
    {
        private IDataStorage _dataStorage = null;
        private int _partVariableId = -1;
        private readonly string _name = "NameTest";
        private string _pathName = "PathTest";

        [SetUp]
        protected void SetUp()
        {
            _dataStorage = new MySqlWrapper();

            // Create PartVariable
            string machineIp = "MachineIpTest";
            string machinePart = "MachinePartTest";
            _pathName = "PathTest/" + NUnit.Framework.TestContext.CurrentContext.Test.Name;
            string unit = null;
            // Insert PartVariable
            _partVariableId = _dataStorage.AddPartVariableByNameAndPath(_name, machineIp, machinePart, _pathName, unit);
            Assert.AreNotEqual(-1, _partVariableId);
        }

        [TearDown]
        protected void TearDown()
        {
            if (_dataStorage != null)
            {
                // Delete inserted PartVariable(s)
                PartValueDao partValue;
                bool result;
                do
                {
                    partValue = _dataStorage.GetLastPartValueByPartVariableId(_partVariableId);
                    if (partValue != null)
                    {
                        // Delete inserted PartValue
                        result = _dataStorage.DeleteFromTableById("PartValue", "idPartValue", partValue.id);
                        Assert.AreEqual(true, result);
                    }
                } while (partValue != null);
                // Delete inserted PartVariable
                result = _dataStorage.DeleteFromTableById("PartVariable", "id", _partVariableId);
                Assert.AreEqual(true, result);

                _dataStorage.Dispose();
                _dataStorage = null;
            }
        }

        [Test]
        public void TestGetValueTypeByName()
        {
            foreach (ObserverType observerType in (ObserverType[])Enum.GetValues(typeof(ObserverType)))
            {
                string type = observerType.ToString();
                ValueTypeDao valueType = _dataStorage.GetValueTypeByName(type);
                Assert.IsNotNull(valueType);
                Assert.AreEqual(type, valueType.name);
            }
        }

        [Test]
        public void TestGetNonExistingValueTypeByName()
        {
            string type = "DoesNotExist";
            ValueTypeDao valueType = _dataStorage.GetValueTypeByName(type);
            Assert.IsNull(valueType);
        }

        [Test]
        public void TestGetPartVariableByName()
        {
            // Get PartVariable
            PartVariableDao partVariable = _dataStorage.GetPartVariableByNameAndPath(_name, _pathName);
            Assert.IsNotNull(partVariable);
            Assert.AreEqual("NameTest", partVariable.name);
            Assert.AreEqual("MachineIpTest", partVariable.machine_ip);
            Assert.AreEqual("MachinePartTest", partVariable.machine_part);
            Assert.AreEqual(_pathName, partVariable.path);
            Assert.IsNull(partVariable.unit);
        }

        [Test]
        public void TestGetNonExistingPartVariableByName()
        {
            string path = "DoesNotExist";
            PartVariableDao partVariable = _dataStorage.GetPartVariableByNameAndPath(_name, path);
            Assert.IsNull(partVariable);
        }

        [Test]
        public void TestGetPartValueByPartVariableId()
        {
            // Get PartVariable
            PartVariableDao partVariable = _dataStorage.GetPartVariableByNameAndPath(_name, _pathName);
            Assert.IsNotNull(partVariable);
            // Get ValueType
            ValueTypeDao valueType = _dataStorage.GetValueTypeByName(ObserverType.ENUM_OBSERVER.ToString());
            Assert.IsNotNull(valueType);
            // Add PartValue
            PartValueDao value = new PartValueDao();
            value.value_enum = "TestValue";
            PartValueDao resultValue = _dataStorage.AddPartValue(partVariable, valueType, value);
            Assert.IsNotNull(resultValue);
            // Get PartVariable
            PartValueDao partValue = _dataStorage.GetLastPartValueByPartVariableId(_partVariableId);
            Assert.IsNotNull(partValue);
            Assert.AreEqual(ObserverType.ENUM_OBSERVER.ToString(), partValue.part_type.name);
            Assert.AreEqual(_partVariableId, partValue.part_variable.id);
            Assert.IsNull(partValue.value_bool);
            Assert.IsNull(partValue.value_float);
            Assert.AreEqual(value, partValue.value_enum);
            Assert.IsNull(partValue.value_int);
            Assert.IsNull(partValue.value_string);
            Assert.IsNull(partValue.value_word);
            // Delete inserted PartValue
            bool result = _dataStorage.DeleteFromTableById("PartValue", "idPartValue", resultValue.id);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestNonExistingGetPartValueByPartVariableId()
        {
            int partVariableId = -1;
            PartValueDao partValue = _dataStorage.GetLastPartValueByPartVariableId(partVariableId);
            Assert.IsNull(partValue);
        }

        [Test]
        public void TestAddAndGetPartVariableByPath()
        {
            string name = "InsertTest";
            string machineIp = "MachineIInsertTest";
            string machinePart = "MachinePartInsertTest";
            string path = "PathInsertTest";
            string unit = "UnitInsertTest";
            // Insert value
            int id = _dataStorage.AddPartVariableByNameAndPath(name, machineIp, machinePart, path, unit);
            Assert.AreNotEqual(-1, id);
            // Check inserted value
            PartVariableDao partVariable = _dataStorage.GetPartVariableByNameAndPath(name, path);
            Assert.AreEqual(id, partVariable.id);
            Assert.AreEqual(name, partVariable.name);
            Assert.AreEqual(machineIp, partVariable.machine_ip);
            Assert.AreEqual(machinePart, partVariable.machine_part);
            Assert.AreEqual(path, partVariable.path);
            Assert.AreEqual(unit, partVariable.unit);
            // Delete inserted PartVariable
            bool result = _dataStorage.DeleteFromTableById("PartVariable", "id", id);
            Assert.AreEqual(true, result);
            // Check inserted PartVariable is deleted
            partVariable = _dataStorage.GetPartVariableByNameAndPath(name, path);
            Assert.IsNull(partVariable);
        }

        [Test]
        public void TestUpdatePartVariableById()
        {
            string name = "NameUpdatedTest";
            string machineIp = null;
            string machinePart = null;
            string path = "PathUpdatedTest";
            string unit = "UnitUpdatedTest";
            // Update value
            bool result = _dataStorage.UpdatePartVariableById(_partVariableId, name, machineIp, machinePart, path, unit);
            Assert.AreEqual(true, result);
            // Check updated value
            PartVariableDao partVariable = _dataStorage.GetPartVariableByNameAndPath(name, path);
            Assert.AreEqual(_partVariableId, partVariable.id);
            Assert.AreEqual(name, partVariable.name);
            Assert.IsNull(partVariable.machine_ip);
            Assert.IsNull(partVariable.machine_part);
            Assert.AreEqual(path, partVariable.path);
            Assert.AreEqual(unit, partVariable.unit);
        }

        [Test]
        public void TestAddAndGetValue()
        {
            // Get PartVariable
            PartVariableDao partVariable = _dataStorage.GetPartVariableByNameAndPath(_name, _pathName);
            Assert.IsNotNull(partVariable);
            // Get ValueType
            ValueTypeDao valueType = _dataStorage.GetValueTypeByName(ObserverType.ENUM_OBSERVER.ToString());
            Assert.IsNotNull(valueType);
            // Add PartValue
            PartValueDao value = new PartValueDao();
            value.value_enum = "TestValue";
            PartValueDao resultValue = _dataStorage.AddPartValue(partVariable, valueType, value);
            Assert.IsNotNull(resultValue);
            Assert.IsNull(resultValue.value_bool);
            Assert.IsNull(resultValue.value_float);
            Assert.AreEqual(value, resultValue.value_enum);
            Assert.IsNull(resultValue.value_int);
            Assert.IsNull(resultValue.value_string);
            Assert.IsNull(resultValue.value_word);
            // Delete inserted PartValue
            bool result = _dataStorage.DeleteFromTableById("PartValue", "idPartValue", resultValue.id);
            Assert.AreEqual(true, result);
        }
    }
}
