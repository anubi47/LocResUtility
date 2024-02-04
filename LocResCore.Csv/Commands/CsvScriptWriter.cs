using CsvHelper;
using CsvHelper.Configuration;
using LocResCore.Commands;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LocResCore.Csv.Commands
{
    public class CsvScriptWriter : IScriptWriter
    {
        public string[] Extensions { get => new string[] { ".csv" }; }

        public void Write(string filePath, CommandPool commandPool)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (commandPool is null)
                throw new ArgumentNullException(nameof(commandPool));

            int processed = 0;
            int notProcessed = 0;
            int rowIndex = 2;
            List<CsvCommand> csvCommands = new List<CsvCommand>();
            foreach (var command in commandPool)
            {
                if (command is InsertCommand insert)
                {
                    csvCommands.Add(new CsvCommand()
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
                    csvCommands.Add(new CsvCommand()
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
                    csvCommands.Add(new CsvCommand()
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
                    csvCommands.Add(new CsvCommand()
                    {
                        Command = delete.Type.ToString(),
                        Culture = delete.Culture,
                        Namespace = delete.NameSpace,
                        Key = delete.Key,
                    });
                }
                else if (command is ReplaceCommand replace)
                {
                    csvCommands.Add(new CsvCommand()
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

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            IWriterConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, configuration))
            {
                csv.WriteRecords(csvCommands);
            }

            Log.Information($"Exported [{processed}/{commandPool.Count}] commands to [{Path.GetFileName(filePath)}].");
            if (notProcessed > 0)
            {
                Log.Warning($"Not exported [{notProcessed}/{commandPool.Count}] commands!");
            }
        }
    }
}
