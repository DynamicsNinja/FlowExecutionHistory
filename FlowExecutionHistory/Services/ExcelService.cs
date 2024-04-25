using Fic.XTB.FlowExecutionHistory.Forms;
using Fic.XTB.FlowExecutionHistory.Models;
using Microsoft.Office.Interop.Excel;
using OfficeOpenXml;
using OfficeOpenXml.ConditionalFormatting;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Fic.XTB.FlowExecutionHistory.Services
{
    public static class ExcelService
    {
        public static void ExportToExcelNew(List<FlowRun> flowRuns, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(filePath))
            {
                var sheet = package.Workbook.Worksheets.Add("Flow Runs");

                sheet.Cells[1, 1].Value = "Id";
                sheet.Cells[1, 2].Value = "Flow Name";
                sheet.Cells[1, 3].Value = "Status";
                sheet.Cells[1, 4].Value = "Start Date";
                sheet.Cells[1, 5].Value = "Duration in milliseconds";
                sheet.Cells[1, 6].Value = "Url";
                sheet.Cells[1, 7].Value = "Error";

                var triggerOuputColumns = new List<string>();

                for (var index = 0; index < flowRuns.Count; index++)
                {
                    var row = flowRuns[index];

                    sheet.Cells[index + 2, 1].Value = row.Id;
                    sheet.Cells[index + 2, 2].Value = row.Flow.Name;
                    sheet.Cells[index + 2, 3].Value = row.Status;

                    sheet.Cells[index + 2, 4].Value = row.StartDate.DateTime;
                    sheet.Cells[index + 2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.FullDateTimePattern;

                    sheet.Cells[index + 2, 5].Value = row.DurationInMilliseconds;
                    sheet.Cells[index + 2, 6].Hyperlink = new Uri(row.Url);
                    sheet.Cells[index + 2, 7].Value = row.Error?.Details;

                    if (row.TriggerOutputs == null || row.TriggerOutputs.Body == null) { continue; }

                    var triggerOutputs = row.TriggerOutputs.Body;

                    foreach (var key in triggerOutputs.Keys)
                    {
                        var columnIndex = triggerOuputColumns.IndexOf(key);

                        if (columnIndex == -1)
                        {
                            triggerOuputColumns.Add(key);
                            columnIndex = triggerOuputColumns.Count - 1;
                        }

                        sheet.Cells[index + 2, columnIndex + 8].Value = triggerOutputs[key];
                    }
                }

                for (var toColumnIndex = 0; toColumnIndex < triggerOuputColumns.Count; toColumnIndex++)
                {
                    sheet.Cells[1, toColumnIndex + 8].Value = "TO: " + triggerOuputColumns[toColumnIndex];
                }

                var range = sheet.Cells[1, 1, sheet.Dimension.End.Row, sheet.Dimension.End.Column];
                var tab = sheet.Tables.Add(range, "Table1");
                tab.TableStyle = OfficeOpenXml.Table.TableStyles.Medium2;

                var successConditionalFormatting = sheet.ConditionalFormatting.AddEqual(sheet.Cells["C2:C" + (flowRuns.Count + 1)]);
                successConditionalFormatting.Formula = $"\"{Enums.FlowRunStatus.Succeeded}\"";
                successConditionalFormatting.Style.Fill.PatternType = ExcelFillStyle.Solid;
                successConditionalFormatting.Style.Fill.BackgroundColor.Color = Color.Green;

                var failureConditionalFormatting = sheet.ConditionalFormatting.AddEqual(sheet.Cells["C2:C" + (flowRuns.Count + 1)]);
                failureConditionalFormatting.Formula = $"\"{Enums.FlowRunStatus.Failed}\"";
                failureConditionalFormatting.Style.Fill.PatternType = ExcelFillStyle.Solid;
                failureConditionalFormatting.Style.Fill.BackgroundColor.Color = Color.Red;

                package.Save();
            }
        }
        public static void ExportToExcel(List<FlowRun> flowRuns, string filePath)
        {
            var excel = new Application();
            var wb = excel.Workbooks.Add();
            var sh = (Worksheet)wb.Sheets.Add();
            sh.Name = "Flow Runs";

            sh.Cells[1, 1] = "Id";
            sh.Cells[1, 2] = "Flow Name";
            sh.Cells[1, 3] = "Status";
            sh.Cells[1, 4] = "Start Date";
            sh.Cells[1, 5] = "Duration in milliseconds";
            sh.Cells[1, 6] = "Url";
            sh.Cells[1, 7] = "Error";

            for (var index = 0; index < flowRuns.Count; index++)
            {
                var row = flowRuns[index];

                sh.Cells[index + 2, "A"] = row.Id;
                sh.Cells[index + 2, "B"] = row.Flow.Name;
                sh.Cells[index + 2, "C"] = row.Status;
                sh.Cells[index + 2, "D"] = row.StartDate.DateTime;
                sh.Cells[index + 2, "E"] = row.DurationInMilliseconds;

                // Add hyperlink to the cell containing the URL
                var cell = (Range)sh.Cells[index + 2, "F"];
                sh.Hyperlinks.Add(cell, row.Url);

                sh.Cells[index + 2, "G"] = row.Error?.Details;
            }

            var statusColumn = sh.Range["C2:C" + (flowRuns.Count + 1)];
            var successCondition = (FormatCondition)statusColumn.FormatConditions.Add(
                XlFormatConditionType.xlExpression,
                XlFormatConditionOperator.xlEqual,
                $"=$C2=\"{Enums.FlowRunStatus.Succeeded}\""
            );

            successCondition.Interior.Color = ColorTranslator.ToOle(Color.Green);
            successCondition.Font.Bold = true;

            var failureCondition = (FormatCondition)statusColumn.FormatConditions.Add(
                XlFormatConditionType.xlExpression,
                XlFormatConditionOperator.xlEqual,
                $"=$C2=\"{Enums.FlowRunStatus.Failed}\""
            );

            failureCondition.Interior.Color = ColorTranslator.ToOle(Color.Red);
            failureCondition.Font.Bold = true;

            var range = sh.Range["A1", $"F{flowRuns.Count + 1}"];
            FormatAsTable(range, "Table1", "TableStyleMedium15");

            range.Columns.AutoFit();

            excel.DisplayAlerts = false;
            wb.SaveAs(filePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, XlSaveAsAccessMode.xlNoChange, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            wb.Close(true);
            excel.Quit();
        }

        private static void FormatAsTable(Range sourceRange, string tableName, string tableStyleName)
        {
            sourceRange.Worksheet.ListObjects.Add(XlListObjectSourceType.xlSrcRange,
                    sourceRange, Type.Missing, XlYesNoGuess.xlYes, Type.Missing).Name =
                tableName;
            sourceRange.Select();
            sourceRange.Worksheet.ListObjects[tableName].TableStyle = tableStyleName;
        }
    }
}
