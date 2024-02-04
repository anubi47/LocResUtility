using LocResCore.Translations;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LocResCore.Json.Translations
{
    public class JsonTranslationWriter : ITranslationWriter
    {
        public string[] Extensions { get => new string[] { ".json" }; }

        public void Write(string filePath, TranslationPool pool)
        {
            JsonPool jsonPool = new JsonPool() { Items = new List<JsonItem>() };
            foreach (TranslationItem item in pool)
            {
                JsonItem jsonItem = new JsonItem() { Culture = item.Culture, Namespaces = new List<JsonNamespace>() };
                jsonPool.Items.Add(jsonItem);
                foreach (TranslationValue value in item)
                {
                    JsonNamespace jsonNamespace = jsonItem.Namespaces.FirstOrDefault(_ => string.Equals(_.Name, value.NameSpace));
                    if (jsonNamespace is null)
                    {
                        jsonNamespace = new JsonNamespace() { Name = value.NameSpace, Strings = new List<JsonString>() };
                        jsonItem.Namespaces.Add(jsonNamespace);
                    }
                    jsonNamespace.Strings.Add(new JsonString() { Key = value.Key, Hash = value.Hash, Original = value.OriginalValue, Value = value.Value });
                }
                Log.Information($"Culture [{item.Culture}]. Wrote [{item.Count}] resources.");
            }
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(jsonPool, options);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, json);
            Log.Information($"Wrote [{pool.Count()}] cultures in file [{Path.GetFileName(filePath)}].");
        }

        public void Write(string filePath, TranslationItem item)
        {
            TranslationPool pool = new TranslationPool() { item };
            Write(filePath, pool);
        }
    }
}
