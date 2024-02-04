using LocResCore.Translations;
using System;

namespace LocResCore.Commands
{
    public abstract class BaseCommand : ICommand
    {
        private readonly CommandType _type;
        private readonly string _culture;
        private readonly string _namespace;
        private readonly string _key;
        public CommandType Type { get { return _type; } }
        public string Culture { get { return _culture; } }
        public string NameSpace { get { return _namespace; } }
        public string Key { get { return _key; } }
        public BaseCommand(CommandType type, string culture, string nameSpace, string key)
        {
            if (string.IsNullOrWhiteSpace(culture))
                throw new ArgumentException("Argument should not be null, empty or consisting of white-space characters!", nameof(culture));
            if (string.IsNullOrWhiteSpace(nameSpace))
                throw new ArgumentException("Argument should not be null, empty or consisting of white-space characters!", nameof(nameSpace));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Argument should not be null, empty or consisting of white-space characters!", nameof(key));
            _type = type;
            _culture = culture;
            _namespace = nameSpace;
            _key = key;
        }
        public abstract void Apply(TranslationItem item);
        public virtual void Apply(TranslationPool pool)
        {
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));
            foreach (TranslationItem item in pool)
            {
                Apply(item);
            }
        }
    }
}
