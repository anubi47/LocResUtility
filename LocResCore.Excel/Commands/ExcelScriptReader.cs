using ClosedXML.Excel;
using LocResCore.Commands;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace LocResCore.Excel.Commands
{
    public class ExcelScriptReader : IScriptReader
    {
        public string[] Extensions { get => new string[] { ".xlsx", ".xlsm", ".xltx", ".xltm" }; }

        public void Read(out CommandPool commandPool, string filePath)
        {
            commandPool = new CommandPool();
            using (var workbook = new XLWorkbook(filePath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    if (string.Equals(worksheet.Cell(1, 1).GetText(), "Command") &&
                        string.Equals(worksheet.Cell(1, 2).GetText(), "Culture") &&
                        string.Equals(worksheet.Cell(1, 3).GetText(), "Namespace") &&
                        string.Equals(worksheet.Cell(1, 4).GetText(), "Key") &&
                        string.Equals(worksheet.Cell(1, 5).GetText(), "Hash") &&
                        string.Equals(worksheet.Cell(1, 6).GetText(), "Value") &&
                        string.Equals(worksheet.Cell(1, 7).GetText(), "OldValue") &&
                        string.Equals(worksheet.Cell(1, 8).GetText(), "NewValue"))
                    {
                        Log.Information($"Worksheet [{worksheet.Name}] is valid command sheet!");
                    }
                    else
                    {
                        Log.Warning($"Worksheet [{worksheet.Name}] is not valid command sheet! Skipped!");
                        Log.Warning($"Columns should be: (1) 'Command', (2) 'Culture', (3) 'Namespace', (4) 'Key', (5) 'Hash', (6) 'Value', (7) 'OldValue', (8) 'NewValue'.");
                        continue;
                    }

                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row
                    foreach (var row in rows)
                    {
                        try
                        {
                            string commandName = row.Cell(1).GetValue<string>() ?? string.Empty;
                            string lrCulture = row.Cell(2).GetValue<string>() ?? string.Empty;
                            string lrNamespace = row.Cell(3).GetValue<string>() ?? string.Empty;
                            string lrKey = row.Cell(4).GetValue<string>() ?? string.Empty;

                            if (string.Equals(commandName, CommandType.Insert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                uint lrHash = row.Cell(5).GetValue<uint>();
                                string lrValue = row.Cell(6).GetValue<string>() ?? string.Empty;
                                ICommand command = new InsertCommand(lrCulture, lrNamespace, lrKey, lrHash, lrValue);
                                commandPool.Add(command);
                            }
                            else if (string.Equals(commandName, CommandType.Update.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                string lrValue = row.Cell(6).GetValue<string>() ?? string.Empty;
                                ICommand command = new UpdateCommand(lrCulture, lrNamespace, lrKey, lrValue);
                                commandPool.Add(command);
                            }
                            else if (string.Equals(commandName, CommandType.Upsert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                            {
                                uint lrHash = row.Cell(5).GetValue<uint>();
                                string lrValue = row.Cell(6).GetValue<string>() ?? string.Empty;
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
                                string lrOldValue = row.Cell(7).GetValue<string>() ?? string.Empty;
                                string lrNewValue = row.Cell(8).GetValue<string>() ?? string.Empty;
                                ICommand command = new ReplaceCommand(lrCulture, lrNamespace, lrKey, lrOldValue, lrNewValue);
                                commandPool.Add(command);
                            }
                            else
                            {
                                Log.Error($"Unexpected command name [{commandName}] at [{row.RowNumber()}] row! Command skipped!");
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Exception trying to read the command at [{row.RowNumber()}] row! Command skipped!");
                        }
                    }
                }
            }
            Log.Information($"Imported [{commandPool.Count}] commands from [{Path.GetFileName(filePath)}].");
        }
    }
}
