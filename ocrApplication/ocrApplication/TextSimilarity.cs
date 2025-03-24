using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;


namespace ocrApplication
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
            foreach (var t in texts)
            {
                // Convert each text to a word frequency vector
                var wordVector = _ocrComparison.GetWordVector(t);
                allWordVectors.Add(wordVector);
                maxDim = Math.Max(maxDim, wordVector.Count);
            }
           
            // Second pass: create embeddings with proper dimensionality
            for (int i = 0; i < texts.Count; i++)
            {
                // Get the precomputed word vector
                var wordVector = allWordVectors[i];
                
                // Convert dictionary values to array while preserving decimal precision
                double[] vector = wordVector.Values.Select(v => v).ToArray();
                
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
        /// Creates Similarity heatmap comparing OCR results.
        /// Visualizes word set overlap between texts.
        /// </summary>
        /// <param name="ocrResults">OCR text results</param>
        /// <param name="groundTruth">Reference text for comparison</param>
        /// <param name="outputFilePath">Excel output path</param>
        /// <param name="ocrSteps">Method names for labeling</param>
        /// /// <param name="similarityType">Comparision Metric Used</param>
        /// <returns>Task for async operation</returns>
        public async Task GenerateAndVisualizeOcrSimilarityMatrix(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> ocrSteps, SimilarityType similarityType)
        {
            int n = ocrResults.Count;
            // Create matrix with space for ground truth plus all OCR results
            double[,] similarityMatrix = new double[n + 1, n + 1]; // +1 for ground truth
            
            // Create a single list with ground truth as the first element, followed by OCR results
            var allTexts = new List<string> { groundTruth };
            allTexts.AddRange(ocrResults);
            
            // Calculate similarity for each pair of texts in the matrix
            for (int i = 0; i < allTexts.Count; i++)
            {
                for (int j = 0; j < allTexts.Count; j++)
                {
                    double similarity;

                    switch (similarityType)
                    {
                        // Creates cosine similarity heatmap comparing OCR results.
                        case SimilarityType.Cosine:
                            similarity = Math.Round(_ocrComparison.CalculateCosineSimilarity(allTexts[i], allTexts[j]), 2);
                            break;

                        // Creates Levenshtein similarity heatmap comparing OCR results.
                        case SimilarityType.Levenshtein:
                            similarity = Math.Round(_ocrComparison.CalculateLevenshteinSimilarity(allTexts[i], allTexts[j]), 2);
                            break;

                        // Creates Jaro-Winkler similarity heatmap comparing OCR results.
                        case SimilarityType.JaroWinkler:
                            similarity = Math.Round(_ocrComparison.CalculateJaroWinklerSimilarity(allTexts[i], allTexts[j]), 2);
                            break;

                        // Creates Jaccard similarity heatmap comparing OCR results.
                        case SimilarityType.Jaccard:
                            similarity = Math.Round(_ocrComparison.CalculateJaccardSimilarity(allTexts[i], allTexts[j]), 2);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    similarityMatrix[i, j] = similarity;
                }
            }
            
            // Create headers with "Ground Truth" as the first element, followed by OCR method names
            var headers = new List<string> { "Ground Truth" };
            headers.AddRange(ocrSteps);
            
            // Save the matrix to Excel with heatmap formatting
            SaveSimilarityMatrixWithHeatmap(similarityMatrix, outputFilePath, $"Similarity_Heatmap_{similarityType}", headers);
            
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
                    if (i != j)
                    {
                        ApplyConditionalFormatting(worksheet.Cells[i + 2, j + 2], normalizedValue);
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
                worksheet.Cells[1, 4].Value = "Jaccard Similarity (%)";
                worksheet.Cells[1, 5].Value = "JaroWinkler Similarity (%)";
                worksheet.Cells[1, 6].Value = "Word Count";
                worksheet.Cells[1, 7].Value = "Character Count";
                worksheet.Cells[1, 8].Value = "Unique Words";
                
                // Style headers with bold text and gray background
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                }

                // Track the best performing methods for highlighting
                double maxCosine=0;
                double maxLevenshtein=0;
                double maxJaccard=0;
                double maxJaroWinkler=0;
                int maxCosineIndex=0;
                int maxLevenshteinIndex=0;
                int maxJaccardIndex=0;
                int maxJaroWinklerIndex=0;
                
                // Calculate metrics for each preprocessing method
                for (int i = 0; i < ocrResults.Count; i++)
                {
                    string ocrResult = ocrResults[i];
                    string methodName = preprocessingMethods[i];
                    
                    // Calculate similarities with the provided ground truth
                    double cosineSimilarity = Math.Round(_ocrComparison.CalculateCosineSimilarity(ocrResult, groundTruth),2);
                    double levenshteinSimilarity = Math.Round(_ocrComparison.CalculateLevenshteinSimilarity(ocrResult, groundTruth),2);
                    double jaccardSimilarity = Math.Round(_ocrComparison.CalculateJaccardSimilarity(ocrResult, groundTruth),2);
                    double jaroWinklerSimilarity = Math.Round(_ocrComparison.CalculateJaroWinklerSimilarity(ocrResult, groundTruth),2);
                    
                    // Calculate text statistics
                    int wordCount = ocrResult.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
                    int charCount = ocrResult.Length;
                    int uniqueWordCount = _ocrComparison.GetWordVector(ocrResult).Count;
                    
                    // Populate the row
                    int row = i + 2;
                    worksheet.Cells[row, 1].Value = methodName;
                    worksheet.Cells[row, 2].Value = cosineSimilarity;
                    worksheet.Cells[row, 3].Value = levenshteinSimilarity;
                    worksheet.Cells[row, 4].Value = jaccardSimilarity;
                    worksheet.Cells[row, 5].Value = jaroWinklerSimilarity;
                    worksheet.Cells[row, 6].Value = wordCount;
                    worksheet.Cells[row, 7].Value = charCount;
                    worksheet.Cells[row, 8].Value = uniqueWordCount;
                    
                    // Apply conditional formatting based on similarity
                    ApplyConditionalFormatting(worksheet.Cells[row, 2], cosineSimilarity);
                    ApplyConditionalFormatting(worksheet.Cells[row, 3], levenshteinSimilarity);
                    ApplyConditionalFormatting(worksheet.Cells[row, 4], jaccardSimilarity);
                    ApplyConditionalFormatting(worksheet.Cells[row, 5], jaroWinklerSimilarity);
                    
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
                    if (jaccardSimilarity > maxJaccard)
                    {
                        maxJaccard=jaccardSimilarity;
                        maxJaccardIndex = i;
                    }
                    if (jaroWinklerSimilarity > maxJaroWinkler)
                    {
                        maxJaroWinkler=jaroWinklerSimilarity;
                        maxJaroWinklerIndex = i;
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
                chart.SetPosition(2, 0, 10, 0);
                chart.SetSize(800, 400);
                chart.Title.Text = "OCR Preprocessing Method Effectiveness";
                
                // Add data series for Cosine and Levenshtein similarities
                var cosineSeries = chart.Series.Add(worksheet.Cells[2, 2, preprocessingMethods.Count + 1, 2], 
                                                   worksheet.Cells[2, 1, preprocessingMethods.Count + 1, 1]);
                cosineSeries.Header = "Cosine Similarity (%)";
                
                var levenshteinSeries = chart.Series.Add(worksheet.Cells[2, 3, preprocessingMethods.Count + 1, 3], 
                                                        worksheet.Cells[2, 1, preprocessingMethods.Count + 1, 1]);
                levenshteinSeries.Header = "Levenshtein Similarity (%)";
                
                var jaccardSeries = chart.Series.Add(worksheet.Cells[2, 4, preprocessingMethods.Count + 1, 4], 
                    worksheet.Cells[2, 1, preprocessingMethods.Count + 1, 1]);
                jaccardSeries.Header = "Jaccard Similarity (%)";
                
                var jaroWinklerSeries = chart.Series.Add(worksheet.Cells[2, 5, preprocessingMethods.Count + 1, 5], 
                    worksheet.Cells[2, 1, preprocessingMethods.Count + 1, 1]);
                jaroWinklerSeries.Header = "Jaro-Winkler Similarity (%)";
                
                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();
                
                // Add a summary section
                int rows = preprocessingMethods.Count + 3;
                int summaryRow = rows + 2;
                worksheet.Cells[summaryRow, 1].Value = "Similarity Analysis Summary";
                worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                worksheet.Cells[summaryRow, 1].Style.Font.Size = 14;
                
                worksheet.Cells[summaryRow + 2, 1].Value = "Best Preprocessing Method:";
                worksheet.Cells[summaryRow + 2, 1].Style.Font.Bold = true;
                
                worksheet.Cells[summaryRow + 2, 2].Value = preprocessingMethods[maxCosineIndex];
                worksheet.Cells[summaryRow + 2, 3].Value = preprocessingMethods[maxLevenshteinIndex];
                worksheet.Cells[summaryRow + 2, 4].Value = preprocessingMethods[maxJaccardIndex];
                worksheet.Cells[summaryRow + 2, 5].Value = preprocessingMethods[maxJaroWinklerIndex];
                
                worksheet.Cells[summaryRow + 3, 1].Value = "Similarity to Ground Truth:";
                worksheet.Cells[summaryRow + 3, 1].Style.Font.Bold = true;
                
                worksheet.Cells[summaryRow + 3, 2].Value = $"{maxCosine}";
                worksheet.Cells[summaryRow + 3, 3].Value = $"{maxLevenshtein}";
                worksheet.Cells[summaryRow + 3, 4].Value = $"{maxJaccard}";
                worksheet.Cells[summaryRow + 3, 5].Value = $"{maxJaroWinkler}";
                
                
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
            if (normalizedValue >= 0.8)
            {
                // Good similarity (green)
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 153, 255, 175));
            }
            else if (normalizedValue >= 0.6)
            {
                // Medium similarity (yellow)
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 200, 255, 200));
            }
            else if (normalizedValue >= 0.4)
            {
                // Medium similarity (yellow)
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255, 200));
            }
            else if (normalizedValue >= 0.2)
            {
                // Medium similarity (yellow)
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 230, 211, 172));
            }
            else
            {
                // Poor similarity (red)
                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 200, 200));
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