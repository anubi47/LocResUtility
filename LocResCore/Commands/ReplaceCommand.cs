using LocResCore.Translations;
using Serilog;
using System;

namespace LocResCore.Commands
{
    public class ReplaceCommand : BaseCommand, ICommand
    {
        public string OldValue { get; private set; }
        public string EscapeOldValue
        {
            get
            {
                return OldValue.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
            }
            private set
            {
                OldValue = value.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
            }
        }
        public string NewValue { get; private set; }
        public string EscapeNewValue
        {
            get
            {
                return NewValue.Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n");
            }
            private set
            {
                NewValue = value.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
            }
        }
        public ReplaceCommand(string culture, string nameSpace, string key, string oldValue, string newValue)
            : base(CommandType.Replace, culture, nameSpace, key)
        {
            if (string.IsNullOrEmpty(oldValue))
                throw new ArgumentException("Argument should not be null or empty!", nameof(oldValue));
            if (newValue is null)
                throw new ArgumentNullException(nameof(newValue));
            EscapeOldValue = oldValue;
            EscapeNewValue = newValue;
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
                            if (value.Value.Contains(OldValue))
                            {
                                Log.Debug($"Replace Command:");
                                Log.Debug($"  Culture   [{Culture}]");
                                Log.Debug($"  Namespace [{NameSpace}]");
                                Log.Debug($"  Key       [{Key}]");
                                Log.Debug($"  OldValue  [{EscapeOldValue}]");
                                Log.Debug($"  NewValue  [{EscapeNewValue}]");
                                Log.Debug($"Applied on element:");
                                Log.Debug($"  Culture   [{item.Culture}]");
                                Log.Debug($"  Namespace [{value.NameSpace}]");
                                Log.Debug($"  Key       [{value.Key}]");
                                Log.Debug($"  Hash      [{value.Hash}]");
                                Log.Debug($"  OldValue  [{value.EscapeValue}]");
                                value.Value = value.Value.Replace(OldValue, NewValue);
                                Log.Debug($"  NewValue  [{value.EscapeValue}]");
                            }
                        }
                    }
                }
            }
        }
    }
}
