using ClosedXML.Excel;
using LocResCore.Commands;
using Serilog;
using System;
using System.IO;

namespace LocResCore.Excel.Commands
{
    public class ExcelScriptWriter : IScriptWriter
    {
        public string[] Extensions { get => new string[] { ".xlsx", ".xlsm", ".xltx", ".xltm" }; }

        public void Write(string filePath, CommandPool commandPool)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (commandPool is null)
                throw new ArgumentNullException(nameof(commandPool));

            int processed = 0;
            int notProcessed = 0;
            using (var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook())
            {
                IXLWorksheet worksheet = null;
                if (workbook.Worksheets.Contains("COMMANDS"))
                {
                    worksheet = workbook.Worksheet("COMMANDS");
                }
                else
                {
                    worksheet = workbook.Worksheets.Add("COMMANDS");
                }
                worksheet.Clear();
                worksheet.SheetView.FreezeRows(1);
                worksheet.SheetView.FreezeColumns(5);
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Cell(1, 1).Value = "Command";
                worksheet.Cell(1, 2).Value = "Culture";
                worksheet.Cell(1, 3).Value = "Namespace";
                worksheet.Cell(1, 4).Value = "Key";
                worksheet.Cell(1, 5).Value = "Hash";
                worksheet.Cell(1, 6).Value = "Value";
                worksheet.Cell(1, 7).Value = "OldValue";
                worksheet.Cell(1, 8).Value = "NewValue";

                int rowIndex = 2;
                foreach (var command in commandPool)
                {
                    string type = string.Empty;
                    string culture = string.Empty;
                    string nameSpace = string.Empty;
                    string key = string.Empty;
                    string hash = string.Empty;
                    string value = string.Empty;
                    string oldValue = string.Empty;
                    string newValue = string.Empty;

                    if (command is InsertCommand insert)
                    {
                        type = insert.Type.ToString();
                        culture = insert.Culture;
                        nameSpace = insert.NameSpace;
                        key = insert.Key;
                        hash = insert.Hash.ToString();
                        value = insert.Value;
                    }
                    else if (command is UpdateCommand update)
                    {
                        type = update.Type.ToString();
                        culture = update.Culture;
                        nameSpace = update.NameSpace;
                        key = update.Key;
                        value = update.EscapeValue;
                    }
                    else if (command is UpsertCommand upsert)
                    {
                        type = upsert.Type.ToString();
                        culture = upsert.Culture;
                        nameSpace = upsert.NameSpace;
                        key = upsert.Key;
                        hash = upsert.Hash.ToString();
                        value = upsert.Value;
                    }
                    else if (command is DeleteCommand delete)
                    {
                        type = delete.Type.ToString();
                        culture = delete.Culture;
                        nameSpace = delete.NameSpace;
                        key = delete.Key;
                    }
                    else if (command is ReplaceCommand replace)
                    {
                        type = replace.Type.ToString();
                        culture = replace.Culture;
                        nameSpace = replace.NameSpace;
                        key = replace.Key;
                        oldValue = replace.OldValue;
                        newValue = replace.NewValue;
                    }
                    else
                    {
                        notProcessed++;
                        Log.Warning($"Unknown Command! Skipped.");
                        continue;
                    }

                    if (type.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [Type] text is too long, [{type.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (culture.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [Culture] text is too long, [{culture.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (nameSpace.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [Namespace] text is too long, [{nameSpace.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (key.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [Key] text is too long, [{key.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (hash.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [Hash] text is too long, [{hash.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (value.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [Value] text is too long, [{value.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (oldValue.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [OldValue] text is too long, [{oldValue.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }
                    if (newValue.Length > 32767)
                    {
                        notProcessed++;
                        Log.Warning($"Command [NewValue] text is too long, [{newValue.Length}] out of [32767] characters allowed! Skipped.");
                        continue;
                    }

                    worksheet.Cell(rowIndex, 1).Value = type;
                    worksheet.Cell(rowIndex, 2).Value = culture;
                    worksheet.Cell(rowIndex, 3).Value = nameSpace;
                    worksheet.Cell(rowIndex, 4).Value = key;
                    worksheet.Cell(rowIndex, 5).Value = hash;
                    worksheet.Cell(rowIndex, 6).Value = value;
                    worksheet.Cell(rowIndex, 7).Value = oldValue;
                    worksheet.Cell(rowIndex, 8).Value = newValue;

                    rowIndex++;
                    processed++;
                }

                worksheet.Column(1).AdjustToContents();
                worksheet.Column(2).AdjustToContents();
                worksheet.Column(3).AdjustToContents();
                worksheet.Column(4).AdjustToContents();
                worksheet.Column(5).AdjustToContents();
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 20;
                worksheet.RangeUsed().SetAutoFilter();
                workbook.SaveAs(filePath);
            }
            Log.Information($"Exported [{processed}/{commandPool.Count}] commands to [{Path.GetFileName(filePath)}].");
            if (notProcessed > 0)
            {
                Log.Warning($"Not exported [{notProcessed}/{commandPool.Count}] commands!");
            }
        }
    }
}
