using System.Collections.Concurrent;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

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
        public static void ExportResults(string outputPath, ConcurrentDictionary<string, string> extractedTexts)
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

       
    }
} 