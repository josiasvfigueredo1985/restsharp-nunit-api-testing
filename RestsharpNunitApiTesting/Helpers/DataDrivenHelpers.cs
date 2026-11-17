using ClosedXML.Excel;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace RestSharpApiTesting.Helpers
{
    public static class DataDrivenHelpers
    {
        public static List<TestCaseData> GetExcelData(string excelFilePath, int worksheetNumber)
        {
            using var workbook = new XLWorkbook(excelFilePath);
            var worksheet = workbook.Worksheet(worksheetNumber);
            int columnCount = worksheet.ColumnsUsed().Count();
            var row = new List<string>(columnCount);
            string[] columnLetters = { "A", "B", "C", "D", "E", "F" };

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                row.Add(worksheet.Cell($"{columnLetters[columnIndex]}2").GetValue<string>());
            }

            return [new TestCaseData(row.ToArray())];
        }
    }
}
