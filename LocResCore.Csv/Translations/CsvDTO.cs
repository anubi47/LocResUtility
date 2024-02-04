using CsvHelper.Configuration.Attributes;

namespace LocResCore.Csv.Translations
{
    internal class CsvItem
    {
        [Index(0)]
        public string Culture {  get; set; }
        [Index(1)]
        public string NameSpace { get; set; }
        [Index(2)]
        public string Key { get; set; }
        [Index(3)]
        public uint Hash { get; set; }
        [Index(4)]
        public string Original { get; set; }
        [Index(5)]
        public string Value { get; set; }
    }
}
