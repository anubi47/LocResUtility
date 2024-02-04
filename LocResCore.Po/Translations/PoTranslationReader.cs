using Karambolo.PO;
using LocResCore.Translations;
using Serilog;
using System;
using System.IO;

namespace LocResCore.Po.Translations
{
    public class PoTranslationReader : ITranslationReader
    {
        public string[] Extensions { get => new string[] { ".pot", ".po" }; }

        public void Read(out TranslationPool pool, string filePath)
        {
            throw new NotImplementedException("Multiple language not supported!");
        }

        public void Read(out TranslationItem item, string filePath, string culture)
        {
            item = new TranslationItem(culture);
            using (var file = File.OpenRead(filePath))
            using (var reader = new StreamReader(file))
            {
                var poParser = new POParser();
                POParseResult poParseResult = poParser.Parse(reader);

                if (!poParseResult.Success)
                {
                    Log.Error($"Failed to parse file [{Path.GetFileName(filePath)}]!");
                    Log.Debug(poParseResult.Diagnostics.ToString());
                    throw new IOException($"Failed to parse file [{Path.GetFileName(filePath)}]!");
                }

                var poCatalog = poParseResult.Catalog;

                foreach (var poEntry in poCatalog)
                {
                    if (poEntry is POSingularEntry poSingularEntry)
                    {
                        string[] contextArray = poEntry.Key.ContextId.Split('/');

                        if (contextArray.Length < 2 || contextArray.Length > 2)
                        {
                            Log.Warning($"Skipped! Unexpected PO Context found [{poEntry.Key.ContextId}]!");
                            continue;
                        }

                        string nameSpace = contextArray[0];
                        string key = contextArray[1];
                        uint hash = 0;
                        string original = poEntry.Key.Id;
                        string value = poSingularEntry.Translation;

                        item.Add(new TranslationValue(nameSpace, key, hash, original, value));
                    }
                }
            }
        }
    }
}
