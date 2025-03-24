using System.Collections.Concurrent;
using System.Drawing;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace ocrApplication
{
    /// <summary>
    /// Provides utility methods for exporting OCR results to various file formats.
    /// Supports exporting to plain text, PDF, Excel, and other formats, with various
    /// levels of detail and summary information.
    /// </summary>
    public static class ExportUtilities
    {
        /// <summary>
        /// Exports OCR results and best preprocessing method summaries to various formats.
        /// Provides comprehensive analysis of all preprocessing methods' performance 
        /// and which ones worked best for each image.
        /// </summary>
        /// <param name="outputPath">Base directory for output files</param>
        /// <param name="extractedTexts">Dictionary of extracted text for each image</param>
        /// <param name="bestCosineMethods">Dictionary mapping images to their best methods based on cosine similarity</param>
        /// <param name="bestLevenshteinMethods">Dictionary mapping images to their best methods based on Levenshtein distance</param>
        /// <param name="bestClusteringMethods">Dictionary mapping images to their best methods based on clustering analysis</param>
        /// <param name="bestJaroWinklerMethods">Dictionary mapping images to their best methods based on Jaro-Winkler similarity</param>
        /// <param name="bestJaccardMethods">Dictionary mapping images to their best methods based on Jaccard similarity</param>
        /// <param name="overallBestMethods">Dictionary mapping images to their overall best preprocessing methods</param>
        /// <remarks>
        /// This method creates multiple output files:
        /// - Text file with basic OCR results
        /// - PDF with formatted OCR results
        /// - Excel file with detailed best method analysis
        /// - Summary files with preprocessing method comparisons
        /// </remarks>
        public static void ExportResults(
            string outputPath, 
            ConcurrentDictionary<string, string> extractedTexts,
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods,
            ConcurrentDictionary<string, string> bestClusteringMethods,
            ConcurrentDictionary<string, string> bestJaroWinklerMethods,
            ConcurrentDictionary<string, string> bestJaccardMethods,
            Dictionary<string, string> overallBestMethods)
        {
            // Create output directory if it doesn't exist
            Directory.CreateDirectory(outputPath);

            // Ask user if they want to export the ground truth OCR results
            Console.WriteLine("\nDo you want to export the ground truth OCR results?");
            Console.WriteLine("0. No export");
            Console.WriteLine("1. Export as plain text (.txt)");
            Console.WriteLine("2. Export as PDF (.pdf)");
            Console.WriteLine("3. Export in both formats");
            
            bool validChoice = false;
            while (!validChoice)
            {
                Console.Write("Enter your choice (0-3): ");
                string? input = Console.ReadLine();
                
                switch (input?.Trim())
                {
                    case "0":
                        Console.WriteLine("No export selected.");
                        validChoice = true;
                        break;
                    case "1":
                        ExportToPlainText(Path.Combine(outputPath, "OCR_Results.txt"), extractedTexts);
                        Console.WriteLine("Ground truth exported as plain text successfully.");
                        validChoice = true;
                        break;
                    case "2":
                        ExportToPdf(Path.Combine(outputPath, "OCR_Results.pdf"), extractedTexts);
                        Console.WriteLine("Ground truth exported as PDF successfully.");
                        validChoice = true;
                        break;
                    case "3":
                        ExportToPlainText(Path.Combine(outputPath, "OCR_Results.txt"), extractedTexts);
                        ExportToPdf(Path.Combine(outputPath, "OCR_Results.pdf"), extractedTexts);
                        Console.WriteLine("Ground truth exported in both formats successfully.");
                        validChoice = true;
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Please enter a number between 0 and 3.");
                        break;
                }
            }
            
            // Export the best methods summary
            ExportBestMethodsSummary(
                outputPath,
                bestCosineMethods,
                bestLevenshteinMethods,
                bestClusteringMethods,
                bestJaroWinklerMethods,
                bestJaccardMethods,
                overallBestMethods
            );
        }
        
        /// <summary>
        /// Exports OCR results to a plain text file with basic formatting.
        /// Each image's filename and extracted text are included with separator lines.
        /// </summary>
        /// <param name="outputPath">Full path for the output text file</param>
        /// <param name="extractedTexts">Dictionary mapping image paths to their extracted OCR text</param>
        /// <exception cref="Exception">Throws exception if writing to file fails</exception>
        private static void ExportToPlainText(string outputPath, ConcurrentDictionary<string, string> extractedTexts)
        {
            try
            {
                using StreamWriter writer = new StreamWriter(outputPath);
                foreach (var entry in extractedTexts)
                {
                    writer.WriteLine($"Image: {Path.GetFileName(entry.Key)}");
                    writer.WriteLine("--------------------------------------------------");
                    writer.WriteLine($"Extracted text: {entry.Value}");
                    writer.WriteLine("--------------------------------------------------\n");
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Error exporting to plain text file: {outputPath}");
                throw;
            }
        }

        /// <summary>
        /// Exports OCR results to a PDF file with formatted layout.
        /// Creates a professional PDF document with image filenames and their OCR text results.
        /// </summary>
        /// <param name="outputPath">Full path for the output PDF file</param>
        /// <param name="extractedTexts">Dictionary mapping image paths to their extracted OCR text</param>
        /// <exception cref="Exception">Throws exception if PDF generation fails</exception>
        private static void ExportToPdf(string outputPath, ConcurrentDictionary<string, string> extractedTexts)
        {
            try
            {
                using PdfWriter writer = new PdfWriter(outputPath);
                using PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);
                foreach (var entry in extractedTexts)
                {
                    document.Add(new Paragraph($"Image: {Path.GetFileName(entry.Key)}"));
                    document.Add(new Paragraph("--------------------------------------------------"));
                    document.Add(new Paragraph($"Extracted text: {entry.Value}"));
                    document.Add(new Paragraph("--------------------------------------------------\n"));
                }
                
                document.Close();
            }
            catch (Exception)
            {
                Console.WriteLine($"Error exporting to PDF file: {outputPath}");
                throw;
            }
        }
        
        /// <summary>
        /// Exports a summary of best preprocessing methods to Excel format.
        /// Creates a structured Excel file showing which methods performed best for each image
        /// according to different metrics, with detailed analysis and comparisons.
        /// </summary>
        /// <param name="outputPath">Base path for output file (without extension)</param>
        /// <param name="bestCosineMethods">Dictionary mapping images to their best methods based on cosine similarity</param>
        /// <param name="bestLevenshteinMethods">Dictionary mapping images to their best methods based on Levenshtein distance</param>
        /// <param name="bestClusteringMethods">Dictionary mapping images to their best methods based on clustering analysis</param>
        /// <param name="bestJaroWinklerMethods">Dictionary mapping images to their best methods based on Jaro-Winkler similarity</param>
        /// <param name="bestJaccardMethods">Dictionary mapping images to their best methods based on Jaccard similarity</param>
        /// <param name="overallBestMethods">Dictionary mapping images to their overall best preprocessing methods</param>
        private static void ExportBestMethodsSummary(
            string outputPath,
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods,
            ConcurrentDictionary<string, string> bestClusteringMethods,
            ConcurrentDictionary<string, string> bestJaroWinklerMethods,
            ConcurrentDictionary<string, string> bestJaccardMethods,
            Dictionary<string, string> overallBestMethods)
        {
            try
            {
                // Get all unique image names
                var allImageNames = new HashSet<string>(
                    bestCosineMethods.Keys
                    .Union(bestLevenshteinMethods.Keys)
                    .Union(bestClusteringMethods.Keys)
                    .Union(bestJaroWinklerMethods.Keys)
                    .Union(bestJaccardMethods.Keys)
                    .Union(overallBestMethods.Keys)
                );
                
                // Export to plain text
                using (StreamWriter writer = new StreamWriter(Path.Combine(outputPath, "Best_Methods_Summary.txt")))
                {
                    writer.WriteLine("BEST PREPROCESSING METHODS SUMMARY");
                    writer.WriteLine("==================================================\n");
                    
                    // Get the console window width
                    int windowWidth = Console.WindowWidth;
                    // Calculate the width for each of the 7 columns
                    int columnWidth = windowWidth / 7;
                
                    // Create the format string dynamically based on the column width
                    string formatString = string.Format(
                        "{{0,-{0}}} | {{1,-{0}}} | {{2,-{0}}} | {{3,{0}}} | {{4,-{0}}} | {{5,-{0}}} | {{6,-{0}}}",
                        columnWidth
                    );
                    
                    writer.WriteLine(formatString, "Image", "Cosine", "Levenshtein", "Clustering", "Jaro-Winkler", "Jaccard", "Overall Best");
                    writer.WriteLine(new string('-', 132));
                    
                    foreach (var imageName in allImageNames)
                    {
                        bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                        bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                        bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                        bestJaroWinklerMethods.TryGetValue(imageName, out string bestJaroWinkler);
                        bestJaccardMethods.TryGetValue(imageName, out string bestJaccard);
                        overallBestMethods.TryGetValue(imageName, out string overallBest);
                        
                        writer.WriteLine(formatString, imageName, bestCosine ?? "N/A", bestLevenshtein ?? "N/A", bestClustering ?? "N/A", bestJaroWinkler ?? "N/A", bestJaccard ?? "N/A", overallBest ?? "N/A");
                    }
                }
                
                // Export to PDF
                using (PdfWriter writer = new PdfWriter(Path.Combine(outputPath, "Best_Methods_Summary.pdf")))
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);
                    
                    // Simplified approach - just use regular paragraph without trying to set bold
                    document.Add(new Paragraph("BEST PREPROCESSING METHODS SUMMARY"));
                    document.Add(new Paragraph("==================================================\n"));
                    
                    // Create a table with the appropriate columns for the summary
                    Table table = new Table(new float[] { 3, 2, 2, 2, 2, 2, 2 }).UseAllAvailableWidth();
                    
                    // Add headers to the table
                    table.AddHeaderCell("Image");
                    table.AddHeaderCell("Cosine");
                    table.AddHeaderCell("Levenshtein");
                    table.AddHeaderCell("Clustering");
                    table.AddHeaderCell("Jaro-Winkler");
                    table.AddHeaderCell("Jaccard");
                    table.AddHeaderCell("Overall Best");
                    
                    // Add rows to the table for each image
                    foreach (var imageName in allImageNames)
                    {
                        bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                        bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                        bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                        bestJaroWinklerMethods.TryGetValue(imageName, out string bestJaroWinkler);
                        bestJaccardMethods.TryGetValue(imageName, out string bestJaccard);
                        overallBestMethods.TryGetValue(imageName, out string overallBest);
                        
                        table.AddCell(imageName);
                        table.AddCell(bestCosine ?? "N/A");
                        table.AddCell(bestLevenshtein ?? "N/A");
                        table.AddCell(bestClustering ?? "N/A");
                        table.AddCell(bestJaroWinkler ?? "N/A");
                        table.AddCell(bestJaccard ?? "N/A");
                        table.AddCell(overallBest ?? "N/A");
                    }
                    
                    document.Add(table);
                    document.Close();
                }
                
                // Export to Excel
                string excelPath = Path.Combine(outputPath, "Best_Methods_Summary.xlsx");
                FileInfo excelFile = new FileInfo(excelPath);
                
                using (ExcelPackage excelPackage = new ExcelPackage(excelFile))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Best Methods");
                    
                    // Style the header
                    worksheet.Cells[1, 1].Value = "Image";
                    worksheet.Cells[1, 2].Value = "Best by Cosine";
                    worksheet.Cells[1, 3].Value = "Best by Levenshtein";
                    worksheet.Cells[1, 4].Value = "Best by Clustering";
                    worksheet.Cells[1, 5].Value = "Best by Jaro-Winkler";
                    worksheet.Cells[1, 6].Value = "Best by Jaccard";
                    worksheet.Cells[1, 7].Value = "Overall Best";
                    
                    // Format header row
                    using (var headerRange = worksheet.Cells[1, 1, 1, 7])
                    {
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        headerRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        headerRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    
                    // Add data rows
                    int row = 2;
                    foreach (var imageName in allImageNames)
                    {
                        bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                        bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                        bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                        bestJaroWinklerMethods.TryGetValue(imageName, out string bestJaroWinkler);
                        bestJaccardMethods.TryGetValue(imageName, out string bestJaccard);
                        overallBestMethods.TryGetValue(imageName, out string overallBest);
                        
                        worksheet.Cells[row, 1].Value = imageName;
                        worksheet.Cells[row, 2].Value = bestCosine ?? "N/A";
                        worksheet.Cells[row, 3].Value = bestLevenshtein ?? "N/A";
                        worksheet.Cells[row, 4].Value = bestClustering ?? "N/A";
                        worksheet.Cells[row, 5].Value = bestJaroWinkler ?? "N/A";
                        worksheet.Cells[row, 6].Value = bestJaccard ?? "N/A";
                        worksheet.Cells[row, 7].Value = overallBest ?? "N/A";
                        
                        // Highlight the overall best method
                        if (!string.IsNullOrEmpty(overallBest))
                        {
                            // Find which column contains the overall best method
                            for (int col = 2; col <= 6; col++)
                            {
                                if (worksheet.Cells[row, col].Value?.ToString() == overallBest)
                                {
                                    worksheet.Cells[row, col].Style.Font.Color.SetColor(Color.Green);
                                    worksheet.Cells[row, col].Style.Font.Bold = true;
                                }
                            }
                            
                            // Also highlight the overall best column
                            worksheet.Cells[row, 7].Style.Font.Color.SetColor(Color.Green);
                            worksheet.Cells[row, 7].Style.Font.Bold = true;
                        }
                        
                        row++;
                    }
                    
                    // Auto-fit columns
                    worksheet.Cells[1, 1, row - 1, 7].AutoFitColumns();
                    
                    // Save the Excel file
                    excelPackage.Save();
                }
                
                // Console.WriteLine($"Best methods summary exported to {outputPath} (text, PDF, and Excel formats)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting best methods summary: {ex.Message}");
            }
        }
    }
} 