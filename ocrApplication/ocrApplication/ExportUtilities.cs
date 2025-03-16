using System.Collections.Concurrent;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.Text;
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
            Dictionary<string, string> overallBestMethods)
        {
            // Call the original method to handle basic export
            ExportResults(outputPath, extractedTexts);
            
            // Export the best methods summary to a separate file
            ExportBestMethodsSummary(outputPath + "_best_methods.txt", bestCosineMethods, bestLevenshteinMethods, 
                overallBestMethods);
        }
        
        /// <summary>
        /// Export best preprocessing methods summary to a text file
        /// </summary>
        private static void ExportBestMethodsSummary(
            string outputPath,
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods,
            Dictionary<string, string> overallBestMethods)
        {
            try
            {
                // Create a string builder to collect the text
                StringBuilder sb = new StringBuilder();
                
                // Add a header
                sb.AppendLine("BEST PREPROCESSING METHODS SUMMARY");
                sb.AppendLine("==================================");
                sb.AppendLine();
                
                // Collect all unique image names
                var allImageNames = new HashSet<string>(
                    bestCosineMethods.Keys
                    .Union(bestLevenshteinMethods.Keys)
                );
                
                // Add a table header
                sb.AppendLine($"{"Image",-30} | {"Best by Cosine",-25} | {"Best by Levenshtein",-25} | {"Overall Best Method",-25}");
                sb.AppendLine(new string('-', 115));
                
                // Add a row for each image
                foreach (var imageName in allImageNames)
                {
                    // Get methods for this image
                    bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                    bestLevenshteinMethods.TryGetValue(imageName, out string bestLev);
                    overallBestMethods.TryGetValue(imageName, out string overallBest);
                    
                    // Add a row to the table
                    sb.AppendLine($"{imageName,-30} | {bestCosine ?? "N/A",-25} | {bestLev ?? "N/A",-25} | {overallBest ?? "N/A",-25}");
                }
                
                // Write the summary to the output file
                File.WriteAllText(outputPath, sb.ToString());
                
                Console.WriteLine($"Best methods summary exported to: {outputPath}");
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
    }
} 