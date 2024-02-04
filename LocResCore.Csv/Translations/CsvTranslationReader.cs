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
    public class CsvTranslationReader : ITranslationReader
    {
        public string[] Extensions { get => new string[] { ".csv" }; }

        public void Read(out TranslationPool pool, string filePath)
        {
            Dictionary<string, uint> countDic = new Dictionary<string, uint>();
            pool = new TranslationPool();
            IReaderConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<CsvItem>();
                foreach (var record in records)
                {
                    string culture = record.Culture;
                    if (!pool.TryGetItem(culture, out TranslationItem item))
                    {
                        item = new TranslationItem(culture);
                        pool.Add(item);
                        countDic.Add(culture, 0);
                    }
                    item.Add(new TranslationValue(record.NameSpace, record.Key, record.Hash, record.Original, record.Value));
                    countDic[culture]++;
                }
            }
            foreach (var count in countDic)
                Log.Information($"Culture [{count.Key}]. Read [{count.Value}] resources.");
            Log.Information($"Read [{pool.Count()}] cultures in file [{Path.GetFileName(filePath)}].");
        }

        public void Read(out TranslationItem item, string filePath, string culture)
        {
            Read(out TranslationPool pool, filePath);
            if (!pool.TryGetItem(culture, out item))
                item = new TranslationItem(culture);
        }
    }
}
