using ClosedXML.Excel;
using LocResCore.Translations;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace LocResCore.Excel.Translations
{
    public class ExcelTranslationWriter : ITranslationWriter
    {
        public string[] Extensions { get => new string[] { ".xlsx", ".xlsm", ".xltx", ".xltm" }; }

        public void Write(string filePath, TranslationPool pool)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (pool is null)
                throw new ArgumentNullException(nameof(pool));

            using (var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook())
            {
                Write(workbook, pool);
                workbook.SaveAs(filePath);
                Log.Information($"Wrote [{Path.GetFileName(filePath)}].");
            }
        }

        public void Write(string filePath, TranslationItem item)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            using (var workbook = File.Exists(filePath) ? new XLWorkbook(filePath) : new XLWorkbook())
            {
                Write(workbook, item);
                workbook.SaveAs(filePath);
            }
        }

        private void Write(IXLWorkbook workbook, TranslationPool pool)
        {
            foreach (TranslationItem culture in pool)
            {
                Write(workbook, culture);
            }
            Log.Information($"Wrote [{pool.Count()}] cultures.");
        }

        private void Write(IXLWorkbook workbook, TranslationItem item)
        {
            int processed = 0;
            int notProcessed = 0;

            WriteSummary(workbook, item);
            IXLWorksheet worksheet = null;
            if (workbook.Worksheets.Contains(item.Culture))
            {
                worksheet = workbook.Worksheet(item.Culture);
            }
            else
            {
                worksheet = workbook.Worksheets.Add(item.Culture);
            }
            worksheet.Clear();
            worksheet.SheetView.FreezeRows(1);
            worksheet.SheetView.FreezeColumns(1);
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Row(1).Style.Font.Bold = true;
            // I could protect the spreadsheet to avoid to make mistakes manually
            // I realized that I passing lot of time to handle the protection manually
            //worksheet.Column(1).Style.Protection.SetLocked(true);
            //worksheet.Column(2).Style.Protection.SetLocked(true);
            //worksheet.Column(3).Style.Protection.SetLocked(true);
            //worksheet.Column(4).Style.Protection.SetHidden(true);
            //worksheet.Column(5).Style.Protection.SetHidden(true);
            //worksheet.Column(6).Style.Protection.SetLocked(false);
            //worksheet.Protect()
            //  .AllowElement(XLSheetProtectionElements.AutoFilter)
            //  .AllowElement(XLSheetProtectionElements.FormatColumns)
            //  .AllowElement(XLSheetProtectionElements.SelectLockedCells)
            //  .AllowElement(XLSheetProtectionElements.SelectUnlockedCells)
            //  .AllowElement(XLSheetProtectionElements.Sort);
            worksheet.Cell(1, 1).Value = "Sts.";
            worksheet.Cell(1, 2).Value = "Namespace";
            worksheet.Cell(1, 3).Value = "Key";
            worksheet.Cell(1, 4).Value = "Hash";
            worksheet.Cell(1, 5).Value = "Original";
            worksheet.Cell(1, 6).Value = "Value";

            int rowIndex = 2;
            foreach (TranslationValue value in item)
            {
                bool warning = false;

                string nameSpace = value.NameSpace;
                if (value.NameSpace.Length > 32767)
                {
                    warning = true;
                    Log.Warning($"Namespace [{value.NameSpace}] text is too long, [{value.NameSpace.Length}] out of [32767] characters allowed! Skipped.");
                    nameSpace = "*** TOO LONG ***";
                }

                string key = value.Key;
                if (value.Key.Length > 32767)
                {
                    warning = true;
                    Log.Warning($"Key [{value.Key}] text is too long, [{value.Key.Length}] out of [32767] characters allowed! Skipped.");
                    key = "*** TOO LONG ***";
                }

                string escapeOriginalValue = value.EscapeOriginalValue;
                if (value.EscapeOriginalValue.Length > 32767)
                {
                    warning = true;
                    Log.Warning($"Source [{value.NameSpace}/{value.Key}] text is too long, [{value.EscapeOriginalValue.Length}] out of [32767] characters allowed! Skipped.");
                    escapeOriginalValue = "*** TOO LONG ***";
                }

                string escapeValue = value.EscapeValue;
                if (value.EscapeValue.Length > 32767)
                {
                    warning = true;
                    Log.Warning($"Target [{value.NameSpace}/{value.Key}] text is too long, [{value.EscapeValue.Length}] out of [32767] characters allowed! Skipped.");
                    escapeValue = "*** TOO LONG ***";
                }

                worksheet.Cell(rowIndex, 1).FormulaR1C1 = $"=IF(OR(ISBLANK(RC[5]),EXACT(RC[4],RC[5])),\"\",\"M\")";
                worksheet.Cell(rowIndex, 2).Value = nameSpace;
                worksheet.Cell(rowIndex, 3).Value = key;
                worksheet.Cell(rowIndex, 4).Value = value.Hash;
                worksheet.Cell(rowIndex, 5).Value = escapeOriginalValue;
                worksheet.Cell(rowIndex, 6).Value = escapeValue;
                rowIndex++;
                if (warning)
                    notProcessed++;
                else
                    processed++;
            }

            worksheet.Column(1).Width = 6;
            worksheet.Column(2).AdjustToContents();
            worksheet.Column(3).AdjustToContents();
            worksheet.Column(4).AdjustToContents();
            worksheet.Column(4).Hide();
            worksheet.Column(5).Hide();
            worksheet.Column(6).Width = 100;
            worksheet.RangeUsed().SetAutoFilter();

            Log.Information($"Culture [{item.Culture}]. Wrote [{processed}/{item.Count}] resources.");
            if (notProcessed > 0)
            {
                Log.Warning($"Culture [{item.Culture}]. Wrote [{notProcessed}/{item.Count}] resources with warning(s)!");
            }
        }

        private void WriteSummary(IXLWorkbook workbook, TranslationItem item)
        {
            if (workbook is null)
                throw new ArgumentNullException(nameof(workbook));
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            IXLWorksheet worksheet;
            if (workbook.Worksheets.Contains("SUMMARY"))
            {
                worksheet = workbook.Worksheet("SUMMARY");
                if (string.Equals(worksheet.Cell(1, 1).GetText(), "Sts.") &&
                    string.Equals(worksheet.Cell(1, 2).GetText(), "Namespace") &&
                    string.Equals(worksheet.Cell(1, 3).GetText(), "Key") &&
                    string.Equals(worksheet.Cell(1, 4).GetText(), "Hash") &&
                    string.Equals(worksheet.Cell(1, 5).GetText(), "Default"))
                {
                    Log.Debug($"Worksheet [{worksheet.Name}] is valid summary sheet!");
                }
                else
                {
                    Log.Warning($"Worksheet [{worksheet.Name}] is not valid summary sheet! Skipped!");
                    Log.Warning($"Columns should be: (1) 'Sts.', (2) 'Namespace', (3) 'Key', (4) 'Hash', (5) 'Default'.");
                    return;
                }
            }
            else
            {
                worksheet = workbook.Worksheets.Add("SUMMARY");
            }
            //worksheet.Clear();
            worksheet.SheetView.FreezeRows(1);
            worksheet.SheetView.FreezeColumns(5);
            worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Row(1).Style.Font.Bold = true;
            // I could protect the spreadsheet to avoid to make mistakes manually
            // I realized that I passing lot of time to handle the protection manually
            //worksheet.Column(1).Style.Protection.SetLocked(true);
            //worksheet.Column(2).Style.Protection.SetLocked(true);
            //worksheet.Column(3).Style.Protection.SetLocked(true);
            //worksheet.Column(4).Style.Protection.SetHidden(true);
            //worksheet.Column(5).Style.Protection.SetHidden(true);
            //worksheet.Protect()
            //  .AllowElement(XLSheetProtectionElements.AutoFilter)
            //  .AllowElement(XLSheetProtectionElements.FormatColumns)
            //  .AllowElement(XLSheetProtectionElements.SelectLockedCells)
            //  .AllowElement(XLSheetProtectionElements.SelectUnlockedCells)
            //  .AllowElement(XLSheetProtectionElements.Sort);
            worksheet.Cell(1, 1).Value = "Sts.";
            worksheet.Cell(1, 2).Value = "Namespace";
            worksheet.Cell(1, 3).Value = "Key";
            worksheet.Cell(1, 4).Value = "Hash";
            worksheet.Cell(1, 5).Value = "Default";

            // Find the total columns used
            int colTotal = worksheet.RangeUsed().ColumnsUsed().Count();
            if (colTotal == 5)
            {
                int rowIndex = 2;
                foreach (TranslationValue value in item)
                {
                    worksheet.Cell(rowIndex, 2).Value = value.NameSpace;
                    worksheet.Cell(rowIndex, 3).Value = value.Key;
                    worksheet.Cell(rowIndex, 4).Value = value.Hash;
                    if (value.EscapeOriginalValue.Length <= 32767)
                    {
                        worksheet.Cell(rowIndex, 5).Value = value.EscapeOriginalValue;
                    }
                    else
                    {
                        worksheet.Cell(rowIndex, 5).Value = "*** TOO LONG ***";
                    }
                    rowIndex++;
                }
            }

            // Find the first free column
            int colIndex;
            for (colIndex = 6; colIndex <= colTotal + 1; colIndex++)
            {
                if (worksheet.Cell(1, colIndex).IsEmpty())
                {
                    break;
                }
                string cultureName = worksheet.Cell(1, colIndex).GetText();
                if (string.Equals(cultureName, item.Culture))
                {
                    break;
                }
                if (string.IsNullOrEmpty(cultureName))
                {
                    break;
                }
            }
            worksheet.Cell(1, colIndex).Value = item.Culture;

            colTotal = worksheet.RangeUsed().ColumnsUsed().Count();

            // Write the formulas
            foreach (var row in worksheet.RangeUsed().RowsUsed().Skip(1))
            {
                // The ClosedXML does not support XLOOKUP function
                // I leave here for future reference
                //row.Cell(colIndex).FormulaA1 = $"=XLOOKUP($C{row.RowNumber()},'{data.Culture}'!C:C,'{data.Culture}'!A:A,\"E\",0,1)";
                row.Cell(1).FormulaR1C1 = $"=IF(COUNTIF(RC[5]:RC[{colTotal - 1}],\"M\")={colTotal - 5},\"M\",IF(COUNTIF(RC[5]:RC[{colTotal - 1}],\"M\")>0,\"E\",\"\"))";
                row.Cell(colIndex).FormulaA1 = $"=INDEX('{item.Culture}'!A:A,MATCH($C{row.RowNumber()},'{item.Culture}'!C:C,0))";
            }

            worksheet.Column(1).Width = 6;
            worksheet.Column(2).AdjustToContents();
            worksheet.Column(3).AdjustToContents();
            worksheet.Column(4).AdjustToContents();
            worksheet.Column(4).Hide();
            worksheet.Column(5).Hide();
            worksheet.RangeUsed().SetAutoFilter();
        }
    }
}
