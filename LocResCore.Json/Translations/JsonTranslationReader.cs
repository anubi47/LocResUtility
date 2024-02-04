using LocResCore.Translations;
using Serilog;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LocResCore.Json.Translations
{
    public class JsonTranslationReader : ITranslationReader
    {
        public string[] Extensions { get => new string[] { ".json" }; }

        public void Read(out TranslationPool pool, string filePath)
        {
            string json = File.ReadAllText(filePath);
            JsonPool jsonPool = JsonSerializer.Deserialize<JsonPool>(json);
            pool = new TranslationPool();
            foreach (JsonItem jsonItem in jsonPool.Items)
            {
                TranslationItem item = new TranslationItem(jsonItem.Culture);
                foreach (JsonNamespace jsonNamespace in jsonItem.Namespaces)
                {
                    foreach (JsonString jsonString in jsonNamespace.Strings)
                    {
                        item.Add(new TranslationValue(jsonNamespace.Name, jsonString.Key, jsonString.Hash, jsonString.Original, jsonString.Value));
                    }
                }
                pool.Add(item);
                Log.Information($"Culture [{item.Culture}]. Read [{item.Count}] resources.");
            }
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
