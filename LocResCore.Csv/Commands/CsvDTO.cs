using System.Collections.Generic;

namespace LocResCore.Csv.Commands
{
    internal class CsvCommand
    {
        public string Command { get; set; }
        public string Culture { get; set; }
        public string Namespace { get; set; }
        public string Key { get; set; }
        public uint Hash { get; set; }
        public string Value { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}
