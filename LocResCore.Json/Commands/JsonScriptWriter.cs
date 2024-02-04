using LocResCore.Commands;
using Serilog;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LocResCore.Json.Commands
{
    public class JsonScriptWriter : IScriptWriter
    {
        public string[] Extensions { get => new string[] { ".json" }; }

        public void Write(string filePath, CommandPool commandPool)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (commandPool is null)
                throw new ArgumentNullException(nameof(commandPool));

            int processed = 0;
            int notProcessed = 0;
            int rowIndex = 2;
            JsonCommandPool jsonCommandPool = new JsonCommandPool();
            foreach (var command in commandPool)
            {
                if (command is InsertCommand insert)
                {
                    jsonCommandPool.Add(new JsonCommand()
                    {
                        Command = insert.Type.ToString(),
                        Culture = insert.Culture,
                        Namespace = insert.NameSpace,
                        Key = insert.Key,
                        Hash = insert.Hash,
                        Value = insert.Value
                    });
                }
                else if (command is UpdateCommand update)
                {
                    jsonCommandPool.Add(new JsonCommand()
                    {
                        Command = update.Type.ToString(),
                        Culture = update.Culture,
                        Namespace = update.NameSpace,
                        Key = update.Key,
                        Value = update.Value
                    });
                }
                else if (command is UpsertCommand upsert)
                {
                    jsonCommandPool.Add(new JsonCommand()
                    {
                        Command = upsert.Type.ToString(),
                        Culture = upsert.Culture,
                        Namespace = upsert.NameSpace,
                        Key = upsert.Key,
                        Hash = upsert.Hash,
                        Value = upsert.Value
                    });
                }
                else if (command is DeleteCommand delete)
                {
                    jsonCommandPool.Add(new JsonCommand()
                    {
                        Command = delete.Type.ToString(),
                        Culture = delete.Culture,
                        Namespace = delete.NameSpace,
                        Key = delete.Key,
                    });
                }
                else if (command is ReplaceCommand replace)
                {
                    jsonCommandPool.Add(new JsonCommand()
                    {
                        Command = replace.Type.ToString(),
                        Culture = replace.Culture,
                        Namespace = replace.NameSpace,
                        Key = replace.Key,
                        OldValue = replace.OldValue,
                        NewValue = replace.NewValue
                    });
                }
                else
                {
                    notProcessed++;
                    Log.Warning($"Unknown Command! Skipped.");
                    continue;
                }
                rowIndex++;
                processed++;
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            string json = JsonSerializer.Serialize(jsonCommandPool, options);
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, json);

            Log.Information($"Exported [{processed}/{commandPool.Count}] commands to [{Path.GetFileName(filePath)}].");
            if (notProcessed > 0)
            {
                Log.Warning($"Not exported [{notProcessed}/{commandPool.Count}] commands!");
            }
        }
    }
}
