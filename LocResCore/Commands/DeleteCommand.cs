using LocResCore.Translations;
using Serilog;
using System;

namespace LocResCore.Commands
{
    public class DeleteCommand : BaseCommand, ICommand
    {
        public DeleteCommand(string culture, string nameSpace, string key)
            : base(CommandType.Delete, culture, nameSpace, key)
        {
        }
        public DeleteCommand(string culture, string nameSpace)
            : this(culture, nameSpace, "*")
        {
        }
        public override void Apply(TranslationItem item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.Culture))
                throw new ArgumentException(nameof(item.Culture));
            if (string.Equals(item.Culture, Culture, StringComparison.InvariantCulture) || string.Equals(Culture, "*", StringComparison.InvariantCulture))
            {
                for (int i = item.Count - 1; i >= 0; i--)
                {
                    TranslationValue value = item[i];
                    if (string.Equals(value.NameSpace, NameSpace, StringComparison.InvariantCulture) || string.Equals(NameSpace, "*", StringComparison.InvariantCulture))
                    {
                        if (string.Equals(value.Key, Key, StringComparison.InvariantCulture) || string.Equals(Key, "*", StringComparison.InvariantCulture))
                        {
                            Log.Debug($"Delete Command:");
                            Log.Debug($"  Culture = [{Culture}]");
                            Log.Debug($"  Namespace = [{NameSpace}]");
                            Log.Debug($"  Key = [{Key}]");
                            Log.Debug($"Applied on element:");
                            Log.Debug($"  Culture = [{item.Culture}]");
                            Log.Debug($"  Namespace = [{value.NameSpace}]");
                            Log.Debug($"  Key = [{value.Key}]");
                            Log.Debug($"  Hash = [{value.Hash}]");
                            Log.Debug($"  Value = [{value.EscapeValue}]");
                            item.RemoveAt(i);
                        }
                    }
                }
            }
        }
    }
}
