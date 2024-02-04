using System.Collections.Generic;

namespace LocResCore.Json.Translations
{
    internal class JsonPool
    {
        public List<JsonItem> Items { get; set; }
    }

    internal class JsonItem
    {
        public string Culture { get; set; }
        public List<JsonNamespace> Namespaces { get; set; }
    }

    internal class JsonNamespace
    {
        public string Name { get; set; }
        public List<JsonString> Strings { get; set; }
    }

    internal class JsonString
    {
        public string Key { get; set; }
        public uint Hash { get; set; }
        public string Original { get; set; }
        public string Value { get; set; }
    }
}
