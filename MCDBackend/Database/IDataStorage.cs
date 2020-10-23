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

namespace MCDBackend.Database
{
    public interface IDataStorage
    {
        void Connect();

        void Disconnect();

        void Dispose();

        ValueTypeDao GetValueTypeByName(string name);

        PartVariableDao GetPartVariableByNameAndPath(string name, string path);

        PartValueDao GetLastPartValueByPartVariableId(int id);

        bool UpdatePartVariableById(int id, string name, string machineIp, string machinePart, string path, string unit);

        int AddPartVariableByNameAndPath(string name, string machineIp, string machinePart, string path, string unit);

        //PartValueDao AddPartValue(PartVariableDao partVariable, ValueTypeDao valueType, Object value);
        PartValueDao AddPartValue(PartVariableDao partVariable, ValueTypeDao valueType, PartValueDao value);

        bool DeleteFromTableById(string tableName, string idColumnName, int id);
    }
}
