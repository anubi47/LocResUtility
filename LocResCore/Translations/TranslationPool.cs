using System;
using System.Collections;
using System.Collections.Generic;

namespace LocResCore.Translations
{
    public class TranslationPool : IEnumerable<TranslationItem>
    {
        private readonly Dictionary<string, TranslationItem> _itemDic;
        private string _defaultCulture = string.Empty;
        public string DefaultCulture { get { return _defaultCulture; } }
        public TranslationPool()
        {
            _itemDic = new Dictionary<string, TranslationItem>();
        }
        public void Add(TranslationItem item, bool isDefault = false)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));
            _itemDic.Add(item.Culture, item);
            if (isDefault)
                _defaultCulture = item.Culture;
        }
        public void Clear()
        {
            _itemDic.Clear();
            _defaultCulture = string.Empty;
        }
        public bool TryGetItem(string culture, out TranslationItem item)
        {
            return _itemDic.TryGetValue(culture, out item);
        }
        public IEnumerator<TranslationItem> GetEnumerator()
        {
            return _itemDic.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
