namespace ocrApplication;

using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using System.Collections.Generic;
using System.IO;

public static class ExecutionTimeLogger
{
    public static void SaveExecutionTimesToExcel(string filePath, List<(string ImageName, string Method, double TimeTaken)> executionTimes)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage package = new ExcelPackage())
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Execution Times");

            // Headers
            worksheet.Cells[1, 1].Value = "Preprocessing Method";
            worksheet.Cells[1, 2].Value = "Execution Time (ms)";

            // Data
            for (int i = 0; i < executionTimes.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = executionTimes[i].Method;
                worksheet.Cells[i + 2, 2].Value = executionTimes[i].TimeTaken;
            }

            // AutoFit columns for better readability
            worksheet.Cells.AutoFitColumns();

            // Create Histogram Chart
            var chart = worksheet.Drawings.AddChart("ExecutionTimeHistogram", eChartType.ColumnClustered);
            chart.Title.Text = "Preprocessing Techniques Execution Time";
            chart.SetPosition(1, 0, 4, 0); // Position at column D
            chart.SetSize(800, 400); // Set chart size

            // Set X-Axis labels (Preprocessing Methods)
            var xRange = worksheet.Cells[2, 1, executionTimes.Count + 1, 1]; // Method Names
            var yRange = worksheet.Cells[2, 2, executionTimes.Count + 1, 2]; // Execution Times

            var series = chart.Series.Add(yRange, xRange);
            series.Header = "Execution Time (ms)"; // Legend Name

            // Formatting
            chart.YAxis.Title.Text = "Execution Time (ms)";
            chart.XAxis.Title.Text = "Preprocessing Technique";
            chart.YAxis.MinValue = 0; // Start from zero
            chart.Legend.Position = eLegendPosition.Bottom;

            // Save the Excel file
            File.WriteAllBytes(filePath, package.GetAsByteArray());
        }
    }
}
