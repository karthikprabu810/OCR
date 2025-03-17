using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;

namespace ocrApplication;

/// <summary>
/// Tracks and visualizes OCR performance metrics.
/// Creates Excel reports with execution times and memory usage visualizations.
/// </summary>
public static class ExecutionTimeLogger
{
    /// <summary>
    /// Generates performance report with charts for preprocessing and OCR methods.
    /// Creates visual comparisons of execution times and memory usage.
    /// </summary>
    /// <param name="filePath">Excel output location</param>
    /// <param name="preprocessingTimes">Preprocessing method performance metrics</param>
    /// <param name="ocrExecutionTimes">OCR tool performance metrics</param>
    public static void SaveExecutionTimesToExcel(string filePath, 
        List<(string ImageName, string Method, double TimeTaken, long MemoryUsage)> preprocessingTimes, 
        List<(string ImageName, string OCRTool, double TimeTaken, long MemoryUsage)> ocrExecutionTimes)
    {
        // Set EPPlus license context to non-commercial to avoid licensing issues
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using ExcelPackage package = new ExcelPackage();
        // Create first worksheet for preprocessing performance data
        var preprocessingSheet = package.Workbook.Worksheets.Add("Preprocessing Time");

        // Set up column headers
        preprocessingSheet.Cells[1, 1].Value = "Preprocessing Method";
        preprocessingSheet.Cells[1, 2].Value = "Execution Time (ms)";
        preprocessingSheet.Cells[1, 3].Value = "Memory Usage (bytes)";
            
        // Populate data for preprocessing methods
        for (int i = 0; i < preprocessingTimes.Count; i++)
        {
            preprocessingSheet.Cells[i + 2, 1].Value = preprocessingTimes[i].Method;
            preprocessingSheet.Cells[i + 2, 2].Value = preprocessingTimes[i].TimeTaken;
            preprocessingSheet.Cells[i + 2, 3].Value = preprocessingTimes[i].MemoryUsage;
        }

        // Auto-resize columns to fit content
        preprocessingSheet.Cells.AutoFitColumns();

        // Create execution time chart for preprocessing methods
        var executionTimeChart = preprocessingSheet.Drawings.AddChart("ExecutionTimeChart", eChartType.ColumnClustered);
        executionTimeChart.Title.Text = "Execution Time (Preprocessing and OCR)";
        // Position chart below the data table (row 1, column 7)
        executionTimeChart.SetPosition(1, 0, 7, 0);
        executionTimeChart.SetSize(800, 400);

        // Set data ranges for the chart
        var xRangeExecutionTime = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1];
        var yRangeExecutionTime = preprocessingSheet.Cells[2, 2, preprocessingTimes.Count + 1, 2];

        // Add data series to the chart
        var execTimeSeries = executionTimeChart.Series.Add(yRangeExecutionTime, xRangeExecutionTime);
        execTimeSeries.Header = "Execution Time (ms)";

        // Create memory usage chart for preprocessing methods
        var memoryUsageChart = preprocessingSheet.Drawings.AddChart("MemoryUsageChart", eChartType.ColumnClustered);
        memoryUsageChart.Title.Text = "Memory Usage (Preprocessing and OCR)";
        // Position chart below the execution time chart
        memoryUsageChart.SetPosition(22, 0, 7, 0);
        memoryUsageChart.SetSize(800, 400);

        // Set data ranges for the memory usage chart
        var xRangeMemoryUsage = preprocessingSheet.Cells[2, 1, preprocessingTimes.Count + 1, 1];
        var yRangeMemoryUsage = preprocessingSheet.Cells[2, 3, preprocessingTimes.Count + 1, 3];

        // Add data series to the memory usage chart
        var memUsageSeries = memoryUsageChart.Series.Add(yRangeMemoryUsage, xRangeMemoryUsage);
        memUsageSeries.Header = "Memory Usage (bytes)";
        // Set distinct color for memory usage chart to differentiate from execution time
        memUsageSeries.Fill.Color = System.Drawing.Color.Red;
            
            
        // Create second worksheet for OCR tool performance data
        var ocrSheet = package.Workbook.Worksheets.Add("OCR Execution Times");

        // Set up column headers
        ocrSheet.Cells[1, 1].Value = "OCR Tool";
        ocrSheet.Cells[1, 2].Value = "Execution Time (ms)";
        ocrSheet.Cells[1, 3].Value = "Memory Usage (bytes)";
            
        // Populate data for OCR tools
        for (int i = 0; i < ocrExecutionTimes.Count; i++)
        {
            ocrSheet.Cells[i + 2, 1].Value = ocrExecutionTimes[i].OCRTool;
            ocrSheet.Cells[i + 2, 2].Value = ocrExecutionTimes[i].TimeTaken;
            ocrSheet.Cells[i + 2, 3].Value = ocrExecutionTimes[i].MemoryUsage;
        }

        // Auto-resize columns to fit content
        ocrSheet.Cells.AutoFitColumns();

        // Create execution time chart for OCR tools
        var executionTimeChart2 = ocrSheet.Drawings.AddChart("ExecutionTimeChart2", eChartType.ColumnClustered);
        executionTimeChart2.Title.Text = "Execution Time (Preprocessing and OCR)";
        executionTimeChart2.SetPosition(1, 0, 7, 0);
        executionTimeChart2.SetSize(800, 400);

        // Set data ranges for the chart
        var xRangeExecutionTime2 = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1];
        var yRangeExecutionTime2 = ocrSheet.Cells[2, 2, ocrExecutionTimes.Count + 1, 2];

        // Add data series to the chart
        var execTimeSeries2 = executionTimeChart2.Series.Add(yRangeExecutionTime2, xRangeExecutionTime2);
        execTimeSeries2.Header = "Execution Time (ms)";

        // Create memory usage chart for OCR tools
        var memoryUsageChart2 = ocrSheet.Drawings.AddChart("MemoryUsageChart2", eChartType.ColumnClustered);
        memoryUsageChart2.Title.Text = "Memory Usage (Preprocessing and OCR)";
        memoryUsageChart2.SetPosition(22, 0, 7, 0);
        memoryUsageChart2.SetSize(800, 400);

        // Set data ranges for the memory usage chart
        var xRangeMemoryUsage2 = ocrSheet.Cells[2, 1, ocrExecutionTimes.Count + 1, 1];
        var yRangeMemoryUsage2 = ocrSheet.Cells[2, 3, ocrExecutionTimes.Count + 1, 3];

        // Add data series to the memory usage chart
        var memUsageSeries2 = memoryUsageChart2.Series.Add(yRangeMemoryUsage2, xRangeMemoryUsage2);
        memUsageSeries2.Header = "Memory Usage (bytes)";
        // Set distinct color for memory usage chart
        memUsageSeries2.Fill.Color = System.Drawing.Color.Red;

        // Save the generated Excel package to the specified file path
        File.WriteAllBytes(filePath, package.GetAsByteArray());
    }

    
    
    /// <summary>
    /// Visualizes text embeddings in 2D space for semantic comparison.
    /// Creates scatter plot to show relationships between OCR results.
    /// </summary>
    /// <param name="excelFilePath">Excel output location</param>
    /// <param name="embeddings">Vector representations of OCR texts</param>
    /// <param name="ocrSteps">Method names for labeling points</param>
    public static void CreateEmbeddingVisualization(string excelFilePath, List<TextEmbedding> embeddings, List<string> ocrSteps)
    {
        using var package = new ExcelPackage(new FileInfo(excelFilePath));
        // Create worksheet for embedding visualization
        var worksheet = package.Workbook.Worksheets.Add("Text Embeddings");
            
        // Reduce the high-dimensional embeddings to 2D for visualization
        var vectors = embeddings.Select(e => e.Vector).ToList();
        var reduced = (vectors);
            
        // Set up column headers for the reduced coordinates
        worksheet.Cells[1, 1].Value = "X";
        worksheet.Cells[1, 2].Value = "Y";
        worksheet.Cells[1, 3].Value = "Label";
            
        // Write the reduced 2D coordinates and labels to the worksheet
        for (int i = 0; i < reduced.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = reduced[i][0]; // X coordinate
            worksheet.Cells[i + 2, 2].Value = reduced[i][1]; // Y coordinate
            worksheet.Cells[i + 2, 3].Value = embeddings[i].Label; // Text label
        }
            
        // Create scatter plot chart for the embeddings
        var chart = worksheet.Drawings.AddChart("EmbeddingScatter", eChartType.XYScatter);
        chart.SetPosition(2, 0, 5, 0);
        chart.SetSize(800, 600);
            
        // Add each embedding as a separate data point in the scatter plot
        for (int i = 0; i < reduced.Count; i++)
        {
            // Get the specific X and Y values for the current data point
            var xRange = worksheet.Cells[2 + i, 1]; // X coordinate for the current point
            var yRange = worksheet.Cells[2 + i, 2]; // Y coordinate for the current point

            // Add a new series for each point with its specific X and Y value
            var series = chart.Series.Add(yRange, xRange);
    
            // Set the OCR method name as the series label for the legend
            series.Header = $"{ocrSteps[i]}"; // E.g., "Tesseract", "Google Vision", etc.
        }
            
        /*
            // Alternative approach: group all embeddings into a single series
            // This is commented out because the individual series approach above provides better visualization
            var series = chart.Series.Add(worksheet.Cells[2, 2, reduced.Count + 1, 2], 
                                        worksheet.Cells[2, 1, reduced.Count + 1, 1]);
            series.Header = "Text Embeddings";
            */
            
        // Customize chart appearance and labels
        chart.Title.Text = "OCR Results Embedding Visualization";
        chart.XAxis.Title.Text = "Dimension 1 (X)";
        chart.YAxis.Title.Text = "Dimension 2 (Y)";
            
        // Save the updated package to the file
        package.Save();
    }

    /// <summary>
    /// Reduces vector dimensionality for visualization purposes.
    /// Implements simplified PCA-like projection to 2D coordinates.
    /// </summary>
    /// <param name="vectors">High-dimensional embedding vectors</param>
    /// <returns>2D coordinates for visualization</returns>
    private static List<double[]> ReduceDimensionality(List<double[]> vectors)
    {
        // Get the dimensionality of the input vectors
        int d = vectors[0].Length;
        
        // Center the data by subtracting the mean from each dimension
        var mean = new double[d];
        for (int i = 0; i < d; i++)
        {
            mean[i] = vectors.Average(v => v[i]);
        }
        
        // Create centered vectors by subtracting the mean
        var centered = vectors.Select(v => v.Zip(mean, (a, b) => a - b).ToArray()).ToList();
        
        // Calculate the covariance matrix
        // This matrix represents how dimensions co-vary with each other
        var covariance = new double[d, d];
        for (int i = 0; i < d; i++)
        {
            for (int j = 0; j < d; j++)
            {
                covariance[i, j] = centered.Average(v => v[i] * v[j]);
            }
        }
        
        // For simplicity, project the vectors onto two dimensions
        // This is a simplified approach - a full PCA would compute eigenvectors
        var reduced = centered.Select(v => new []
        {
            // Simple projection onto first "principal component" (approximated)
            v.Sum(x => x) / Math.Sqrt(d),
            
            // Simple projection onto second "principal component" (approximated)
            // Uses a cosine function to ensure orthogonality to the first component
            v.Select((x, i) => x * Math.Cos(2 * Math.PI * i / d)).Sum()
        }).ToList();
        
        return reduced;
    }

    /// <summary>
    /// Saves clustering analysis results to an Excel file.
    /// </summary>
    /// <param name="excelFilePath">Path to the Excel file.</param>
    /// <param name="clusterLabels">Array of cluster labels for each preprocessing method.</param>
    /// <param name="silhouetteScore">Silhouette score indicating clustering quality.</param>
    /// <param name="worksheetName">Name of the worksheet to create.</param>
    /// <param name="bestPreprocessingMethod">The best preprocessing method determined by clustering.</param>
    /// <param name="preprocessingMethodNames">Names of the preprocessing methods.</param>
    public static void SaveClusteringResultsToExcel(
        string excelFilePath,
        int[] clusterLabels,
        double silhouetteScore,
        string worksheetName,
        string bestPreprocessingMethod,
        List<string> preprocessingMethodNames)
    {
        // Set EPPlus license context to non-commercial to avoid licensing issues
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        // Create or open the Excel package
        FileInfo fileInfo = new FileInfo(excelFilePath);
        using ExcelPackage package = new ExcelPackage(fileInfo);

        // Remove existing worksheet if it exists
        var existingSheet = package.Workbook.Worksheets.FirstOrDefault(s => s.Name == worksheetName);
        if (existingSheet != null)
        {
            package.Workbook.Worksheets.Delete(existingSheet);
        }

        // Create a new worksheet for clustering results
        var clusteringSheet = package.Workbook.Worksheets.Add(worksheetName);

        // Set headers
        clusteringSheet.Cells[1, 1].Value = "Preprocessing Method";
        clusteringSheet.Cells[1, 2].Value = "Cluster ID";
        clusteringSheet.Cells[1, 3].Value = "Is Best Method";

        // Bold the headers
        clusteringSheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;

        // Handle potential mismatch between clusterLabels and preprocessingMethodNames
        int dataLength = Math.Min(
            preprocessingMethodNames?.Count ?? 0, 
            clusterLabels?.Length ?? 0);

        // Add data rows
        for (int i = 0; i < dataLength; i++)
        {
            int row = i + 2; // +2 because we start data from row 2 (after headers)

            // Preprocessing method name
            clusteringSheet.Cells[row, 1].Value = preprocessingMethodNames[i];

            // Cluster ID
            clusteringSheet.Cells[row, 2].Value = clusterLabels[i];

            // Is this the best method
            bool isBest = preprocessingMethodNames[i] == bestPreprocessingMethod;
            clusteringSheet.Cells[row, 3].Value = isBest ? "Yes" : "No";

            // Highlight the best method
            if (isBest)
            {
                // Highlight the entire row
                clusteringSheet.Cells[row, 1, row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                clusteringSheet.Cells[row, 1, row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }
        }

        // Add a section for clustering metrics
        clusteringSheet.Cells[dataLength + 3, 1].Value = "Clustering Metrics";
        clusteringSheet.Cells[dataLength + 3, 1].Style.Font.Bold = true;

        clusteringSheet.Cells[dataLength + 4, 1].Value = "Silhouette Score";
        clusteringSheet.Cells[dataLength + 4, 2].Value = silhouetteScore;

        clusteringSheet.Cells[dataLength + 5, 1].Value = "Best Preprocessing Method";
        clusteringSheet.Cells[dataLength + 5, 2].Value = bestPreprocessingMethod;

        // Auto-size columns for better readability
        clusteringSheet.Cells[1, 1, dataLength + 5, 3].AutoFitColumns();

        // Create a pie chart to visualize cluster distribution only if we have sufficient data
        if (clusterLabels != null && clusterLabels.Length > 0)
        {
            var pieChart = clusteringSheet.Drawings.AddChart("Cluster Distribution", eChartType.Pie3D);

            // Set chart data source - count members in each cluster
            var uniqueClusters = clusterLabels.Distinct().ToList();
            var clusterCounts = uniqueClusters.Select(c => clusterLabels.Count(l => l == c)).ToList();

            // Create a temporary data section for the chart
            int charDataStartRow = dataLength + 7;
            clusteringSheet.Cells[charDataStartRow, 1].Value = "Cluster";
            clusteringSheet.Cells[charDataStartRow, 2].Value = "Count";

            for (int i = 0; i < uniqueClusters.Count; i++)
            {
                clusteringSheet.Cells[charDataStartRow + i + 1, 1].Value = $"Cluster {uniqueClusters[i]}";
                clusteringSheet.Cells[charDataStartRow + i + 1, 2].Value = clusterCounts[i];
            }

            // Configure the chart
            pieChart.SetPosition(dataLength + 3, 0, 3, 0);
            pieChart.SetSize(400, 300);
            pieChart.Series.Add(
                ExcelCellBase.GetAddress(charDataStartRow + 1, 2, charDataStartRow + uniqueClusters.Count, 2),
                ExcelCellBase.GetAddress(charDataStartRow + 1, 1, charDataStartRow + uniqueClusters.Count, 1)
            );
            pieChart.Title.Text = "Cluster Distribution";
        }

        // Save the changes
        package.Save();
    }
}
