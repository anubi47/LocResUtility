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
    public class CsvScriptReader : IScriptReader
    {
        public string[] Extensions { get => new string[] { ".csv" }; }

        public void Read(out CommandPool commandPool, string filePath)
        {
            Dictionary<string, uint> countDic = new Dictionary<string, uint>();
            commandPool = new CommandPool();
            IReaderConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, configuration))
            {
                var records = csv.GetRecords<CsvCommand>();
                int index = 0;
                foreach (var record in records)
                {
                    try
                    {
                        if (string.Equals(record.Command, CommandType.Insert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            ICommand command = new InsertCommand(record.Culture, record.Namespace, record.Key, record.Hash, record.Value);
                            commandPool.Add(command);
                        }
                        else if (string.Equals(record.Command, CommandType.Update.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            ICommand command = new UpdateCommand(record.Culture, record.Namespace, record.Key, record.Value);
                            commandPool.Add(command);
                        }
                        else if (string.Equals(record.Command, CommandType.Upsert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            ICommand command = new UpsertCommand(record.Culture, record.Namespace, record.Key, record.Hash, record.Value);
                            commandPool.Add(command);
                        }
                        else if (string.Equals(record.Command, CommandType.Delete.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            ICommand command = new DeleteCommand(record.Culture, record.Namespace, record.Key);
                            commandPool.Add(command);
                        }
                        else if (string.Equals(record.Command, CommandType.Replace.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            ICommand command = new ReplaceCommand(record.Culture, record.Namespace, record.Key, record.OldValue, record.NewValue);
                            commandPool.Add(command);
                        }
                        else
                        {
                            Log.Error($"Unexpected command name [{record.Command}] at [{index}] index! Command skipped!");
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
}
