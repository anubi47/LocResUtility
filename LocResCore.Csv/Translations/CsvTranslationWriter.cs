using CsvHelper;
using CsvHelper.Configuration;
using LocResCore.Translations;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LocResCore.Csv.Translations
{
    public class CsvTranslationWriter : ITranslationWriter
    {
        public string[] Extensions { get => new string[] { ".csv" }; }

        public void Write(string filePath, TranslationPool pool)
        {
            List<CsvItem> csvItems = new List<CsvItem>();
            foreach (var item in pool)
            {
                foreach (var value in item)
                {
                    csvItems.Add(new CsvItem()
                    {
                        Culture = item.Culture,
                        NameSpace = value.NameSpace,
                        Key = value.Key,
                        Hash = value.Hash,
                        Original = value.EscapeOriginalValue,
                        Value = value.EscapeValue
                    });
                }
                Log.Information($"Culture [{item.Culture}]. Wrote [{item.Count}] resources.");
            }

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            IWriterConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, configuration))
            {
                csv.WriteRecords(csvItems);
            }
            Log.Information($"Wrote [{pool.Count()}] cultures in file [{Path.GetFileName(filePath)}].");
        }

        public void Write(string filePath, TranslationItem item)
        {
            TranslationPool pool = new TranslationPool() { item };
            Write(filePath, pool);
        }
    }
}
