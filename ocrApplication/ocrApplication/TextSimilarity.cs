using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;


namespace ocrApplication
{
    /// <summary>
    /// Container for text similarity analysis functionality.
    /// Provides classes for comparing and visualizing OCR results.
    /// </summary>
    public abstract class TextSimilarity
    {
        /// <summary>
        /// Creates visual representations of similarity between OCR results.
        /// Generates Excel-based heatmaps and reports for comparing preprocessing methods.
        /// </summary>
        public class SimilarityMatrixGenerator
        {
            /// <summary>
            /// Text comparison utility for calculating similarity metrics.
            /// </summary>
            private readonly OcrComparison _ocrComparison = new();
            
            /// <summary>
            /// Converts OCR text results into vector embeddings for visualization.
            /// Maps text to word frequency vectors with consistent dimensions.
            /// </summary>
            /// <param name="texts">OCR text results to convert</param>
            /// <param name="labels">Method names for each text</param>
            /// <returns>Vector representations with labels</returns>
            public List<TextEmbedding> GenerateTextEmbeddings(List<string> texts, List<string> labels)
            {
                var embeddings = new List<TextEmbedding>();
                
                // Find the maximum dimension across all word vectors for normalization
                int maxDim = 0;
                var allWordVectors = new List<Dictionary<string, double>>();
                
                // First pass: compute all word vectors and determine max dimension
                for (int i = 0; i < texts.Count; i++)
                {
                    // Convert each text to a word frequency vector
                    var wordVector = _ocrComparison.GetWordVector(texts[i]);
                    allWordVectors.Add(wordVector);
                    maxDim = Math.Max(maxDim, wordVector.Count);
                }
                
                // Second pass: create embeddings with proper dimensionality
                for (int i = 0; i < texts.Count; i++)
                {
                    // Get the precomputed word vector
                    var wordVector = allWordVectors[i];
                    
                    // Convert dictionary values to array while preserving decimal precision
                    double[] vector = wordVector.Values.Select(v => (double)v).ToArray();
                    
                    // Ensure all vectors have the same dimension by padding with zeros if needed
                    if (vector.Length < maxDim)
                    {
                        Array.Resize(ref vector, maxDim);
                    }
                    
                    // Normalize vector values to prevent integer truncation in visualization
                    // This ensures decimal points are preserved when plotted
                    double maxValue = vector.Length > 0 ? vector.Max() : 1.0;
                    if (maxValue > 0)
                    {
                        for (int j = 0; j < vector.Length; j++)
                        {
                            vector[j] = Math.Round(vector[j] / maxValue, 4); // Preserve 4 decimal places
                        }
                    }
                    
                    // Create a TextEmbedding with the normalized vector and its label
                    embeddings.Add(new TextEmbedding(vector, labels[i]));
                }
                
                return embeddings;
            }

            /// <summary>
            /// Creates cosine similarity heatmap comparing OCR results.
            /// Visualizes word-level similarity between texts.
            /// </summary>
            /// <param name="ocrResults">OCR text results</param>
            /// <param name="groundTruth">Reference text for comparison</param>
            /// <param name="outputFilePath">Excel output path</param>
            /// <param name="ocrSteps">Method names for labeling</param>
            /// <returns>Task for async operation</returns>
            public async Task GenerateAndVisualizeOcrSimilarityMatrix(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> ocrSteps)
            {
                int n = ocrResults.Count;
                // Create matrix with space for ground truth plus all OCR results
                double[,] similarityMatrix = new double[n + 1, n + 1]; // +1 for ground truth
                
                // Create a single list with ground truth as the first element, followed by OCR results
                // This simplifies the matrix calculation
                var allTexts = new List<string> { groundTruth };
                allTexts.AddRange(ocrResults);
                
                // Calculate cosine similarity for each pair of texts in the matrix
                for (int i = 0; i < allTexts.Count; i++)
                {
                    for (int j = 0; j < allTexts.Count; j++)
                    {
                        // When comparing with ground truth (first row/column), use cosine similarity
                        // This measures how similar the word frequency distributions are
                        if (i == 0 || j == 0)
                        {
                            similarityMatrix[i, j] = Math.Round(_ocrComparison.CalculateCosineSimilarity(allTexts[i], allTexts[j]),2);
                        }
                        else
                        {
                            // For comparing OCR methods with each other
                            similarityMatrix[i, j] = Math.Round(_ocrComparison.CalculateCosineSimilarity(allTexts[i], allTexts[j]),2);
                        }
                    }
                }
                
                // Create headers with "Ground Truth" as the first element, followed by OCR method names
                var headers = new List<string> { "Ground Truth" };
                headers.AddRange(ocrSteps);
                
                // Save the matrix to Excel with heatmap formatting
                // Heatmap uses color intensity to show similarity strength
                SaveSimilarityMatrixWithHeatmap(similarityMatrix, outputFilePath, "OCR_Similarity_Heatmap_Cosine", headers);
                
                // Brief delay to ensure file saving completes
                await Task.Delay(1000);
            }
            
            
            /// <summary>
            /// Creates Levenshtein similarity heatmap comparing OCR results.
            /// Visualizes character-level edit distance between texts.
            /// </summary>
            /// <param name="ocrResults">OCR text results</param>
            /// <param name="groundTruth">Reference text for comparison</param>
            /// <param name="outputFilePath">Excel output path</param>
            /// <param name="ocrSteps">Method names for labeling</param>
            /// <returns>Task for async operation</returns>
            public async Task GenerateAndVisualizeOcrSimilarityMatrixLv(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> ocrSteps)
            {
                int n = ocrResults.Count;
                // Create matrix with space for ground truth plus all OCR results
                double[,] similarityMatrix = new double[n + 1, n + 1]; // +1 for ground truth
                
                // Create a single list with ground truth as the first element, followed by OCR results
                var allTexts = new List<string> { groundTruth };
                allTexts.AddRange(ocrResults);
                
                // Calculate Levenshtein similarity for each pair of texts in the matrix
                for (int i = 0; i < allTexts.Count; i++)
                {
                    for (int j = 0; j < allTexts.Count; j++)
                    {
                        // When comparing with ground truth (first row/column), use Levenshtein similarity
                        // This measures character-level edit distance, good for detecting spelling errors
                        if (i == 0 || j == 0)
                        {
                            similarityMatrix[i, j] = Math.Round(_ocrComparison.CalculateLevenshteinSimilarity(allTexts[i], allTexts[j]),2);
                        }
                        else
                        {
                            // For comparing OCR methods with each other
                            similarityMatrix[i, j] = Math.Round(_ocrComparison.CalculateLevenshteinSimilarity(allTexts[i], allTexts[j]),2);
                        }
                    }
                }
                
                // Create headers with "Ground Truth" as the first element, followed by OCR method names
                var headers = new List<string> { "Ground Truth" };
                headers.AddRange(ocrSteps);
                
                // Save the matrix to Excel with heatmap formatting
                SaveSimilarityMatrixWithHeatmap(similarityMatrix, outputFilePath, "OCR_Similarity_Heatmap_Levenshtein", headers);
                
                // Brief delay to ensure file saving completes
                await Task.Delay(1000);
            }

            /// <summary>
            /// Creates color-coded Excel heatmap for similarity visualization.
            /// Higher values appear green, lower values appear red.
            /// </summary>
            /// <param name="matrix">Similarity values matrix</param>
            /// <param name="filePath">Excel file location</param>
            /// <param name="sheetName">Worksheet name</param>
            /// <param name="headers">Row/column labels</param>
            private void SaveSimilarityMatrixWithHeatmap(double[,] matrix, string filePath, string sheetName, List<string> headers)
            {
                using var package = new ExcelPackage(new FileInfo(filePath));
                // Check if the sheet already exists, and delete it if it does
                var worksheet = package.Workbook.Worksheets[sheetName];
                    
                if (worksheet != null)
                {
                    package.Workbook.Worksheets.Delete(sheetName);
                }
                    
                // Create a new worksheet
                worksheet = package.Workbook.Worksheets.Add(sheetName);
                    
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                    
                // Add headers to the first row and column for labeling
                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 2].Value = headers[i]; // Column headers
                    worksheet.Cells[i + 2, 1].Value = headers[i]; // Row headers
                }
                    
                // Populate the matrix with similarity values and apply color formatting
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        var cell = worksheet.Cells[i + 2, j + 2];
                        cell.Value = matrix[i, j];
                            
                        // Apply conditional formatting (heatmap)
                        // Higher values (closer to 100) get more intense green
                        // Lower values get more intense red
                        double normalizedValue = matrix[i, j]/100; // Assuming values are 0-100
                            
                        if (normalizedValue >= 0.5)
                        {
                            // Green gradient for higher similarity (0.5-1.0)
                            // More similar = more intense green color
                            byte greenIntensity = (byte)(155 + (normalizedValue - 0.5) * 200);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255 - greenIntensity, 255, 255 - greenIntensity));
                        }
                        else
                        {
                            // Red gradient for lower similarity (0-0.5)
                            // Less similar = more intense red color
                            byte redIntensity = (byte)(155 + (0.5 - normalizedValue) * 200);
                            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255 - redIntensity, 255 - redIntensity));
                        }
                    }
                }
                    
                // Format the header row and column with bold text and gray background
                using (var range = worksheet.Cells[1, 1, 1, cols + 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }
                    
                using (var range = worksheet.Cells[1, 1, rows + 1, 1])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }
                    
                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                    
                // Add a summary section below the matrix
                int summaryRow = rows + 4;
                worksheet.Cells[summaryRow, 1].Value = "Similarity Analysis Summary";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Font.Size = 14;
                    
                // Find the preprocessing method with the highest similarity to ground truth
                // This helps identify the most effective OCR method
                int bestMethodIndex = 0;
                double bestSimilarity = 0;
                for (int i = 1; i < rows; i++)
                {
                    if (matrix[0, i] > bestSimilarity)
                    {
                        bestSimilarity = matrix[0, i];
                        bestMethodIndex = i - 1;
                    }
                }
                    
                // Add the best method information to the summary
                worksheet.Cells[summaryRow + 2, 1].Value = "Best Preprocessing Method:";
                worksheet.Cells[summaryRow + 2, 2].Value = headers[bestMethodIndex + 1];
                worksheet.Cells[summaryRow + 2, 1].Style.Font.Bold = true;
                    
                // Add similarity score information to the summary
                worksheet.Cells[summaryRow + 3, 1].Value = "Similarity to Ground Truth:";
                worksheet.Cells[summaryRow + 3, 2].Value = $"{bestSimilarity:F2}%";
                worksheet.Cells[summaryRow + 3, 1].Style.Font.Bold = true;
                    
                // Save the Excel file
                package.Save();

                // Console.WriteLine($"Similarity matrix heatmap has been saved to {sheetName} in the Excel file.");
            }

            /// <summary>
            /// Creates comparative report of preprocessing method effectiveness.
            /// Analyzes multiple metrics and generates charts for visual comparison.
            /// </summary>
            /// <param name="ocrResults">OCR text results</param>
            /// <param name="groundTruth">Reference text for comparison</param>
            /// <param name="outputFilePath">Excel output path</param>
            /// <param name="preprocessingMethods">Method names for labeling</param>
            /// <returns>Task for async operation</returns>
            public async Task GeneratePreprocessingEffectivenessReport(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> preprocessingMethods)
            {
                using (var package = new ExcelPackage(new FileInfo(outputFilePath)))
                {
                    // Create a new worksheet for the preprocessing effectiveness report
                    var worksheet = package.Workbook.Worksheets.Add("Preprocessing_Effectiveness");
                    
                    // Set up column headers for different metrics
                    worksheet.Cells[1, 1].Value = "Preprocessing Method";
                    worksheet.Cells[1, 2].Value = "Cosine Similarity (%)";
                    worksheet.Cells[1, 3].Value = "Levenshtein Similarity (%)";
                    worksheet.Cells[1, 4].Value = "Word Count";
                    worksheet.Cells[1, 5].Value = "Character Count";
                    worksheet.Cells[1, 6].Value = "Unique Words";
                    
                    // Style headers with bold text and gray background
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

                    // Track the best performing methods for highlighting
                    double maxCosine=0;
                    double maxLevenshtein=0;
                    int maxCosineIndex=0;
                    int maxLevenshteinIndex=0;
                    
                    // Calculate metrics for each preprocessing method
                    for (int i = 0; i < ocrResults.Count; i++)
                    {
                        string ocrResult = ocrResults[i];
                        string methodName = preprocessingMethods[i];
                        
                        // Calculate similarities with the provided ground truth
                        double cosineSimilarity = Math.Round(_ocrComparison.CalculateCosineSimilarity(ocrResult, groundTruth),2);
                        double levenshteinSimilarity = Math.Round(_ocrComparison.CalculateLevenshteinSimilarity(ocrResult, groundTruth),2);
                        
                        // Calculate text statistics
                        int wordCount = ocrResult.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                        int charCount = ocrResult.Length;
                        int uniqueWordCount = _ocrComparison.GetWordVector(ocrResult).Count;
                        
                        // Populate the row
                        int row = i + 2;
                        worksheet.Cells[row, 1].Value = methodName;
                        worksheet.Cells[row, 2].Value = cosineSimilarity;
                        worksheet.Cells[row, 3].Value = levenshteinSimilarity;
                        worksheet.Cells[row, 4].Value = wordCount;
                        worksheet.Cells[row, 5].Value = charCount;
                        worksheet.Cells[row, 6].Value = uniqueWordCount;
                        
                        // Apply conditional formatting based on similarity
                        ApplyConditionalFormatting(worksheet.Cells[row, 2], cosineSimilarity);
                        ApplyConditionalFormatting(worksheet.Cells[row, 3], levenshteinSimilarity);
                        
                        if (cosineSimilarity > maxCosine)
                        {
                            maxCosine=cosineSimilarity;
                            maxCosineIndex = i;
                        }
                        if (levenshteinSimilarity > maxLevenshtein)
                        {
                            maxLevenshtein=levenshteinSimilarity;
                            maxLevenshteinIndex = i;
                        }
                    }
                    
                    // Add ground truth statistics (using the actual ground truth)
                    int groundTruthRow = preprocessingMethods.Count + 2;
                    int groundTruthWordCount = groundTruth.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    int groundTruthCharCount = groundTruth.Length;
                    int groundTruthUniqueWordCount = _ocrComparison.GetWordVector(groundTruth).Count;
                    
                    worksheet.Cells[groundTruthRow, 1].Value = "Ground Truth";
                    worksheet.Cells[groundTruthRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[groundTruthRow, 4].Value = groundTruthWordCount;
                    worksheet.Cells[groundTruthRow, 5].Value = groundTruthCharCount;
                    worksheet.Cells[groundTruthRow, 6].Value = groundTruthUniqueWordCount;
                    
                    // Create a chart to visualize similarities
                    var chart = worksheet.Drawings.AddChart("SimilarityChart", eChartType.ColumnClustered);
                    chart.SetPosition(2, 0, 7, 0);
                    chart.SetSize(800, 400);
                    chart.Title.Text = "OCR Preprocessing Method Effectiveness";
                    
                    // Add data series for Cosine and Levenshtein similarities
                    var cosineSeries = chart.Series.Add(worksheet.Cells[2, 2, preprocessingMethods.Count + 1, 2], 
                                                       worksheet.Cells[2, 1, preprocessingMethods.Count + 1, 1]);
                    cosineSeries.Header = "Cosine Similarity (%)";
                    
                    var levenshteinSeries = chart.Series.Add(worksheet.Cells[2, 3, preprocessingMethods.Count + 1, 3], 
                                                            worksheet.Cells[2, 1, preprocessingMethods.Count + 1, 1]);
                    levenshteinSeries.Header = "Levenshtein Similarity (%)";
                    
                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();
                    
                    // Add a summary section
                    int rows = preprocessingMethods.Count + 3;
                    int summaryRow = rows + 2;
                    worksheet.Cells[summaryRow, 1].Value = "Similarity Analysis Summary";
                    worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[summaryRow, 1].Style.Font.Size = 14;
                    
                    worksheet.Cells[summaryRow + 2, 1].Value = "Best Preprocessing Method:";
                    worksheet.Cells[summaryRow + 2, 2].Value = preprocessingMethods[maxCosineIndex];
                    worksheet.Cells[summaryRow + 2, 1].Style.Font.Bold = true;
                    
                    worksheet.Cells[summaryRow + 2, 3].Value = preprocessingMethods[maxLevenshteinIndex];
                    
                    worksheet.Cells[summaryRow + 3, 1].Value = "Similarity to Ground Truth:";
                    worksheet.Cells[summaryRow + 3, 2].Value = $"{maxCosine}";
                    worksheet.Cells[summaryRow + 3, 3].Value = $"{maxLevenshtein}";
                    worksheet.Cells[summaryRow + 3, 1].Style.Font.Bold = true;
                    
                    // Save the Excel file
                    await package.SaveAsync();
                }
                
                // Console.WriteLine("Preprocessing effectiveness report has been generated.");
                await Task.Delay(1000);
            }

            // Apply Color formatting for the cells based on the similarity percentage
            private void ApplyConditionalFormatting(ExcelRange cell, double normalizedValue)
            {
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                /*
                if (value >= 91)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Green); // 91-100: Green
                }
                else if (value >= 81)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);  // 81-90: Yellow
                }
                else if (value >= 71)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.PaleGreen);  // 71-80: Orange
                }
                else if (value >= 61)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightYellow); // 61-70: Orange Red
                }
                else if (value >= 51)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Yellow);  // 51-60: Yellow
                }
                else if (value >= 41)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);  // 41-50: Orange
                }
                else if (value >= 31)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral); // 31-40: Orange Red
                }
                else if (value >= 21)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Coral); // 21-30: Orange Red
                }
                else if (value >= 11)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.OrangeRed); // 11-20: Orange Red
                }
                else
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Red); // 0-10: Red
                }*/
                
                
                
                
                
                if (normalizedValue >= 0.7)
                {
                    // Good similarity (green)
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 200, 255, 200));
                }
                else if (normalizedValue >= 0.5)
                {
                    // Medium similarity (yellow)
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255, 200));
                }
                else
                {
                    // Poor similarity (red)
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 200, 200));
                }
            }
        }
    }

    /// <summary>
    /// Vector representation of text for similarity analysis.
    /// Maps text to numerical format for comparison and visualization.
    /// </summary>
    public class TextEmbedding
    {
        /// <summary>
        /// Numerical vector representing word frequencies or features.
        /// </summary>
        public double[] Vector { get; set; }
        
        /// <summary>
        /// Descriptive name identifying the OCR method or preprocessing.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Creates a new text embedding with vector and label.
        /// </summary>
        /// <param name="vector">Word frequency or feature vector</param>
        /// <param name="label">Method name or identifier</param>
        public TextEmbedding(double[] vector, string label)
        {
            Vector = vector;
            Label = label;
        }
    }
}