using LocResCore.Translations;
using LocResLib.Hashing;
using Serilog;
using System;

namespace LocResCore.Commands
{
    public class InsertCommand : BaseCommand, ICommand
    {
        public uint Hash { get; private set; }
        public string Value { get; private set; }
        public string EscapeValue
        {
            get
            {
                return Value.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
            }
            private set
            {
                Value = value.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
            }
        }
        public InsertCommand(string culture, string nameSpace, string key, uint hash, string value)
            : base(CommandType.Insert, culture, nameSpace, key)
        {
            if (string.Equals(nameSpace, "*", StringComparison.InvariantCulture))
                throw new ArgumentException("Argument should not be [*]!", nameof(nameSpace));
            if (string.Equals(key, "*", StringComparison.InvariantCulture))
                throw new ArgumentException("Argument should not be [*]!", nameof(key));
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Argument should not be null or empty!", nameof(value));
            Hash = hash;
            EscapeValue = value;
        }
        public InsertCommand(string culture, string nameSpace, string key, string original, string value)
            : this(culture, nameSpace, key, Crc.StrCrc32(original), value)
        {
        }
        public override void Apply(TranslationItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (string.Equals(item.Culture, Culture, StringComparison.InvariantCulture) || string.Equals(Culture, "*", StringComparison.InvariantCulture))
            {
                Log.Debug($"Insert Command:");
                Log.Debug($"  Culture   [{Culture}]");
                Log.Debug($"  Namespace [{NameSpace}]");
                Log.Debug($"  Key       [{Key}]");
                Log.Debug($"  Hash      [{Hash}]");
                Log.Debug($"  Value     [{EscapeValue}]");
                Log.Debug($"Applied on element:");
                Log.Debug($"  Culture   [{item.Culture}]");
                item.Add(new TranslationValue(NameSpace, Key, Hash, string.Empty, Value));
            }
        }
    }
}
