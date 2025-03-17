using System.Collections.Concurrent;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using OfficeOpenXml;

namespace ocrApplication
{
    public static class ExportUtilities
    {
        // Export OCR results to a plain text file
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

        // Export OCR results to a PDF file using iText library
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
            }
            catch (Exception)
            {
                Console.WriteLine($"Error exporting to PDF file: {outputPath}");
                throw;
            }
        }

        // Method to prompt user for export type and perform the export
        private static void ExportResults(string outputPath, ConcurrentDictionary<string, string> extractedTexts)
        {
            bool validSelection = false;
            while (!validSelection)
            {
                Console.WriteLine("Select export type:");
                Console.WriteLine("0. None");
                Console.WriteLine("1. Plain Text");
                Console.WriteLine("2. PDF");
                Console.WriteLine("3. Both");
                Console.Write("Enter your choice (0, 1, 2, or 3): ");
                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        Console.WriteLine("No export selected.");
                        validSelection = true;
                        break;
                    case "1":
                        ExportToPlainText(outputPath + ".txt", extractedTexts);
                        Console.WriteLine("Exported to plain text successfully.");
                        validSelection = true;
                        break;
                    case "2":
                        ExportToPdf(outputPath + ".pdf", extractedTexts);
                        Console.WriteLine("Exported to PDF successfully.");
                        validSelection = true;
                        break;
                    case "3":
                        ExportToPlainText(outputPath + ".txt", extractedTexts);
                        ExportToPdf(outputPath + ".pdf", extractedTexts);
                        Console.WriteLine("Exported to both plain text and PDF successfully.");
                        validSelection = true;
                        break;
                    default:
                        Console.WriteLine("Invalid selection. Please enter 0, 1, 2, or 3.");
                        break;
                }
            }
        }

        // Overloaded method that also includes best preprocessing methods information
        public static void ExportResults(
            string outputPath, 
            ConcurrentDictionary<string, string> extractedTexts,
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods,
            ConcurrentDictionary<string, string> bestClusteringMethods,
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
            
            // Best methods summaries are not exported as per user request
        }
        
        // Exports the summary of best preprocessing methods to various file formats.
        private static void ExportBestMethodsSummary(
            string outputPath,
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods,
            ConcurrentDictionary<string, string> bestClusteringMethods,
            Dictionary<string, string> overallBestMethods)
        {
            try
            {
                // Get all unique image names
                var allImageNames = new HashSet<string>(
                    bestCosineMethods.Keys
                    .Union(bestLevenshteinMethods.Keys)
                    .Union(bestClusteringMethods.Keys)
                    .Union(overallBestMethods.Keys)
                );
                
                // Export to plain text
                using (StreamWriter writer = new StreamWriter(Path.Combine(outputPath, "Best_Methods_Summary.txt")))
                {
                    writer.WriteLine("BEST PREPROCESSING METHODS SUMMARY");
                    writer.WriteLine("==================================================\n");
                    
                    writer.WriteLine(string.Format("{0,-25} | {1,-18} | {2,-18} | {3,-18} | {4,-18}", 
                        "Image", "Best by Cosine", "Best by Levenshtein", "Best by Clustering", "Overall Best"));
                    writer.WriteLine(new string('-', 108));
                    
                    foreach (var imageName in allImageNames)
                    {
                        bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                        bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                        bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                        overallBestMethods.TryGetValue(imageName, out string overallBest);
                        
                        writer.WriteLine(string.Format("{0,-25} | {1,-18} | {2,-18} | {3,-18} | {4,-18}", 
                            imageName, 
                            bestCosine ?? "N/A", 
                            bestLevenshtein ?? "N/A",
                            bestClustering ?? "N/A",
                            overallBest ?? "N/A"));
                    }
                }
                
                // Export to PDF
                using (PdfWriter writer = new PdfWriter(Path.Combine(outputPath, "Best_Methods_Summary.pdf")))
                using (PdfDocument pdf = new PdfDocument(writer))
                {
                    Document document = new Document(pdf);
                    
                    // Add title without using SetBold method
                    Paragraph titleParagraph = new Paragraph("BEST PREPROCESSING METHODS SUMMARY");
                    document.Add(titleParagraph);
                    document.Add(new Paragraph("==================================================\n"));
                    
                    // Create a table for the summary
                    iText.Layout.Element.Table table = new iText.Layout.Element.Table(5);
                    table.AddHeaderCell("Image");
                    table.AddHeaderCell("Best by Cosine");
                    table.AddHeaderCell("Best by Levenshtein");
                    table.AddHeaderCell("Best by Clustering");
                    table.AddHeaderCell("Overall Best");
                    
                    foreach (var imageName in allImageNames)
                    {
                        bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                        bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                        bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                        overallBestMethods.TryGetValue(imageName, out string overallBest);
                        
                        table.AddCell(imageName);
                        table.AddCell(bestCosine ?? "N/A");
                        table.AddCell(bestLevenshtein ?? "N/A");
                        table.AddCell(bestClustering ?? "N/A");
                        table.AddCell(overallBest ?? "N/A");
                    }
                    
                    document.Add(table);
                }
                
                // Export to Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (ExcelPackage package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Best Methods Summary");
                    
                    // Add headers
                    worksheet.Cells[1, 1].Value = "Image";
                    worksheet.Cells[1, 2].Value = "Best by Cosine";
                    worksheet.Cells[1, 3].Value = "Best by Levenshtein";
                    worksheet.Cells[1, 4].Value = "Best by Clustering";
                    worksheet.Cells[1, 5].Value = "Overall Best";
                    
                    // Style headers
                    worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
                    
                    // Add data
                    int row = 2;
                    foreach (var imageName in allImageNames)
                    {
                        bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                        bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                        bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                        overallBestMethods.TryGetValue(imageName, out string overallBest);
                        
                        worksheet.Cells[row, 1].Value = imageName;
                        worksheet.Cells[row, 2].Value = bestCosine ?? "N/A";
                        worksheet.Cells[row, 3].Value = bestLevenshtein ?? "N/A";
                        worksheet.Cells[row, 4].Value = bestClustering ?? "N/A";
                        worksheet.Cells[row, 5].Value = overallBest ?? "N/A";
                        
                        row++;
                    }
                    
                    // Auto-size columns
                    worksheet.Cells[1, 1, row - 1, 5].AutoFitColumns();
                    
                    // Save the Excel file
                    package.SaveAs(new FileInfo(Path.Combine(outputPath, "Best_Methods_Summary.xlsx")));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting best methods summary: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Read the best preprocessing methods from the Excel file
        /// </summary>
        /// <param name="excelFilePath">Path to the Excel file to read from</param>
        /// <returns>A tuple containing the best methods by cosine similarity and Levenshtein distance</returns>
        public static (string? bestCosine, string? bestLevenshtein) ReadBestMethodsFromExcel(string excelFilePath)
        {
            string? bestCosineMethod = null;
            string? bestLevenshteinMethod = null;
            
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
                {
                    // Get the Preprocessing_Effectiveness worksheet
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == "Preprocessing_Effectiveness");
                    if (worksheet == null)
                    {
                        return (null, null);
                    }
                    
                    // Find the row with "Best Preprocessing Method:" label (usually in column 1)
                    int? bestMethodRow = null;
                    
                    for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        string cellValue = worksheet.Cells[row, 1].Text;
                        if (cellValue.Contains("Best Preprocessing Method:", StringComparison.OrdinalIgnoreCase))
                        {
                            bestMethodRow = row;
                            break;
                        }
                    }
                    
                    if (bestMethodRow.HasValue)
                    {
                        // Read the best cosine method from column 2
                        bestCosineMethod = worksheet.Cells[bestMethodRow.Value, 2].Text;
                        if (string.IsNullOrWhiteSpace(bestCosineMethod))
                        {
                            bestCosineMethod = null;
                        }
                        
                        // Read the best Levenshtein method from column 3
                        bestLevenshteinMethod = worksheet.Cells[bestMethodRow.Value, 3].Text;
                        if (string.IsNullOrWhiteSpace(bestLevenshteinMethod))
                        {
                            bestLevenshteinMethod = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading from Excel: {ex.Message}");
            }
            
            return (bestCosineMethod, bestLevenshteinMethod);
        }

        /// <summary>
        /// Provides functionality for reading data from Excel files.
        /// </summary>
        public class ExcelFileReader
        {
            /// <summary>
            /// Reads the best preprocessing method based on cosine similarity from an Excel file.
            /// </summary>
            /// <param name="excelFilePath">Path to the Excel file.</param>
            /// <returns>The best preprocessing method or null if not found.</returns>
            public string? ReadBestCosineSimilarityMethodFromExcel(string excelFilePath)
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(excelFilePath));
                    var worksheet = package.Workbook.Worksheets["CosineDistance"];
                    if (worksheet == null) return null;
                    
                    // Best method is in the second column, 3 rows after the last data row
                    for (int row = worksheet.Dimension.End.Row - 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var label = worksheet.Cells[row, 1].Text;
                        if (label.Contains("Best", StringComparison.OrdinalIgnoreCase))
                        {
                            return worksheet.Cells[row, 2].Text;
                        }
                    }
                    
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Reads the best preprocessing method based on Levenshtein distance from an Excel file.
            /// </summary>
            /// <param name="excelFilePath">Path to the Excel file.</param>
            /// <returns>The best preprocessing method or null if not found.</returns>
            public string? ReadBestLevenshteinMethodFromExcel(string excelFilePath)
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(excelFilePath));
                    var worksheet = package.Workbook.Worksheets["Levenshtein"];
                    if (worksheet == null) return null;
                    
                    // Best method is in the second column, 3 rows after the last data row
                    for (int row = worksheet.Dimension.End.Row - 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var label = worksheet.Cells[row, 1].Text;
                        if (label.Contains("Best", StringComparison.OrdinalIgnoreCase))
                        {
                            return worksheet.Cells[row, 2].Text;
                        }
                    }
                    
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Reads the best preprocessing method based on clustering analysis from an Excel file.
            /// </summary>
            /// <param name="excelFilePath">Path to the Excel file.</param>
            /// <returns>The best preprocessing method or null if not found.</returns>
            public string? ReadBestClusteringMethodFromExcel(string excelFilePath)
            {
                try
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using var package = new ExcelPackage(new FileInfo(excelFilePath));
                    var worksheet = package.Workbook.Worksheets["clusterAnalysis"];
                    if (worksheet == null) return null;
                    
                    // Find the best preprocessing method in the clustering worksheet
                    // First look for a cell with "Best Preprocessing Method" label
                    for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var label = worksheet.Cells[row, 1].Text;
                        if (label.Contains("Best Preprocessing Method", StringComparison.OrdinalIgnoreCase))
                        {
                            return worksheet.Cells[row, 2].Text;
                        }
                    }
                    
                    // If not found by label, try looking for a cell with "Yes" in the "Is Best Method" column
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        if (worksheet.Dimension.End.Column >= 3)
                        {
                            var isBest = worksheet.Cells[row, 3].Text;
                            if (isBest.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                return worksheet.Cells[row, 1].Text;
                            }
                        }
                    }
                    
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
} 