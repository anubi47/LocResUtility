using ClosedXML.Excel;
using LocResCore.Translations;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace LocResCore.Excel.Translations
{
    public class ExcelTranslationReader : ITranslationReader
    {
        public string[] Extensions { get => new string[] { ".xlsx", ".xlsm", ".xltx", ".xltm" }; }

        public void Read(out TranslationPool game, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var workbook = new XLWorkbook(filePath))
            {
                Read(out game, workbook);
                Log.Information($"Read [{Path.GetFileName(filePath)}].");
            }
        }

        public void Read(out TranslationItem item, string filePath, string culture)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var workbook = new XLWorkbook(filePath))
            {
                if (workbook.Worksheets.Contains(culture))
                {
                    var worksheet = workbook.Worksheet(culture);
                    Read(out item, worksheet);
                    Log.Information($"Read [{Path.GetFileName(filePath)}].");
                }
                else
                {
                    item = new TranslationItem(culture);
                    Log.Warning($"Excel file [{Path.GetFileName(filePath)}] does not have a worksheet with name [{culture}]!");
                }
            }
        }

        private void Read(out TranslationPool pool, IXLWorkbook workbook)
        {
            pool = new TranslationPool();
            foreach (var worksheet in workbook.Worksheets)
            {
                if (string.Equals(worksheet.Cell(1, 1).GetText(), "Sts.") &&
                    string.Equals(worksheet.Cell(1, 2).GetText(), "Namespace") &&
                    string.Equals(worksheet.Cell(1, 3).GetText(), "Key") &&
                    string.Equals(worksheet.Cell(1, 4).GetText(), "Hash") &&
                    string.Equals(worksheet.Cell(1, 5).GetText(), "Original") &&
                    string.Equals(worksheet.Cell(1, 6).GetText(), "Value"))
                {
                    Log.Information($"Worksheet [{worksheet.Name}] is valid culture sheet!");
                }
                else
                {
                    Log.Warning($"Worksheet [{worksheet.Name}] is not valid culture sheet! Skipped!");
                    Log.Warning($"Columns should be: (1) 'Sts.', (2) 'Namespace', (3) 'Key', (4) 'Hash', (5) 'Original', (6) Value.");
                    continue;
                }

                Read(out TranslationItem culture, worksheet);
                pool.Add(culture);
            }
            Log.Information($"Read [{pool.Count()}] cultures.");
        }

        private void Read(out TranslationItem item, IXLWorksheet worksheet)
        {
            item = new TranslationItem(worksheet.Name);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header row
            foreach (var row in rows)
            {
                string nameSpace = row.Cell(2).GetValue<string>() ?? string.Empty;
                string key = row.Cell(3).GetValue<string>() ?? string.Empty;
                uint hash = row.Cell(4).GetValue<uint>();
                string original = row.Cell(5).GetValue<string>() ?? string.Empty;
                string value = row.Cell(6).GetValue<string>() ?? string.Empty;
                item.Add(new TranslationValue(nameSpace, key, hash, original, value));
            }
            Log.Information($"Culture [{item.Culture}]. Read [{item.Count}] resources for [{item.Culture}] culture.");
        }
    }
}
