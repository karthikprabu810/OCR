using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ocrApplication
{
    /// <summary>
    /// Provides functionality for summarizing and displaying OCR processing results.
    /// Handles reading best methods from Excel files, displaying summary tables, 
    /// and preparing data for export. This class centralizes all reporting and 
    /// summary generation functionality for OCR results.
    /// </summary>
    public static class OcrSummary
    {
        /// <summary>
        /// Displays the extracted text from all processed images in a formatted console output.
        /// Shows the file name and extracted text for each image that was processed.
        /// </summary>
        /// <param name="extractedTexts">Dictionary containing image paths and their extracted OCR text.</param>
        /// <exception cref="ArgumentNullException">Thrown when extractedTexts is null.</exception>
        public static void DisplayExtractedTexts(ConcurrentDictionary<string, string> extractedTexts)
        {
            if (extractedTexts == null)
                throw new ArgumentNullException(nameof(extractedTexts));
                
            // Header for the extracted text summary section
            Console.WriteLine("\n==================================================");
            Console.WriteLine("SUMMARY OF EXTRACTED TEXT FROM ALL IMAGES");
            Console.WriteLine("==================================================");
            
            // Display each image's extracted text with formatting
            foreach (var entry in extractedTexts)
            {
                // Show only the filename, not the full path
                Console.WriteLine($"\nImage: {Path.GetFileName(entry.Key)}");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"Extracted text: {entry.Value}");
                Console.WriteLine("--------------------------------------------------");
            }
        }
        
        /// <summary>
        /// Reads the best preprocessing methods from Excel files generated during OCR processing.
        /// For each image, attempts to read the Excel file containing the analysis results
        /// and extracts the best methods based on cosine similarity and Levenshtein distance.
        /// </summary>
        /// <param name="imageFiles">Array of image file paths that were processed.</param>
        /// <param name="ocrResultsFolder">Path to the folder containing OCR results.</param>
        /// <returns>Tuple containing dictionaries of best methods by cosine similarity and Levenshtein distance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static (ConcurrentDictionary<string, string>, ConcurrentDictionary<string, string>) 
            ReadBestMethodsFromExcelFiles(string[] imageFiles, string ocrResultsFolder)
        {
            if (imageFiles == null)
                throw new ArgumentNullException(nameof(imageFiles));
                
            if (string.IsNullOrEmpty(ocrResultsFolder))
                throw new ArgumentNullException(nameof(ocrResultsFolder));
                
            // Create thread-safe collections to store the best methods
            var excelBestMethodsByCosine = new ConcurrentDictionary<string, string>();
            var excelBestMethodsByLevenshtein = new ConcurrentDictionary<string, string>();
            
            Console.WriteLine("\nReading best preprocessing methods from Excel files...");
            
            // Process each image file to read its corresponding Excel file
            foreach (string imagePath in imageFiles)
            {
                try
                {
                    // Get the image name without extension to use as folder/file identifier
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
                    
                    // Construct the paths to the OCR results folder and Excel file
                    string imageOcrResultFolder = Path.Combine(ocrResultsFolder, fileNameWithoutExtension);
                    string excelFilePath = Path.Combine(imageOcrResultFolder, $"Comparative_Analysis_{fileNameWithoutExtension}.xlsx");
                    
                    // Only proceed if the Excel file exists
                    if (File.Exists(excelFilePath))
                    {
                        // Read the best methods from the Excel file using ExportUtilities
                        (string? bestCosine, string? bestLevenshtein) = ExportUtilities.ReadBestMethodsFromExcel(excelFilePath);
                        
                        // Store the best method by cosine similarity if it was found
                        if (!string.IsNullOrEmpty(bestCosine))
                        {
                            excelBestMethodsByCosine[fileNameWithoutExtension] = bestCosine;
                        }
                        
                        // Store the best method by Levenshtein distance if it was found
                        if (!string.IsNullOrEmpty(bestLevenshtein))
                        {
                            excelBestMethodsByLevenshtein[fileNameWithoutExtension] = bestLevenshtein;
                        }
                    }
                }
                catch (Exception)
                {
                    // If there's an error reading from Excel, continue with the next file
                    // Errors might include file access issues, corrupted Excel files, etc.
                    continue;
                }
            }
            
            // Return both dictionaries as a tuple
            return (excelBestMethodsByCosine, excelBestMethodsByLevenshtein);
        }
        
        /// <summary>
        /// Displays a summary table of the best preprocessing methods for each image.
        /// Shows a comparison of methods determined by cosine similarity and Levenshtein distance,
        /// along with the overall best method combining both approaches.
        /// </summary>
        /// <param name="bestCosineMethods">Dictionary of best methods by cosine similarity.</param>
        /// <param name="bestLevenshteinMethods">Dictionary of best methods by Levenshtein distance.</param>
        /// <returns>Dictionary of overall best methods determined by combining both metrics.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static Dictionary<string, string> DisplayBestMethodsSummary(
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods)
        {
            if (bestCosineMethods == null)
                throw new ArgumentNullException(nameof(bestCosineMethods));
                
            if (bestLevenshteinMethods == null)
                throw new ArgumentNullException(nameof(bestLevenshteinMethods));
                
            // Header for the best methods summary section
            Console.WriteLine("\n==================================================");
            Console.WriteLine("BEST PREPROCESSING METHODS SUMMARY");
            Console.WriteLine("==================================================");
            
            // Get all unique image names from both dictionaries
            var allImageNames = new HashSet<string>(
                bestCosineMethods.Keys
                .Union(bestLevenshteinMethods.Keys)
            );
            
            // Create an instance of OcrComparison for determining overall best method
            var ocrComparisonForOverall = new OcrComparison();
            
            // Dictionary to store overall best methods
            var overallBestMethods = new Dictionary<string, string>();
            
            // Print a formatted table header with column alignment
            Console.WriteLine("\n{0,-30} | {1,-25} | {2,-25} | {3,-25}", 
                "Image", "Best by Cosine", "Best by Levenshtein", "Overall Best Method");
            Console.WriteLine(new string('-', 115));
            
            // Process each image and display its best methods
            foreach (var imageName in allImageNames)
            {
                // Try to get the best methods from both dictionaries
                bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                
                // Determine overall best method using OcrComparison
                string overallBestMethod = ocrComparisonForOverall.DetermineOverallBestMethod(bestCosine, bestLevenshtein);
                
                // Store the overall best method if it's not null or empty
                if (!string.IsNullOrEmpty(overallBestMethod))
                {
                    overallBestMethods[imageName] = overallBestMethod;
                }
                
                // Display the row in the table with proper formatting
                // Use "N/A" as fallback if a method is not available
                Console.WriteLine("{0,-30} | {1,-25} | {2,-25} | {3,-25}", 
                    imageName, 
                    bestCosine ?? "N/A", 
                    bestLevenshtein ?? "N/A",
                    overallBestMethod ?? "N/A");
            }
            
            // Add a closing line to the table
            Console.WriteLine(new string('-', 115));
            
            return overallBestMethods;
        }
        
        /// <summary>
        /// Generates a complete summary of OCR processing results and exports them.
        /// This method orchestrates the entire summary generation process, including displaying
        /// extracted texts, reading best methods from Excel files, displaying the summary table,
        /// and exporting the results to files.
        /// </summary>
        /// <param name="imageFiles">Array of image file paths that were processed.</param>
        /// <param name="ocrResultsFolder">Path to the folder containing OCR results.</param>
        /// <param name="outputFolderPath">Path to save exported results.</param>
        /// <param name="extractedTexts">Dictionary containing image paths and their extracted OCR text.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public static void GenerateAndExportSummary(
            string[] imageFiles, 
            string ocrResultsFolder, 
            string outputFolderPath,
            ConcurrentDictionary<string, string> extractedTexts)
        {
            if (imageFiles == null)
                throw new ArgumentNullException(nameof(imageFiles));
                
            if (string.IsNullOrEmpty(ocrResultsFolder))
                throw new ArgumentNullException(nameof(ocrResultsFolder));
                
            if (string.IsNullOrEmpty(outputFolderPath))
                throw new ArgumentNullException(nameof(outputFolderPath));
                
            if (extractedTexts == null)
                throw new ArgumentNullException(nameof(extractedTexts));
                
            // Step 1: Display summary of extracted texts
            DisplayExtractedTexts(extractedTexts);
            
            // Step 2: Read best methods from Excel files
            var (bestCosineMethods, bestLevenshteinMethods) = ReadBestMethodsFromExcelFiles(imageFiles, ocrResultsFolder);
            
            // Step 3: Display best methods summary and get overall best methods
            var overallBestMethods = DisplayBestMethodsSummary(bestCosineMethods, bestLevenshteinMethods);
            
            // Step 4: Export results including overall best methods
            ExportUtilities.ExportResults(
                outputFolderPath + "/OCR_Results", 
                extractedTexts, 
                bestCosineMethods, 
                bestLevenshteinMethods, 
                overallBestMethods);
            
            Console.WriteLine("\nSummary generated and exported successfully.");
        }
    }
} 