using LocResCore.Commands;
using Serilog;
using System;
using System.IO;
using System.Text.Json;

namespace LocResCore.Json.Commands
{
    public class JsonScriptReader : IScriptReader
    {
        public string[] Extensions { get => new string[] { ".json" }; }

        public void Read(out CommandPool commandPool, string filePath)
        {
            commandPool = new CommandPool();
            string json = File.ReadAllText(filePath);
            JsonElement jsonArray = JsonSerializer.Deserialize<JsonElement>(json);
            int index = 0;
            foreach (var jsonElement in jsonArray.EnumerateArray())
            {
                try
                {
                    string commandName = jsonElement.GetProperty("Command").GetString();
                    string lrCulture = jsonElement.GetProperty("Culture").GetString();
                    string lrNamespace = jsonElement.GetProperty("Namespace").GetString();
                    string lrKey = jsonElement.GetProperty("Key").GetString();
                    if (string.Equals(commandName, CommandType.Insert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        uint lrHash = jsonElement.GetProperty("Hash").GetUInt32();
                        string lrValue = jsonElement.GetProperty("Value").GetString();
                        ICommand command = new InsertCommand(lrCulture, lrNamespace, lrKey, lrHash, lrValue);
                        commandPool.Add(command);
                    }
                    else if (string.Equals(commandName, CommandType.Update.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        string lrValue = jsonElement.GetProperty("Value").GetString();
                        ICommand command = new UpdateCommand(lrCulture, lrNamespace, lrKey, lrValue);
                        commandPool.Add(command);
                    }
                    else if (string.Equals(commandName, CommandType.Upsert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        uint lrHash = jsonElement.GetProperty("Hash").GetUInt32();
                        string lrValue = jsonElement.GetProperty("Value").GetString();
                        ICommand command = new UpsertCommand(lrCulture, lrNamespace, lrKey, lrHash, lrValue);
                        commandPool.Add(command);
                    }
                    else if (string.Equals(commandName, CommandType.Delete.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        ICommand command = new DeleteCommand(lrCulture, lrNamespace, lrKey);
                        commandPool.Add(command);
                    }
                    else if (string.Equals(commandName, CommandType.Replace.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        string lrOldValue = jsonElement.GetProperty("OldValue").GetString();
                        string lrNewValue = jsonElement.GetProperty("NewValue").GetString();
                        ICommand command = new ReplaceCommand(lrCulture, lrNamespace, lrKey, lrOldValue, lrNewValue);
                        commandPool.Add(command);
                    }
                    else
                    {
                        Log.Error($"Unexpected command name [{commandName}] at [{index}] index! Command skipped!");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Exception trying to read the command at [{index}] index! Command skipped!");
                }
                index++;
            }
        }
    }   
}
