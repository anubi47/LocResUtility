using System.Collections.Generic;
using System.Linq;

namespace LocResCore.Translations
{
    public class TranslationWriterPool : ITranslationWriterPool
    {
        private readonly Dictionary<string, ITranslationWriter> _writerDic = new Dictionary<string, ITranslationWriter>();

        public void RegisterWriter(ITranslationWriter writer)
        {
            foreach (string extension in writer.Extensions)
            {
                _writerDic[extension] = writer;
            }
        }

        public IEnumerable<string> SupportedExtensions()
        {
            return _writerDic.Keys.ToList();
        }

        public bool TryGetWriter(string extension, out ITranslationWriter writer)
        {
            return _writerDic.TryGetValue(extension, out writer);
        }
    }
}
