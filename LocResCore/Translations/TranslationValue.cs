using System;

namespace LocResCore.Translations
{
    public class TranslationValue
    {
        public TranslationValue(string nameSpace, string key, uint hash, string originalValue, string value)
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
                throw new ArgumentException("Argument should not be null, empty or consisting of white-space characters!", nameof(nameSpace));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Argument should not be null, empty or consisting of white-space characters!", nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Argument should not be null or empty!", nameof(value));
            NameSpace = nameSpace;
            Key = key;
            Hash = hash;
            EscapeOriginalValue = originalValue;
            EscapeValue = value;
        }
        public TranslationValue(string nameSpace, string key, uint hash, string value)
            : this(nameSpace, key, hash, value, value)
        {
        }
        public string NameSpace { get; private set; }
        public string Key { get; private set; }
        public uint Hash { get; private set; }
        public string OriginalValue { get; private set; }
        public string Value { get; set; }
        public string EscapeOriginalValue
        {
            get
            {
                return OriginalValue.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
            }
            private set
            {
                OriginalValue = value.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
            }
        }
        public string EscapeValue
        {
            get
            {
                return Value.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
            }
            set
            {
                Value = value.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
            }
        }
        public bool IsEmpty { get { return string.IsNullOrEmpty(Value); } }
        public bool IsModified { get { return !string.Equals(OriginalValue, Value, StringComparison.InvariantCulture) && !string.IsNullOrEmpty(Value); } }
    }
}
