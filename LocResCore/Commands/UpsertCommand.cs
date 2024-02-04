using LocResCore.Translations;
using LocResLib.Hashing;
using Serilog;
using System;

namespace LocResCore.Commands
{
    public class UpsertCommand : BaseCommand, ICommand
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
        public UpsertCommand(string culture, string nameSpace, string key, uint hash, string value)
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
        public UpsertCommand(string culture, string nameSpace, string key, string original, string value)
            : this(culture, nameSpace, key, Crc.StrCrc32(original), value)
        {
        }
        public override void Apply(TranslationItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (string.Equals(item.Culture, Culture, StringComparison.InvariantCulture) || string.Equals(Culture, "*", StringComparison.InvariantCulture))
            {
                bool isUpdate = false;
                foreach (TranslationValue value in item)
                {
                    if (string.Equals(value.NameSpace, NameSpace, StringComparison.InvariantCulture))
                    {
                        if (string.Equals(value.Key, Key, StringComparison.InvariantCulture))
                        {
                            Log.Debug($"Upsert Command [UPDATE]:");
                            Log.Debug($"  Culture   [{Culture}]");
                            Log.Debug($"  Namespace [{NameSpace}]");
                            Log.Debug($"  Key       [{Key}]");
                            Log.Debug($"  Value     [{EscapeValue}]");
                            Log.Debug($"Applied on element:");
                            Log.Debug($"  Culture   [{item.Culture}]");
                            Log.Debug($"  Namespace [{value.NameSpace}]");
                            Log.Debug($"  Key       [{value.Key}]");
                            Log.Debug($"  Hash      [{value.Hash}]");
                            Log.Debug($"  Value     [{value.EscapeValue}]");
                            isUpdate = true;
                            value.EscapeValue = Value;
                        }
                    }
                }
                if (!isUpdate)
                {
                    Log.Debug($"Upsert Command [INSERT]:");
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
}
