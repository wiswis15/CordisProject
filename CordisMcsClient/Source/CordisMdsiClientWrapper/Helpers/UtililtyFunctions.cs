using Cordis.Mdsi.Client.Dtos.MachinePart.TypedValue;
using Cordis.Mdsi.Client.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cordis.MdsiClientWrapper.Helpers
{
    public static class UtilityFunctions
    {
        /// <summary>
        /// Returns the controller part of a full machine part path.
        /// </summary>
        /// <param name="machinePartFullPath"></param>
        /// <returns></returns>
        public static string GetControllerName(string machinePartFullPath)
        {
            string result = "";

            if (!String.IsNullOrEmpty(machinePartFullPath))
            {
                // Check for multiple parts
                int pos = machinePartFullPath.IndexOf('/');
                if (pos > -1)
                {
                    // Check if first part contains a '|', i.e. is a controller
                    string firstPart = machinePartFullPath.Substring(0, pos);
                    pos = firstPart.IndexOf('|');
                    if (pos > -1)
                    {
                        result = firstPart;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the machine part path of a full machine part path. In other words, removes the controller part.
        /// </summary>
        /// <param name="machinePartFullPath"></param>
        /// <returns></returns>
        public static string GetMachinePartPath(string machinePartFullPath)
        {
            string result = "";

            if (!String.IsNullOrEmpty(machinePartFullPath))
            {
                result = machinePartFullPath;

                // Check for multiple parts
                int slashPos = machinePartFullPath.IndexOf('/');
                if (slashPos > -1)
                {
                    // Check if first part contains a '|', i.e. is a controller
                    string firstPart = machinePartFullPath.Substring(0, slashPos);
                    int pipePos = firstPart.IndexOf('|');
                    if (pipePos > -1)
                    {
                        result = machinePartFullPath.Substring(slashPos + 1);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the observer name in a full observer path (the part after the last '/')
        /// </summary>
        /// <param name="observerFullPath"></param>
        /// <returns></returns>
        public static string GetObserverName(string observerFullPath)
        {
            string result = "";

            if (!String.IsNullOrEmpty(observerFullPath))
            {
                result = observerFullPath;

                // Check for multiple parts
                int slashPos = observerFullPath.LastIndexOf('/');
                if (slashPos > -1)
                {
                    // Return the last part
                    result = observerFullPath.Substring(slashPos + 1);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the machine part path in a full observer path (the part before the last '/')
        /// </summary>
        /// <param name="observerFullPath"></param>
        /// <returns></returns>
        public static string GetObserverMachinePart(string observerFullPath)
        {
            string result = "";

            if (!String.IsNullOrEmpty(observerFullPath))
            {
                // Check for multiple parts
                int slashPos = observerFullPath.LastIndexOf('/');
                if (slashPos > -1)
                {
                    // Return the first part
                    result = observerFullPath.Substring(0, slashPos);
                }
            }

            return result;
        }

        public static string Value(TypedValueDto newValue)
        {
            string retVal = "";

            try
            {
                switch (newValue.Type)
                {
                    case TypedValueTypeEnum.BOOLEAN_TYPED_VALUE:
                        BooleanTypedValueDto b = newValue as BooleanTypedValueDto;
                        if (b != null) retVal = b.BooleanValue.ToString();
                        break;
                    case TypedValueTypeEnum.ENUM_TYPED_VALUE:
                        EnumTypedValueDto e = newValue as EnumTypedValueDto;
                        if (e != null) retVal = e.EnumValue.ToString();
                        break;
                    case TypedValueTypeEnum.INT_TYPED_VALUE:
                        IntTypedValueDto i = newValue as IntTypedValueDto;
                        if (i != null) retVal = i.IntValue.ToString();
                        break;
                    //case TypedValueTypeEnum.INT_64T_YPED_VALUE:
                    //    Int64TypedValueDto di = newValue as Int64TypedValueDto;
                    //    if (di != null) retVal = di.Int64Value.ToString();
                    //    break;
                    case TypedValueTypeEnum.FLOAT_TYPED_VALUE:
                        FloatTypedValueDto f = newValue as FloatTypedValueDto;
                        if (f != null) retVal = f.FloatValue.ToString();
                        break;
                    case TypedValueTypeEnum.STRING_TYPED_VALUE:
                        StringTypedValueDto s = newValue as StringTypedValueDto;
                        if (s != null) retVal = s.StringValue;
                        break;
                }
            }
            catch (Exception)
            {
            }

            return retVal;
        }
    }
}
