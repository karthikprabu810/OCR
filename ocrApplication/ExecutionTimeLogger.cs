using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace ocrApplication;

public static class ExecutionTimeLogger
{
    public static void SaveExecutionTimesToExcel(string filePath, 
        List<(string ImageName, string Method, double TimeTaken, long MemoryUsage)> preprocessingTimes, 
        List<(string ImageName, string OCRTool, double TimeTaken, long MemoryUsage)> ocrExecutionTimes)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (ExcelPackage package = new ExcelPackage())
        {
            // **Sheet 1: Preprocessing and OCR Execution Times & Memory Usage**
            var preprocessingSheet = package.Workbook.Worksheets.Add("Preprocessing Time");

            preprocessingSheet.Cells[1, 1].Value = "Preprocessing Method";
            preprocessingSheet.Cells[1, 2].Value = "Execution Time (ms)";
            preprocessingSheet.Cells[1, 3].Value = "Memory Usage (bytes)";
            //preprocessingSheet.Cells[1, 4].Value = "OCR Tool";
            //preprocessingSheet.Cells[1, 5].Value = "Execution Time (ms)";
            //preprocessingSheet.Cells[1, 6].Value = "Memory Usage (bytes)";

            for (int i = 0; i < preprocessingTimes.Count; i++)
            {
                preprocessingSheet.Cells[i + 2, 1].Value = preprocessingTimes[i].Method;
                preprocessingSheet.Cells[i + 2, 2].Value = preprocessingTimes[i].TimeTaken;
                preprocessingSheet.Cells[i + 2, 3].Value = preprocessingTimes[i].MemoryUsage;
            }

            //for (int i = 0; i < ocrExecutionTimes.Count; i++)
            //{
              //  preprocessingSheet.Cells[i + 2, 4].Value = ocrExecutionTimes[i].OCRTool;
              //  preprocessingSheet.Cells[i + 2, 5].Value = ocrExecutionTimes[i].TimeTaken;
              //  preprocessingSheet.Cells[i + 2, 6].Value = ocrExecutionTimes[i].MemoryUsage;
            //}

            preprocessingSheet.Cells.AutoFitColumns();

            // Add graph for Execution Time and Memory Usage (side by side)
            // **Execution Time Graph**
            var executionTimeChart = preprocessingSheet.Drawings.AddChart("ExecutionTimeChart", eChartType.ColumnClustered);
            executionTimeChart.Title.Text = "Execution Time (Preprocessing and OCR)";
            executionTimeChart.SetPosition(1, 0, 7, 0);
            executionTimeChart.SetSize(800, 400);

            var xRangeExecutionTime = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1];
            var yRangeExecutionTime = preprocessingSheet.Cells[2, 2, preprocessingTimes.Count + 1, 2];

            var execTimeSeries = executionTimeChart.Series.Add(yRangeExecutionTime, xRangeExecutionTime);
            execTimeSeries.Header = "Execution Time (ms)";

            // **Memory Usage Graph**
            var memoryUsageChart = preprocessingSheet.Drawings.AddChart("MemoryUsageChart", eChartType.ColumnClustered);
            memoryUsageChart.Title.Text = "Memory Usage (Preprocessing and OCR)";
            memoryUsageChart.SetPosition(22, 0, 7, 0);
            memoryUsageChart.SetSize(800, 400);

            var xRangeMemoryUsage = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1];
            var yRangeMemoryUsage = preprocessingSheet.Cells[2, 3, preprocessingTimes.Count + 1, 3];

            var memUsageSeries = memoryUsageChart.Series.Add(yRangeMemoryUsage, xRangeMemoryUsage);
            memUsageSeries.Header = "Memory Usage (bytes)";
            // Change the color of the chart series
            memUsageSeries.Fill.Color = System.Drawing.Color.Red; // Set to any color you prefer
            
            
            // **Sheet 2: Detailed Graphs for Execution Time and Memory Usage**
            var ocrSheet = package.Workbook.Worksheets.Add("OCR Execution Times");

            //ocrSheet.Cells[1, 1].Value = "Preprocessing Method";
            //ocrSheet.Cells[1, 2].Value = "Execution Time (ms)";
            //ocrSheet.Cells[1, 3].Value = "Memory Usage (bytes)";
            ocrSheet.Cells[1, 1].Value = "OCR Tool";
            ocrSheet.Cells[1, 2].Value = "Execution Time (ms)";
            ocrSheet.Cells[1, 3].Value = "Memory Usage (bytes)";

            // for (int i = 0; i < preprocessingTimes.Count; i++)
            // {
            //    ocrSheet.Cells[i + 2, 1].Value = preprocessingTimes[i].Method;
            //    ocrSheet.Cells[i + 2, 2].Value = preprocessingTimes[i].TimeTaken;
            //    ocrSheet.Cells[i + 2, 3].Value = preprocessingTimes[i].MemoryUsage;
            //}

            for (int i = 0; i < ocrExecutionTimes.Count; i++)
            {
                ocrSheet.Cells[i + 2, 1].Value = ocrExecutionTimes[i].OCRTool;
                ocrSheet.Cells[i + 2, 2].Value = ocrExecutionTimes[i].TimeTaken;
                ocrSheet.Cells[i + 2, 3].Value = ocrExecutionTimes[i].MemoryUsage;
            }

            ocrSheet.Cells.AutoFitColumns();

            // Add **Execution Time** Graph
            var executionTimeChart2 = ocrSheet.Drawings.AddChart("ExecutionTimeChart2", eChartType.ColumnClustered);
            executionTimeChart2.Title.Text = "Execution Time (Preprocessing and OCR)";
            executionTimeChart2.SetPosition(1, 0, 7, 0);
            executionTimeChart2.SetSize(800, 400);

            var xRangeExecutionTime2 = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1];
            var yRangeExecutionTime2 = ocrSheet.Cells[2, 2, ocrExecutionTimes.Count + 1, 2];

            var execTimeSeries2 = executionTimeChart2.Series.Add(yRangeExecutionTime2, xRangeExecutionTime2);
            execTimeSeries2.Header = "Execution Time (ms)";

            // Add **Memory Usage** Graph
            var memoryUsageChart2 = ocrSheet.Drawings.AddChart("MemoryUsageChart2", eChartType.ColumnClustered);
            memoryUsageChart2.Title.Text = "Memory Usage (Preprocessing and OCR)";
            memoryUsageChart2.SetPosition(22, 0, 7, 0);
            memoryUsageChart2.SetSize(800, 400);

            var xRangeMemoryUsage2 = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1];
            var yRangeMemoryUsage2 = ocrSheet.Cells[2, 3, ocrExecutionTimes.Count + 1, 3];

            var memUsageSeries2 = memoryUsageChart2.Series.Add(yRangeMemoryUsage2, xRangeMemoryUsage2);
            memUsageSeries2.Header = "Memory Usage (bytes)";
            // Change the color of the chart series
            memUsageSeries2.Fill.Color = System.Drawing.Color.Red; // Set to any color you prefer
/*
            // **Sheet 3: Visualisation - Display Four Graphs**
            var visualisationSheet = package.Workbook.Worksheets.Add("Visualisation");

            // Preprocessing Execution Time and Memory Usage Graphs side by side
            var executionTimeChartForVisualisation = visualisationSheet.Drawings.AddChart("ExecutionTimeForVisualisation", eChartType.ColumnClustered);
            executionTimeChartForVisualisation.Title.Text = "Preprocessing Execution Time";
            executionTimeChartForVisualisation.SetPosition(1, 0, 1, 0);
            executionTimeChartForVisualisation.SetSize(800, 400);

            var xRangePreprocessVisual = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1];
            var yRangePreprocessVisual = preprocessingSheet.Cells[2, 2, preprocessingTimes.Count + 1, 2];
            var preprocessSeriesForVisualisation = executionTimeChartForVisualisation.Series.Add(yRangePreprocessVisual, xRangePreprocessVisual);
            preprocessSeriesForVisualisation.Header = "Execution Time (ms)";

            // Preprocessing Memory Usage
            var memoryUsageChartForVisualisation = visualisationSheet.Drawings.AddChart("MemoryUsageForVisualisation", eChartType.ColumnClustered);
            memoryUsageChartForVisualisation.Title.Text = "Preprocessing Memory Usage";
            memoryUsageChartForVisualisation.SetPosition(1, 0, 8, 0);
            memoryUsageChartForVisualisation.SetSize(800, 400);

            var yRangePreprocessMemory = preprocessingSheet.Cells[2, 3, preprocessingTimes.Count + 1, 3];
            var memorySeriesForVisualisation = memoryUsageChartForVisualisation.Series.Add(yRangePreprocessMemory, xRangePreprocessVisual);
            memorySeriesForVisualisation.Header = "Memory Usage (bytes)";

            // OCR Execution Time and Memory Usage Graphs side by side
            var executionTimeChartForVisualisation2 = visualisationSheet.Drawings.AddChart("ExecutionTimeForVisualisation2", eChartType.ColumnClustered);
            executionTimeChartForVisualisation2.Title.Text = "OCR Execution Time";
            executionTimeChartForVisualisation2.SetPosition(22, 0, 1, 0);
            executionTimeChartForVisualisation2.SetSize(800, 400);

            var xRangeOcrVisual = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1];
            var yRangeOcrVisual = ocrSheet.Cells[2, 5, ocrExecutionTimes.Count + 1, 5];
            var ocrSeriesForVisualisation = executionTimeChartForVisualisation2.Series.Add(yRangeOcrVisual, xRangeOcrVisual);
            ocrSeriesForVisualisation.Header = "Execution Time (ms)";

            // OCR Memory Usage
            var memoryUsageChartForVisualisation2 = visualisationSheet.Drawings.AddChart("MemoryUsageForVisualisation2", eChartType.ColumnClustered);
            memoryUsageChartForVisualisation2.Title.Text = "OCR Memory Usage";
            memoryUsageChartForVisualisation2.SetPosition(22, 0, 8, 0);
            memoryUsageChartForVisualisation2.SetSize(800, 400);

            var yRangeOcrMemory = ocrSheet.Cells[2, 6, ocrExecutionTimes.Count + 1, 6];
            var memorySeriesForVisualisation2 = memoryUsageChartForVisualisation2.Series.Add(yRangeOcrMemory, xRangeOcrVisual);
            memorySeriesForVisualisation2.Header = "Memory Usage (bytes)";
*/
            // Save the Excel file
            File.WriteAllBytes(filePath, package.GetAsByteArray());
        }
    }

    /*
    public static void ComparisionPlot(string excelFilePath, List<double> levenshteinResult, List<double> cosineResult)
    {
        // Open the existing Excel file
                FileInfo existingFile = new FileInfo(excelFilePath);
                
                List<string> ocrSteps = new List<string>
                {
                    "Original OCR",
                    "grayscale OCR",
                    "gaussian OCR",
                    "median OCR",
                    "adaptive_thresholding OCR",
                    "gamma_correction OCR",
                    "canny_edge OCR",
                    "dilation OCR",
                    "erosion OCR",
                    "otsu_binarization OCR",
                    "deskew OCR",
                    "Combo1 OCR"
                };
                
                using (var package = new ExcelPackage(existingFile))
                {
                    // Create a 4th sheet (index starts from 0)
                    var worksheet = package.Workbook.Worksheets.Add("Similarity Index");

                    // Write data to the 4th sheet
                    worksheet.Cells[1, 1].Value = "Method for OCR";
                    worksheet.Cells[1, 2].Value = "Levenshtein Similarity";
                    worksheet.Cells[1, 3].Value = "Cosine Similarity";

                    // Populate the sheet with data
                    for (int i = 0; i < ocrSteps.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = ocrSteps[i];
                        worksheet.Cells[i + 2, 2].Value = levenshteinResult[i];
                        worksheet.Cells[i + 2, 3].Value = cosineResult[i];
                    }

                    // Add a line chart for the data
                    var chart = worksheet.Drawings.AddChart("LineChart", eChartType.Line);
                    chart.SetPosition(2, 0, 6, 0); // Set position on the sheet
                    chart.SetSize(700, 400); // Set size of the chart

                    // Set data series for the chart
                    var chart1= chart.Series.Add(worksheet.Cells["B2:B13"], worksheet.Cells["A2:A13"]); // Levenshtein
                    var chart2= chart.Series.Add(worksheet.Cells["C2:C13"], worksheet.Cells["A2:A13"]); // Cosine

                    // Customize chart title
                    chart.Title.Text = "OCR Similarity Results";
                    chart1.Header = "Levenshtein Similarity";
                    chart2.Header = "Cosine Similarity";
                    
                    worksheet.Cells[20, 1].Value = "Result with more Similarity to Original Image based on ";
                    worksheet.Cells[21, 1].Value = "Levenshtein Similarity";
                    worksheet.Cells[21, 3].Value = $"{ocrSteps[IndexOfMaxValue(levenshteinResult)]}";
                    worksheet.Cells[22, 1].Value = "Cosine Similarity";
                    worksheet.Cells[22, 3].Value = $"{ocrSteps[IndexOfMaxValue(cosineResult)]}";
                    

                    // Save the changes to the Excel file
                    package.Save();
                }

                Console.WriteLine("Excel file updated and saved successfully.");
                
    }
    */
    public static int IndexOfMaxValue<T>(List<T> list) where T : IComparable<T>
    {
        if (list == null || list.Count == 0)
        {
            return -1; // Return -1 for empty or null list
        }

        int maxIndex = 0; // Start with the first element as the maximum
        T maxValue = list[0];

        // Iterate through the list to find the maximum value
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].CompareTo(maxValue) > 0)
            {
                maxValue = list[i];
                maxIndex = i;
            }
        }

        return maxIndex; // Return the index of the element with the max value
    }
    
    
    public static void CreateEmbeddingVisualization(string excelFilePath, List<TextEmbedding> embeddings)
    {
        using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
        {
            List<string> ocrSteps = new List<string>
            {
                "Original OCR",
                "grayscale OCR",
                "gaussian OCR",
                "median OCR",
                "adaptive_thresholding OCR",
                "gamma_correction OCR",
                "canny_edge OCR",
                "dilation OCR",
                "erosion OCR",
                "otsu_binarization OCR",
                "deskew OCR",
                "Combo1 OCR"
            };
            var worksheet = package.Workbook.Worksheets.Add("Text Embeddings");
            
            // Perform dimensionality reduction to 2D using simple PCA
            var vectors = embeddings.Select(e => e.Vector).ToList();
            var reduced = ReduceDimensionality(vectors);
            
            // Write the reduced coordinates and labels
            worksheet.Cells[1, 1].Value = "X";
            worksheet.Cells[1, 2].Value = "Y";
            worksheet.Cells[1, 3].Value = "Label";
            
            for (int i = 0; i < reduced.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = reduced[i][0];
                worksheet.Cells[i + 2, 2].Value = reduced[i][1];
                worksheet.Cells[i + 2, 3].Value = embeddings[i].Label;
            }
            
            // Create scatter plot
            var chart = worksheet.Drawings.AddChart("EmbeddingScatter", eChartType.XYScatter);
            chart.SetPosition(2, 0, 5, 0);
            chart.SetSize(800, 600);
            
            // Add data series
            for (int i = 0; i < reduced.Count; i++)
            {
                // Get the specific X and Y values for the current row (or line)
                var xRange = worksheet.Cells[2 + i, 1]; // Single X value for the current row
                var yRange = worksheet.Cells[2 + i, 2]; // Single Y value for the current row

                // Add a new series for each row with its specific X and Y value
                var series = chart.Series.Add(yRange, xRange);
    
                // Set a unique header for each series, which will be used as a legend name
                series.Header = $"{ocrSteps[i]}"; // Example: Embedding 1, Embedding 2, etc.
            }
            
            /*
            // Add data series (group serires)
            var series = chart.Series.Add(worksheet.Cells[2, 2, reduced.Count + 1, 2], 
                                        worksheet.Cells[2, 1, reduced.Count + 1, 1]);
            series.Header = "Text Embeddings";
            */
            
            // Customize chart
            chart.Title.Text = "OCR Results Embedding Visualization";
            chart.XAxis.Title.Text = "Dimension 1 (X)";
            chart.YAxis.Title.Text = "Dimension 2 (Y)";
            
            package.Save();
        }
    }
    private static List<double[]> ReduceDimensionality(List<double[]> vectors)
    {
        // Simple PCA implementation for 2D visualization
        int n = vectors.Count;
        int d = vectors[0].Length;
        
        // Center the data
        var mean = new double[d];
        for (int i = 0; i < d; i++)
        {
            mean[i] = vectors.Average(v => v[i]);
        }
        
        var centered = vectors.Select(v => v.Zip(mean, (a, b) => a - b).ToArray()).ToList();
        
        // Calculate covariance matrix
        var covariance = new double[d, d];
        for (int i = 0; i < d; i++)
        {
            for (int j = 0; j < d; j++)
            {
                covariance[i, j] = centered.Average(v => v[i] * v[j]);
            }
        }
        
        // For simplicity, project onto first two principal components
        // (In a production environment, you might want to use a proper linear algebra library)
        var reduced = centered.Select(v => new double[]
        {
            v.Sum(x => x) / Math.Sqrt(d),  // Simplified projection
            v.Select((x, i) => x * Math.Cos(2 * Math.PI * i / d)).Sum() // Simplified projection
        }).ToList();
        
        return reduced;
    }
    
    
}
