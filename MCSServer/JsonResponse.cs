using System;

namespace MCDDatabase
{
    public enum ObserverType
    {
        BOOLEAN_OBSERVER,
        FLOAT_OBSERVER,
        ENUM_OBSERVER,
        INT_OBSERVER,
        STRING_OBSERVER,
        WORD_OBSERVER
    }

    public class JsonResponse
    {
        public string type { get; set; }
        public JsonValue[] value { get; set; }
    }

    public class JsonValue
    {
        public string name { get; set; }
        public string unit { get; set; }
        public string type { get; set; }
        public JsonMachinePart machinePart { get; set; }
        public Object currentValue { get; set; }
        public string trueText { get; set; }
        public string falseText { get; set; }
    }

    public class JsonMachinePart
    {
        public string fullPath { get; set; }
        public string name { get; set; }
    }
}
