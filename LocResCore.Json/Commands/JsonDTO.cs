using System.Collections.Generic;

namespace LocResCore.Json.Commands
{
    internal class JsonCommand
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

    internal class JsonCommandPool : List<JsonCommand>
    {
    }
}
