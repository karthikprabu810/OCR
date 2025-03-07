using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;


namespace ocrApplication
{
    public class TextSimilarity
    {
        public class SimilarityMatrixGenerator
        {
            // Instance of OcrComparison class to access non-static methods
            private readonly OcrComparison _ocrComparison;

            // Constructor to initialize OcrComparison instance
            public SimilarityMatrixGenerator()
            {
                _ocrComparison = new OcrComparison();
            }

            public List<TextEmbedding> GenerateTextEmbeddings(List<string> texts, List<string> labels)
            {
                var embeddings = new List<TextEmbedding>();
                
                for (int i = 0; i < texts.Count; i++)
                {
                    var wordVector = _ocrComparison.GetWordVector(texts[i]);
                    
                    // Convert dictionary to array for visualization
                    var vector = wordVector.Values.ToArray();
                    
                    // If vectors have different dimensions, pad with zeros
                    int maxDim = texts.Select(t => _ocrComparison.GetWordVector(t).Count).Max();
                    if (vector.Length < maxDim)
                    {
                        Array.Resize(ref vector, maxDim);
                    }
                    
                    embeddings.Add(new TextEmbedding(vector, labels[i]));
                }
                
                return embeddings;
            }

            public async Task GenerateAndVisualizeOcrSimilarityMatrix(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> ocrSteps)
            {
                int n = ocrResults.Count;
                double[,] similarityMatrix = new double[n + 1, n + 1]; // +1 for ground truth
                
                // Create a list with ground truth as the first element, followed by OCR results
                var allTexts = new List<string> { groundTruth };
                allTexts.AddRange(ocrResults);
                
                // Calculate cosine similarity for each pair of texts
                for (int i = 0; i < allTexts.Count; i++)
                {
                    for (int j = 0; j < allTexts.Count; j++)
                    {
                        // When comparing with ground truth (first row/column), use actual similarity
                        if (i == 0 || j == 0)
                        {
                            similarityMatrix[i, j] = _ocrComparison.CalculateCosineSimilarity(allTexts[i], allTexts[j]);
                        }
                        else
                        {
                            // For other comparisons, calculate normally
                            similarityMatrix[i, j] = _ocrComparison.CalculateCosineSimilarity(allTexts[i], allTexts[j]);
                        }
                    }
                }
                
                // Create headers with "Ground Truth" as the first element
                var headers = new List<string> { "Ground Truth" };
                headers.AddRange(ocrSteps);
                
                // Save the matrix to Excel with heatmap formatting
                SaveSimilarityMatrixWithHeatmap(similarityMatrix, outputFilePath, "OCR_Similarity_Heatmap_Cosine", headers);
                
                await Task.Delay(1000);
            }
            
            
            public async Task GenerateAndVisualizeOcrSimilarityMatrixLv(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> ocrSteps)
            {
                int n = ocrResults.Count;
                double[,] similarityMatrix = new double[n + 1, n + 1]; // +1 for ground truth
                
                // Create a list with ground truth as the first element, followed by OCR results
                var allTexts = new List<string> { groundTruth };
                allTexts.AddRange(ocrResults);
                
                // Calculate cosine similarity for each pair of texts
                for (int i = 0; i < allTexts.Count; i++)
                {
                    for (int j = 0; j < allTexts.Count; j++)
                    {
                        // When comparing with ground truth (first row/column), use actual similarity
                        if (i == 0 || j == 0)
                        {
                            similarityMatrix[i, j] = _ocrComparison.CalculateLevenshteinSimilarity(allTexts[i], allTexts[j]);
                        }
                        else
                        {
                            // For other comparisons, calculate normally
                            similarityMatrix[i, j] = _ocrComparison.CalculateLevenshteinSimilarity(allTexts[i], allTexts[j]);
                        }
                    }
                }
                
                // Create headers with "Ground Truth" as the first element
                var headers = new List<string> { "Ground Truth" };
                headers.AddRange(ocrSteps);
                
                // Save the matrix to Excel with heatmap formatting
                SaveSimilarityMatrixWithHeatmap(similarityMatrix, outputFilePath, "OCR_Similarity_Heatmap_Levenshtein", headers);
                
                await Task.Delay(1000);
            }

            private void SaveSimilarityMatrixWithHeatmap(double[,] matrix, string filePath, string sheetName, List<string> headers)
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[sheetName];
                    
                    if (worksheet != null)
                    {
                        package.Workbook.Worksheets.Delete(sheetName);
                    }
                    
                    worksheet = package.Workbook.Worksheets.Add(sheetName);
                    
                    int rows = matrix.GetLength(0);
                    int cols = matrix.GetLength(1);
                    
                    // Add headers to the first row and column
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cells[1, i + 2].Value = headers[i]; // Column headers
                        worksheet.Cells[i + 2, 1].Value = headers[i]; // Row headers
                    }
                    
                    // Populate the matrix
                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            var cell = worksheet.Cells[i + 2, j + 2];
                            cell.Value = matrix[i, j];
                            
                            // Apply conditional formatting (heatmap)
                            // Higher values (closer to 100) get more intense green
                            // Lower values get more intense red
                            double normalizedValue = matrix[i, j] / 100.0; // Assuming values are 0-100
                            
                            if (normalizedValue >= 0.5)
                            {
                                // Green gradient for higher similarity (0.5-1.0)
                                byte greenIntensity = (byte)(155 + (normalizedValue - 0.5) * 200);
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255 - greenIntensity, 255, 255 - greenIntensity));
                            }
                            else
                            {
                                // Red gradient for lower similarity (0-0.5)
                                byte redIntensity = (byte)(155 + (0.5 - normalizedValue) * 200);
                                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255, 255, 255 - redIntensity, 255 - redIntensity));
                            }
                        }
                    }
                    
                    // Format the header row and column
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
                    
                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();
                    
                    // Add a summary section
                    int summaryRow = rows + 4;
                    worksheet.Cells[summaryRow, 1].Value = "Similarity Analysis Summary";
                    worksheet.Cells[summaryRow, 1].Style.Font.Bold = true;
                    worksheet.Cells[summaryRow, 1].Style.Font.Size = 14;
                    
                    // Find the preprocessing method with highest similarity to ground truth
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
                    
                    worksheet.Cells[summaryRow + 2, 1].Value = "Best Preprocessing Method:";
                    worksheet.Cells[summaryRow + 2, 2].Value = headers[bestMethodIndex + 1];
                    worksheet.Cells[summaryRow + 2, 1].Style.Font.Bold = true;
                    
                    worksheet.Cells[summaryRow + 3, 1].Value = "Similarity to Ground Truth:";
                    worksheet.Cells[summaryRow + 3, 2].Value = $"{bestSimilarity:F2}%";
                    worksheet.Cells[summaryRow + 3, 1].Style.Font.Bold = true;
                    
                    // Save the Excel file
                    package.Save();
                }
                
                Console.WriteLine($"Similarity matrix heatmap has been saved to {sheetName} in the Excel file.");
            }

            public async Task GeneratePreprocessingEffectivenessReport(List<string> ocrResults, string groundTruth, string outputFilePath, List<string> preprocessingMethods)
            {
                using (var package = new ExcelPackage(new FileInfo(outputFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Preprocessing_Effectiveness");
                    
                    // Set up headers
                    worksheet.Cells[1, 1].Value = "Preprocessing Method";
                    worksheet.Cells[1, 2].Value = "Cosine Similarity (%)";
                    worksheet.Cells[1, 3].Value = "Levenshtein Similarity (%)";
                    worksheet.Cells[1, 4].Value = "Word Count";
                    worksheet.Cells[1, 5].Value = "Character Count";
                    worksheet.Cells[1, 6].Value = "Unique Words";
                    
                    // Style headers
                    using (var range = worksheet.Cells[1, 1, 1, 6])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    }

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
                        double cosineSimilarity = _ocrComparison.CalculateCosineSimilarity(ocrResult, groundTruth);
                        double levenshteinSimilarity = _ocrComparison.CalculateLevenshteinSimilarity(ocrResult, groundTruth);
                        
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
                        ApplyConditionalFormatting(worksheet.Cells[row, 2], cosineSimilarity );
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
                    package.Save();
                }
                
                Console.WriteLine("Preprocessing effectiveness report has been generated.");
                await Task.Delay(1000);
            }

            private void ApplyConditionalFormatting(ExcelRange cell, double value)
            {
                cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                
                if (value >= 91 && value <= 100)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Green); // 91-100: Green
                }
                else if (value >= 81 && value <= 90)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LawnGreen);  // 81-90: Yellow
                }
                else if (value >= 71 && value <= 80)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.PaleGreen);  // 71-80: Orange
                }
                else if (value >= 61 && value <= 70)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightYellow); // 61-70: Orange Red
                }
                else if (value >= 51 && value <= 60)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Yellow);  // 51-60: Yellow
                }
                else if (value >= 41 && value <= 50)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightSalmon);  // 41-50: Orange
                }
                else if (value >= 31 && value <= 40)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.LightCoral); // 31-40: Orange Red
                }
                else if (value >= 21 && value <= 30)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Coral); // 21-30: Orange Red
                }
                else if (value >= 11 && value <= 20)
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.OrangeRed); // 11-20: Orange Red
                }
                else
                {
                    cell.Style.Fill.BackgroundColor.SetColor(Color.Red); // 0-10: Red
                }
                
                
                
                
                
                /*if (normalizedValue >= 0.7)
                {
                    // Good similarity (green)
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 200, 255, 200));
                }
                else if (normalizedValue >= 0.5)
                {
                    // Medium similarity (yellow)
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 255, 200));
                }
                else
                {
                    // Poor similarity (red)
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 200, 200));
                }*/
            }
        }
    }

    public class TextEmbedding
    {
        public double[] Vector { get; set; }
        public string Label { get; set; }

        public TextEmbedding(double[] vector, string label)
        {
            Vector = vector;
            Label = label;
        }
    }
}
