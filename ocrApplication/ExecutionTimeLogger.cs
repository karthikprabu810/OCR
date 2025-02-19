using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace ocrApplication;

public static class ExecutionTimeLogger
{
    public static void SaveExecutionTimesToExcel(string filePath, 
        List<(string ImageName, string Method, double TimeTaken)> preprocessingTimes, 
        List<(string ImageName, string OCRTool, double TimeTaken)> ocrExecutionTimes)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage package = new ExcelPackage())
        {
            // **Sheet 1: Preprocessing Execution Times**
            var preprocessingSheet = package.Workbook.Worksheets.Add("Preprocessing Times");

            preprocessingSheet.Cells[1, 1].Value = "Preprocessing Method";
            preprocessingSheet.Cells[1, 2].Value = "Execution Time (ms)";

            for (int i = 0; i < preprocessingTimes.Count; i++)
            {
                preprocessingSheet.Cells[i + 2, 1].Value = preprocessingTimes[i].Method;
                preprocessingSheet.Cells[i + 2, 2].Value = preprocessingTimes[i].TimeTaken;
            }

            preprocessingSheet.Cells.AutoFitColumns();

            // Add histogram for preprocessing methods
            var preprocessingChart = preprocessingSheet.Drawings.AddChart("PreprocessingTimeHistogram", eChartType.ColumnClustered);
            preprocessingChart.Title.Text = "Preprocessing Execution Time";
            preprocessingChart.SetPosition(1, 0, 4, 0);
            preprocessingChart.SetSize(800, 400);

            var xRangePreprocess = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1]; 
            var yRangePreprocess = preprocessingSheet.Cells[2, 2, preprocessingTimes.Count + 1, 2];

            var preprocessSeries = preprocessingChart.Series.Add(yRangePreprocess, xRangePreprocess);
            preprocessSeries.Header = "Execution Time (ms)";

            preprocessingChart.YAxis.Title.Text = "Execution Time (ms)";
            preprocessingChart.XAxis.Title.Text = "Preprocessing Technique";
            preprocessingChart.YAxis.MinValue = 0;
            preprocessingChart.Legend.Position = eLegendPosition.Bottom;
            
            // **Sheet 2: OCR Execution Times**
            var ocrSheet = package.Workbook.Worksheets.Add("OCR Execution Times");

            ocrSheet.Cells[1, 1].Value = "Image Filter";
            ocrSheet.Cells[1, 2].Value = "Execution Time (ms)";

            for (int i = 0; i < ocrExecutionTimes.Count; i++)
            {
                ocrSheet.Cells[i + 2, 1].Value = ocrExecutionTimes[i].OCRTool;
                ocrSheet.Cells[i + 2, 2].Value = ocrExecutionTimes[i].TimeTaken;
            }

            ocrSheet.Cells.AutoFitColumns();

            // Add histogram for OCR times
            var ocrChart = ocrSheet.Drawings.AddChart("OCRTimeHistogram", eChartType.ColumnClustered);
            ocrChart.Title.Text = "OCR Execution Time";
            ocrChart.SetPosition(1, 0, 4, 0);
            ocrChart.SetSize(800, 400);

            var xRangeOcr = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1]; 
            var yRangeOcr = ocrSheet.Cells[2, 2, ocrExecutionTimes.Count + 1, 2];

            var ocrSeries = ocrChart.Series.Add(yRangeOcr, xRangeOcr);
            ocrSeries.Header = "Execution Time (ms)";

            // Set OCR bar graph color to orange
            ocrSeries.Fill.Color = Color.Orange;

            ocrChart.YAxis.Title.Text = "Execution Time (ms)";
            ocrChart.XAxis.Title.Text = "Image Filter";
            ocrChart.YAxis.MinValue = 0;
            ocrChart.Legend.Position = eLegendPosition.Bottom;

            // **Sheet 3: Visualisation - Display Two Bar Graphs**
            var visualisationSheet = package.Workbook.Worksheets.Add("Visualisation");

            // **Chart 1: Preprocessing Execution Time (Bar Graph)**
            var preprocessingChartForVisualisation = visualisationSheet.Drawings.AddChart("PreprocessingTimeHistogramVisual", eChartType.ColumnClustered);
            preprocessingChartForVisualisation.Title.Text = "Preprocessing Execution Time";
            preprocessingChartForVisualisation.SetPosition(1, 0, 1, 0);
            preprocessingChartForVisualisation.SetSize(800, 400);

            var xRangePreprocessVisual = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1]; 
            var yRangePreprocessVisual = preprocessingSheet.Cells[2, 2, preprocessingTimes.Count + 1, 2];

            var preprocessSeriesForVisualisation = preprocessingChartForVisualisation.Series.Add(yRangePreprocessVisual, xRangePreprocessVisual);
            preprocessSeriesForVisualisation.Header = "Execution Time (ms)";

            preprocessingChartForVisualisation.YAxis.Title.Text = "Execution Time (ms)";
            preprocessingChartForVisualisation.XAxis.Title.Text = "Preprocessing Technique";
            preprocessingChartForVisualisation.YAxis.MinValue = 0;
            preprocessingChartForVisualisation.Legend.Position = eLegendPosition.Bottom;

            // **Chart 2: OCR Execution Time (Bar Graph)**
            var ocrChartForVisualisation = visualisationSheet.Drawings.AddChart("OCRExecutionTimeHistogramVisual", eChartType.ColumnClustered);
            ocrChartForVisualisation.Title.Text = "OCR Execution Time";
            ocrChartForVisualisation.SetPosition(22, 0, 1, 0);  // Placing it below the first chart
            ocrChartForVisualisation.SetSize(800, 400);

            var xRangeOcrVisual = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1]; 
            var yRangeOcrVisual = ocrSheet.Cells[2, 2, ocrExecutionTimes.Count + 1, 2];

            var ocrSeriesForVisualisation = ocrChartForVisualisation.Series.Add(yRangeOcrVisual, xRangeOcrVisual);
            ocrSeriesForVisualisation.Header = "Execution Time (ms)";

            // Set OCR bar graph color to orange
            ocrSeriesForVisualisation.Fill.Color = Color.Orange;

            ocrChartForVisualisation.YAxis.Title.Text = "Execution Time (ms)";
            ocrChartForVisualisation.XAxis.Title.Text = "Image Filter";
            ocrChartForVisualisation.YAxis.MinValue = 0;
            ocrChartForVisualisation.Legend.Position = eLegendPosition.Bottom;

            // Save the Excel file
            File.WriteAllBytes(filePath, package.GetAsByteArray());
        }
    }
}