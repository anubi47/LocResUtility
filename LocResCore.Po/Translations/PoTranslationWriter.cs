using Karambolo.PO;
using LocResCore.Translations;
using System;
using System.IO;

namespace LocResCore.Po.Translations
{
    public class PoTranslationWriter : ITranslationWriter
    {
        public string[] Extensions { get => new string[] { ".pot", ".po" }; }

        public void Write(string filePath, TranslationPool pool)
        {
            throw new NotImplementedException("Multiple language not supported!");
        }

        public void Write(string filePath, TranslationItem item)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            using (var file = File.Create(filePath))
            using (var writer = new StreamWriter(file))
            {
                var poCatalog = new POCatalog();
                poCatalog.Encoding = "UTF-8";
                //poCatalog.Language = data.Culture;

                foreach (TranslationValue value in item)
                {
                    string nameSpace = value.NameSpace;
                    string key = value.Key;
                    string context = $"{nameSpace}/{key}";
                    string source = value.OriginalValue;

                    var poKey = new POKey(source, contextId: context);
                    var poEntry = new POSingularEntry(poKey);
                    string extension = Path.GetExtension(filePath);
                    if (string.Equals(extension, ".po"))
                    {
                        poEntry.Translation = value.Value;
                    }
                    poCatalog.Add(poEntry);
                }

                var poGenerator = new POGenerator();
                poGenerator.Generate(writer, poCatalog);
            }
        }
    }
}
