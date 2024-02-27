using Fic.XTB.FlowExecutionHistory.Models;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fic.XTB.FlowExecutionHistory.Services
{
    public static class ExcelService
    {
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

            var range = sh.Range["A1", $"G{flowRuns.Count + 1}"];
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
