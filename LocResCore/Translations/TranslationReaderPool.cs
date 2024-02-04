using System.Collections.Generic;
using System.Linq;

namespace LocResCore.Translations
{
    public class TranslationReaderPool : ITranslationReaderPool
    {
        private readonly Dictionary<string, ITranslationReader> _readerDic = new Dictionary<string, ITranslationReader>();

        public void RegisterReader(ITranslationReader reader)
        {
            foreach (string extension in reader.Extensions)
            {
                _readerDic[extension] = reader;
            }
        }

        public IEnumerable<string> SupportedExtensions()
        {
            return _readerDic.Keys.ToList();
        }

        public bool TryGetReader(string extension, out ITranslationReader reader)
        {
            return _readerDic.TryGetValue(extension, out reader);
        }
    }
}
