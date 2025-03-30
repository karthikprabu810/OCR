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
    /// <param name="imagePreprocessing">Preprocessing method performance metrics</param>
    /// <param name="ocrExtraction">OCR tool performance metrics</param>
    public static void SaveExecutionTimesToExcel(string filePath, 
        List<(string ImageName, string Method, double TimeTaken, long MemoryUsage)> imagePreprocessing, 
        List<(string ImageName, string OCRTool, double TimeTaken, long MemoryUsage)> ocrExtraction)
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
        for (int i = 0; i < imagePreprocessing.Count; i++)
        {
            preprocessingSheet.Cells[i + 2, 1].Value = imagePreprocessing[i].Method;
            preprocessingSheet.Cells[i + 2, 2].Value = imagePreprocessing[i].TimeTaken;
            preprocessingSheet.Cells[i + 2, 3].Value = imagePreprocessing[i].MemoryUsage;
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
        var xRangeExecutionTime = preprocessingSheet.Cells[2, 1, imagePreprocessing.Count + 1, 1];
        var yRangeExecutionTime = preprocessingSheet.Cells[2, 2, imagePreprocessing.Count + 1, 2];

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
        var xRangeMemoryUsage = preprocessingSheet.Cells[2, 1, imagePreprocessing.Count + 1, 1];
        var yRangeMemoryUsage = preprocessingSheet.Cells[2, 3, imagePreprocessing.Count + 1, 3];

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
        for (int i = 0; i < ocrExtraction.Count; i++)
        {
            ocrSheet.Cells[i + 2, 1].Value = ocrExtraction[i].OCRTool;
            ocrSheet.Cells[i + 2, 2].Value = ocrExtraction[i].TimeTaken;
            ocrSheet.Cells[i + 2, 3].Value = ocrExtraction[i].MemoryUsage;
        }

        // Auto-resize columns to fit content
        ocrSheet.Cells.AutoFitColumns();

        // Create execution time chart for OCR tools
        var executionTimeChart2 = ocrSheet.Drawings.AddChart("ExecutionTimeChart2", eChartType.ColumnClustered);
        executionTimeChart2.Title.Text = "Execution Time (Preprocessing and OCR)";
        executionTimeChart2.SetPosition(1, 0, 7, 0);
        executionTimeChart2.SetSize(800, 400);

        // Set data ranges for the chart
        var xRangeExecutionTime2 = ocrSheet.Cells[2, 1, ocrExtraction.Count + 1, 1];
        var yRangeExecutionTime2 = ocrSheet.Cells[2, 2, ocrExtraction.Count + 1, 2];

        // Add data series to the chart
        var execTimeSeries2 = executionTimeChart2.Series.Add(yRangeExecutionTime2, xRangeExecutionTime2);
        execTimeSeries2.Header = "Execution Time (ms)";

        // Create a second chart for memory usage
        var memoryUsageChart2 = ocrSheet.Drawings.AddChart("MemoryUsageChart2", eChartType.ColumnClustered);
        memoryUsageChart2.Title.Text = "Memory Usage (Preprocessing and OCR)";
        memoryUsageChart2.SetPosition(22, 0, 7, 0);
        memoryUsageChart2.SetSize(800, 400);

        // Set data ranges for the memory usage chart
        var xRangeMemoryUsage2 = ocrSheet.Cells[2, 1, ocrExtraction.Count + 1, 1];
        var yRangeMemoryUsage2 = ocrSheet.Cells[2, 3, ocrExtraction.Count + 1, 3];

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
        
        // Store the original vectors in the worksheet for reference
        worksheet.Cells[1, 6].Value = "Original Vectors (for reference)";
        worksheet.Cells[1, 6].Style.Font.Bold = true;
        
        for (int i = 0; i < vectors.Count; i++)
        {
            worksheet.Cells[i + 2, 6].Value = embeddings[i].Label;
            
            // Store vector values with full decimal precision
            for (int j = 0; j < vectors[i].Length && j < 20; j++) // Limit to first 20 dimensions
            {
                worksheet.Cells[i + 2, 7 + j].Value = vectors[i][j];
                worksheet.Cells[i + 2, 7 + j].Style.Numberformat.Format = "0.0000"; // Force decimal display
            }
        }
        
        // Perform dimensionality reduction to get 2D coordinates
        var reduced = ReduceDimensionality(vectors);
        
        

        // Set up column headers for the reduced coordinates
        worksheet.Cells[1, 1].Value = "X";
        worksheet.Cells[1, 2].Value = "Y";
        worksheet.Cells[1, 3].Value = "Label";
        
        worksheet.Cells[1, 1, 1, 3].Style.Font.Bold = true;
            
        // Write the reduced 2D coordinates and labels to the worksheet
        for (int i = 0; i < reduced.Count; i++)
        {
            worksheet.Cells[i + 2, 1].Value = (reduced[i] != null && reduced[i].Length > 0) ? reduced[i][0] : 0; // X coordinate
            worksheet.Cells[i + 2, 2].Value = (reduced[i] != null && reduced[i].Length > 0) ? reduced[i][1] : 0; // Y coordinate
            worksheet.Cells[i + 2, 3].Value = embeddings[i].Label; // Text label
            
            // Set number format to display decimals
            worksheet.Cells[i + 2, 1].Style.Numberformat.Format = "0.0000";
            worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "0.0000";
        }
            
        // Create scatter plot chart for the embeddings
        var chart = worksheet.Drawings.AddChart("EmbeddingScatter", eChartType.XYScatter);
        chart.SetPosition(2, 0, 5, 0);
        chart.SetSize(800, 600);
        
        // Create individual series for each point to show the labels in the legend
        for (int i = 0; i < reduced.Count; i++)
        {
            // Create a separate range for each point
            var xCell = worksheet.Cells[i + 2, 1];
            var yCell = worksheet.Cells[i + 2, 2];
            
            // Add a series for this point
            var series = chart.Series.Add(yCell, xCell);
            series.Header = embeddings[i].Label; // Use the label as the series name for the legend
        }
            
        // Customize chart appearance and labels
        chart.Title.Text = "OCR Results Embedding Visualization";
        chart.XAxis.Title.Text = "Dimension 1 (X)";
        chart.YAxis.Title.Text = "Dimension 2 (Y)";
        
        // Enable major gridlines
        chart.XAxis.MajorGridlines.Width = 0.25f;
        chart.YAxis.MajorGridlines.Width = 0.25f;
            
        // Save the updated package to the file
        package.Save();
    }

    /// <summary>
    /// Reduces vector dimensionality for visualization purposes.
    /// Implements a simplified PCA-like projection to 2D coordinates.
    /// </summary>
    /// <param name="vectors">High-dimensional embedding vectors</param>
    /// <returns>2D coordinates for visualization</returns>
    private static List<double[]> ReduceDimensionality(List<double[]> vectors)
    {
        if (vectors.Count == 0)
            return new List<double[]>();
            
        // Handle case where vectors already have 2 or fewer dimensions
        if (vectors[0].Length <= 2)
        {
            return vectors.Select(v => 
            {
                if (v.Length == 1)
                    return new double[] { v[0], 0 }; // Add a zero Y coordinate
                return v;
            }).ToList();
        }
        
        try
        {
            // Check if all vectors have the same dimensionality
            int d = vectors[0].Length;
            if (vectors.Any(v => v.Length != d))
            {
                // Ensure all vectors have the same dimensionality by copying to new arrays with proper size
                int maxDim = vectors.Max(v => v.Length);
                var uniformVectors = new List<double[]>();
                
                foreach (var originalVector in vectors)
                {
                    var newVector = new double[maxDim]; // Create a new array of the right size
                    
                    // Copy the values from the original vector
                    for (int j = 0; j < originalVector.Length; j++)
                    {
                        newVector[j] = originalVector[j];
                    }
                    
                    // The rest of the elements remain as default 0
                    uniformVectors.Add(newVector);
                }
                
                // Replace the original vectors with the uniform ones
                vectors = uniformVectors;
                d = maxDim;
            }
            
            // Center the data by subtracting the mean
            var mean = new double[d];
            for (int i = 0; i < d; i++)
            {
                mean[i] = vectors.Average(v => v[i]);
            }
            
            var centered = vectors.Select(v => v.Zip(mean, (a, b) => a - b).ToArray()).ToList();
            
            // Use a simple projection to 2D
            // This is a simplified approach - for better results, 
            // a proper PCA implementation would be needed
            var result = new List<double[]>();
            foreach (var vector in centered)
            {
                // Project to 2D using mean of first half and second half
                // This is a simplification that works for visualization
                double x = 0, y = 0;
                
                if (vector.Length >= 4)
                {
                    // Use first and second quarter of dimensions for x, y
                    int quarter = vector.Length / 4;
                    for (int i = 0; i < quarter; i++)
                        x += vector[i];
                    for (int i = quarter; i < 2 * quarter; i++)
                        y += vector[i];
                    
                    // Normalize
                    x /= quarter;
                    y /= quarter;
                }
                else
                {
                    // For shorter vectors, use simpler approach
                    if (vector.Length > 0) x = vector[0];
                    if (vector.Length > 1) y = vector[1];
                }
                
                // Add small random jitter to prevent points from overlapping
                Random rand = new Random(vector.GetHashCode());
                x += (rand.NextDouble() - 0.5) * 0.1;
                y += (rand.NextDouble() - 0.5) * 0.1;
                
                result.Add(new double[] { Math.Round(x, 4), Math.Round(y, 4) });
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in dimensionality reduction: {ex.Message}");
            
            // Fallback to identity mapping if something goes wrong
            return vectors.Select(v => 
            {
                if (v.Length >= 2)
                    return new double[] { v[0], v[1] };
                else if (v.Length == 1)
                    return new double[] { v[0], 0 };
                else
                    return new double[] { 0, 0 };
            }).ToList();
        }
    }

    /// <summary>
    /// Saves clustering analysis results to an Excel file with visualizations.
    /// </summary>
    /// <param name="excelFilePath">Path to the Excel file.</param>
    /// <param name="clusterLabels">Array of cluster labels for each preprocessing method.</param>
    /// <param name="overallSilhouetteScore">Overall silhouette score indicating clustering quality.</param>
    /// <param name="individualSilhouetteScores">Individual silhouette scores for each preprocessing method.</param>
    /// <param name="worksheetName">Name of the worksheet to create.</param>
    /// <param name="bestPreprocessingMethod">The best preprocessing method determined by clustering.</param>
    /// <param name="preprocessingMethodNames">Names of the preprocessing methods.</param>
    public static void SaveClusteringResultsToExcel(
        string excelFilePath,
        int[]? clusterLabels,
        double overallSilhouetteScore,
        double[] individualSilhouetteScores,
        string worksheetName,
        string bestPreprocessingMethod,
        List<string>? preprocessingMethodNames)
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
        clusteringSheet.Cells[1, 3].Value = "Silhouette Score";
        clusteringSheet.Cells[1, 4].Value = "Is Best Method";

        // Bold the headers
        clusteringSheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;

        // Handle potential mismatch between clusterLabels and preprocessingMethodNames
        int dataLength = Math.Min(
            preprocessingMethodNames?.Count ?? 0, 
            clusterLabels?.Length ?? 0);
            
        // Ensure individualSilhouetteScores is valid and has enough elements
        bool hasIndividualScores = individualSilhouetteScores.Length >= dataLength;

        // Add data rows
        for (int i = 0; i < dataLength; i++)
        {
            int row = i + 2; // +2 because we start data from row 2 (after headers)

            // Preprocessing method name
            clusteringSheet.Cells[row, 1].Value = preprocessingMethodNames[i];

            // Cluster ID
            clusteringSheet.Cells[row, 2].Value = clusterLabels[i];
            
            // Individual silhouette score
            clusteringSheet.Cells[row, 3].Value = hasIndividualScores ? 
                Math.Round(individualSilhouetteScores[i], 4) : 
                "N/A";

            // Is this the best method
            bool isBest = preprocessingMethodNames[i] == bestPreprocessingMethod;
            clusteringSheet.Cells[row, 4].Value = isBest ? "Yes" : "No";

            // Highlight the best method
            if (isBest)
            {
                // Highlight the entire row
                clusteringSheet.Cells[row, 1, row, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                clusteringSheet.Cells[row, 1, row, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }
            
            // Style the silhouette score cell (conditional formatting)
            if (hasIndividualScores)
            {
                // Set color scale based on silhouette score value
                double score = individualSilhouetteScores[i];
                if (!double.IsNaN(score) && !double.IsInfinity(score))
                {
                    // Color coding: Red for negative (bad), Yellow for near zero, Green for positive (good)
                    if (score < 0)
                    {
                        clusteringSheet.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        clusteringSheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightPink);
                    }
                    else if (score > 0.7)
                    {
                        clusteringSheet.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        clusteringSheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                    }
                    else if (score > 0.3)
                    {
                        clusteringSheet.Cells[row, 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        clusteringSheet.Cells[row, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
                    }
                }
            }
        }

        // Add a section for clustering metrics
        clusteringSheet.Cells[dataLength + 3, 1].Value = "Clustering Metrics";
        clusteringSheet.Cells[dataLength + 3, 1].Style.Font.Bold = true;

        clusteringSheet.Cells[dataLength + 4, 1].Value = "Overall Silhouette Score";
        clusteringSheet.Cells[dataLength + 4, 2].Value = Math.Round(overallSilhouetteScore, 4);

        clusteringSheet.Cells[dataLength + 5, 1].Value = "Best Preprocessing Method";
        clusteringSheet.Cells[dataLength + 5, 2].Value = bestPreprocessingMethod;
        
        clusteringSheet.Cells[dataLength + 6, 1].Value = "Silhouette Score Interpretation";
        clusteringSheet.Cells[dataLength + 6, 1].Style.Font.Bold = true;
        
        clusteringSheet.Cells[dataLength + 7, 1].Value = "< 0: Likely incorrect clustering";
        clusteringSheet.Cells[dataLength + 8, 1].Value = "0-0.3: Weak structure";
        clusteringSheet.Cells[dataLength + 9, 1].Value = "0.3-0.7: Reasonable structure";
        clusteringSheet.Cells[dataLength + 10, 1].Value = "> 0.7: Strong structure";

        // Auto-size columns for better readability
        clusteringSheet.Cells[1, 1, dataLength + 10, 4].AutoFitColumns();

        // Create a pie chart to visualize cluster distribution only if we have sufficient data
        if (clusterLabels != null && clusterLabels.Length > 0)
        {
            var pieChart = clusteringSheet.Drawings.AddChart("Cluster Distribution", eChartType.Pie3D);

            // Set chart data source - count members in each cluster
            var uniqueClusters = clusterLabels.Distinct().ToList();
            var clusterCounts = uniqueClusters.Select(c => clusterLabels.Count(l => l == c)).ToList();

            // Create a temporary data section for the chart
            int charDataStartRow = dataLength + 12;
            clusteringSheet.Cells[charDataStartRow, 1].Value = "Cluster";
            clusteringSheet.Cells[charDataStartRow, 2].Value = "Count";

            for (int i = 0; i < uniqueClusters.Count; i++)
            {
                clusteringSheet.Cells[charDataStartRow + i + 1, 1].Value = $"Cluster {uniqueClusters[i]}";
                clusteringSheet.Cells[charDataStartRow + i + 1, 2].Value = clusterCounts[i];
            }

            // Configure the chart
            pieChart.SetPosition(dataLength + 7, 0, 6, 0);
            pieChart.SetSize(400, 300);
            pieChart.Series.Add(
                ExcelCellBase.GetAddress(charDataStartRow + 1, 2, charDataStartRow + uniqueClusters.Count, 2),
                ExcelCellBase.GetAddress(charDataStartRow + 1, 1, charDataStartRow + uniqueClusters.Count, 1)
            );
            pieChart.Title.Text = "Cluster Distribution";
        }
        
        // Create a bar chart for silhouette scores if we have individual scores
        if (individualSilhouetteScores.Length > 0)
        {
            var barChart = clusteringSheet.Drawings.AddChart("Silhouette Scores", eChartType.ColumnClustered);
            
            // Set chart position and size
            barChart.SetPosition(1, 0, 6, 0);
            barChart.SetSize(500, 300);
            
            // Configure the series data
            barChart.Series.Add(
                ExcelCellBase.GetAddress(2, 3, dataLength + 1, 3),
                ExcelCellBase.GetAddress(2, 1, dataLength + 1, 1)
            );
            
            // Set chart title and axis labels
            barChart.Title.Text = "Silhouette Scores by Preprocessing Method";
            barChart.XAxis.Title.Text = "Preprocessing Method";
            barChart.YAxis.Title.Text = "Silhouette Score";
            
            // Set Y-axis range from -1 to 1
            barChart.YAxis.MinValue = -1.0;
            barChart.YAxis.MaxValue = 1.0;
        }

        // Save the changes
        package.Save();
    }
}
