using LocResCore.Translations;
using Serilog;
using System;

namespace LocResCore.Commands
{
    public class UpdateCommand : BaseCommand, ICommand
    {
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
        public UpdateCommand(string culture, string nameSpace, string key, string value)
            : base(CommandType.Update, culture, nameSpace, key)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("Argument should not be null or empty!", nameof(value));
            EscapeValue = value;
        }
        public override void Apply(TranslationItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (string.Equals(item.Culture, Culture, StringComparison.InvariantCulture) || string.Equals(Culture, "*", StringComparison.InvariantCulture))
            {
                foreach (TranslationValue value in item)
                {
                    if (string.Equals(value.NameSpace, NameSpace, StringComparison.InvariantCulture) || string.Equals(NameSpace, "*", StringComparison.InvariantCulture))
                    {
                        if (string.Equals(value.Key, Key, StringComparison.InvariantCulture) || string.Equals(Key, "*", StringComparison.InvariantCulture))
                        {
                            Log.Debug($"Update Command:");
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
                            value.EscapeValue = Value;
                        }
                    }
                }
            }
        }
    }
}
