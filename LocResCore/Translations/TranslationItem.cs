using System;
using System.Collections.Generic;

namespace LocResCore.Translations
{
    public class TranslationItem : List<TranslationValue>
    {
        private readonly string _culture;
        public string Culture { get { return _culture; } }
        public TranslationItem(string culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
                throw new ArgumentException("Argument should not be null, empty or consisting of white-space characters!", nameof(culture));
            _culture = culture;
        }
    }
}
