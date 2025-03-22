using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Security.Permissions;
using System.Security;
using OfficeOpenXml;

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
        /// Reads the best preprocessing methods from Excel files created during OCR processing.
        /// </summary>
        /// <param name="outputDirectory">Directory containing the Excel files</param>
        /// <returns>Tuple containing dictionaries mapping image paths to their best methods based on different similarity metrics</returns>
        public static (
            Dictionary<string, string> bestCosineMethods, 
            Dictionary<string, string> bestLevenshteinMethods,
            Dictionary<string, string> bestJaroWinklerMethods,
            Dictionary<string, string> bestJaccardMethods,
            Dictionary<string, string> bestClusteringMethods) ReadBestMethodsFromExcelFiles(string outputDirectory)
        {
            var bestCosineMethods = new Dictionary<string, string>();
            var bestLevenshteinMethods = new Dictionary<string, string>();
            var bestJaroWinklerMethods = new Dictionary<string, string>();
            var bestJaccardMethods = new Dictionary<string, string>();
            var bestClusteringMethods = new Dictionary<string, string>();
            
            try
            {
                // Get all Excel files in the output directory
                var excelFiles = Directory.GetFiles(outputDirectory, "Comparative_Analysis_*.xlsx");
                
                foreach (var excelFile in excelFiles)
                {
                    // Extract image name from file name
                    string fileName = Path.GetFileNameWithoutExtension(excelFile);
                    string imageName = fileName.Replace("Comparative_Analysis_", "");
                    
                    // Read best methods from Excel file
                    using (var package = new ExcelPackage(new FileInfo(excelFile)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets["Similarity Scores"];
                        if (worksheet != null)
                        {
                            // Find the summary section
                            int row = worksheet.Dimension.End.Row - 5; // Assuming fixed layout with summary at end
                            
                            // Get best methods
                            bestCosineMethods[imageName] = worksheet.Cells[row - 5, 2].Value?.ToString();
                            bestLevenshteinMethods[imageName] = worksheet.Cells[row - 4, 2].Value?.ToString();
                            bestJaroWinklerMethods[imageName] = worksheet.Cells[row - 3, 2].Value?.ToString();
                            bestJaccardMethods[imageName] = worksheet.Cells[row - 2, 2].Value?.ToString();
                            bestClusteringMethods[imageName] = worksheet.Cells[row - 1, 2].Value?.ToString();
                        }
                    }
                }
                
                Console.WriteLine($"Read best methods from {excelFiles.Length} Excel files");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading best methods from Excel files: {ex.Message}");
            }
            
            return (bestCosineMethods, bestLevenshteinMethods, bestJaroWinklerMethods, bestJaccardMethods, bestClusteringMethods);
        }
        
        /// <summary>
        /// Displays a summary of the best preprocessing methods for each image based on all similarity metrics.
        /// Creates a console table showing which method performed best for each image according to
        /// cosine similarity, Levenshtein distance, Jaro-Winkler, Jaccard, and clustering analysis.
        /// </summary>
        /// <param name="bestCosineMethods">Dictionary mapping images to their best methods based on cosine similarity</param>
        /// <param name="bestLevenshteinMethods">Dictionary mapping images to their best methods based on Levenshtein distance</param>
        /// <param name="bestJaroWinklerMethods">Dictionary mapping images to their best methods based on Jaro-Winkler similarity</param>
        /// <param name="bestJaccardMethods">Dictionary mapping images to their best methods based on Jaccard similarity</param>
        /// <param name="bestClusteringMethods">Dictionary mapping images to their best methods based on clustering analysis</param>
        /// <returns>
        /// A dictionary mapping each image to its overall best preprocessing method,
        /// determined by combining results from all metrics using a voting mechanism.
        /// </returns>
        public static Dictionary<string, string> DisplayEnhancedBestMethodsSummary(
            Dictionary<string, string> bestCosineMethods,
            Dictionary<string, string> bestLevenshteinMethods,
            Dictionary<string, string> bestJaroWinklerMethods,
            Dictionary<string, string> bestJaccardMethods,
            Dictionary<string, string> bestClusteringMethods)
        {
            if (bestCosineMethods == null)
                throw new ArgumentNullException(nameof(bestCosineMethods));
                
            if (bestLevenshteinMethods == null)
                throw new ArgumentNullException(nameof(bestLevenshteinMethods));
                
            if (bestJaroWinklerMethods == null)
                throw new ArgumentNullException(nameof(bestJaroWinklerMethods));
                
            if (bestJaccardMethods == null)
                throw new ArgumentNullException(nameof(bestJaccardMethods));
                
            if (bestClusteringMethods == null)
                throw new ArgumentNullException(nameof(bestClusteringMethods));
                
            // Header for the best methods summary section
            Console.WriteLine("\n==================================================");
            Console.WriteLine("BEST PREPROCESSING METHODS SUMMARY");
            Console.WriteLine("==================================================");
            
            // Get all unique image names from all dictionaries
            var allImageNames = new HashSet<string>(
                bestCosineMethods.Keys
                .Union(bestLevenshteinMethods.Keys)
                .Union(bestJaroWinklerMethods.Keys)
                .Union(bestJaccardMethods.Keys)
                .Union(bestClusteringMethods.Keys)
            );
            
            // Dictionary to store overall best methods
            var overallBestMethods = new Dictionary<string, string>();
            
            // Print a formatted table header with column alignment
            Console.WriteLine("\n{0,-20} | {1,-16} | {2,-16} | {3,-16} | {4,-16} | {5,-16} | {6,-16}", 
                "Image", "Cosine", "Levenshtein", "Jaro-Winkler", "Jaccard", "Clustering", "Overall Best");
            Console.WriteLine(new string('-', 132));
            
            // Dictionary to count overall occurrences of each method
            var overallMethodCounts = new Dictionary<string, int>();
            
            // Process each image and display its best methods
            foreach (var imageName in allImageNames)
            {
                // Try to get the best methods from all dictionaries
                bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                bestJaroWinklerMethods.TryGetValue(imageName, out string bestJaroWinkler);
                bestJaccardMethods.TryGetValue(imageName, out string bestJaccard);
                bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                
                // Determine overall best method using a simple voting mechanism
                string overallBestMethod = DetermineEnhancedOverallBestMethod(
                    bestCosine, bestLevenshtein, bestJaroWinkler, bestJaccard, bestClustering);
                
                // Store the overall best method if it's not null or empty
                if (!string.IsNullOrEmpty(overallBestMethod))
                {
                    overallBestMethods[imageName] = overallBestMethod;
                }
                
                // Update overall method counts
                AddMethodToCountStatic(overallMethodCounts, overallBestMethod);
                
                // Display the row in the table with proper formatting
                // Use "N/A" as fallback if a method is not available
                Console.WriteLine("{0,-20} | {1,-16} | {2,-16} | {3,-16} | {4,-16} | {5,-16} | {6,-16}", 
                    imageName, 
                    bestCosine ?? "N/A", 
                    bestLevenshtein ?? "N/A",
                    bestJaroWinkler ?? "N/A",
                    bestJaccard ?? "N/A",
                    bestClustering ?? "N/A",
                    overallBestMethod ?? "N/A");
            }
            
            // Add a closing line to the table
            Console.WriteLine(new string('-', 132));
            
            // Display overall counts
            Console.WriteLine("\nOverall Best Method Distribution:");
            Console.WriteLine("==================================================");
            
            foreach (var pair in overallMethodCounts.OrderByDescending(p => p.Value))
            {
                Console.WriteLine($"{pair.Key}: {pair.Value} images ({(double)pair.Value / allImageNames.Count:P1})");
            }
            
            return overallBestMethods;
        }

        /// <summary>
        /// Determines the overall best preprocessing method based on results from all metrics.
        /// Uses a voting system to find consensus among the five similarity metrics.
        /// </summary>
        private static string DetermineEnhancedOverallBestMethod(
            string bestCosineSimilarityMethod, 
            string bestLevenshteinMethod, 
            string bestJaroWinklerMethod,
            string bestJaccardMethod,
            string bestClusteringMethod)
        {
            // Count occurrences of each method
            var methodCounts = new Dictionary<string, int>();
            
            // Add methods that are not null or empty
            AddMethodToCountStatic(methodCounts, bestCosineSimilarityMethod);
            AddMethodToCountStatic(methodCounts, bestLevenshteinMethod);
            AddMethodToCountStatic(methodCounts, bestJaroWinklerMethod);
            AddMethodToCountStatic(methodCounts, bestJaccardMethod);
            AddMethodToCountStatic(methodCounts, bestClusteringMethod);
            
            // Return the method with the highest count, or prioritize based on method priority if tied
            return methodCounts
                .OrderByDescending(x => x.Value)
                .ThenBy(x => GetMethodPriorityStatic(x.Key))
                .FirstOrDefault().Key ?? "N/A";
        }

        // Static helper method to add a method to the count dictionary
        private static void AddMethodToCountStatic(Dictionary<string, int> counts, string method)
        {
            if (!string.IsNullOrEmpty(method) && method != "N/A")
            {
                if (counts.ContainsKey(method))
                {
                    counts[method]++;
                }
                else
                {
                    counts[method] = 1;
                }
            }
        }
        
        // Static helper method to determine method priority (lower is better)
        private static int GetMethodPriorityStatic(string method)
        {
            if (string.IsNullOrEmpty(method) || method == "N/A")
                return int.MaxValue;
            
            // Define priorities for preprocessing methods
            Dictionary<string, int> priorities = new Dictionary<string, int>
            {
                { "Binarization", 1 },
                { "Otsu", 2 },
                { "AdaptiveThresholding", 3 },
                { "GaussianBlur", 4 },
                { "Normalization", 5 },
                { "DilationErosion", 6 },
                { "HistogramEqualization", 7 },
                { "GammaCorrection", 8 },
                { "NoiseReduction", 9 },
                { "Sharpening", 10 },
                { "ContrastEnhancement", 11 },
                { "EdgeEnhancement", 12 },
                { "Original", 20 } // Lower priority for original
            };
            
            return priorities.TryGetValue(method, out int priority) ? priority : 15; // Default priority
        }
        
        /// <summary>
        /// Generates comprehensive OCR summaries and exports them to output files.
        /// Reads best methods, displays summary information, and triggers the export process.
        /// </summary>
        /// <param name="imageFiles">Array of image file paths that were processed</param>
        /// <param name="ocrResultsFolder">Folder containing the OCR result files</param>
        /// <param name="outputFolderPath">Destination folder for exported summary files</param>
        /// <param name="extractedTexts">Dictionary containing image paths and their extracted OCR text</param>
        /// <remarks>
        /// This method orchestrates the entire summary and export process, combining results from
        /// different analyses and creating various output formats (Excel, PDF, etc.).
        /// </remarks>
        public static void GenerateAndExportEnhancedSummary(
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
            var (bestCosineMethods, bestLevenshteinMethods, bestJaroWinklerMethods, bestJaccardMethods, bestClusteringMethods) = 
                ReadBestMethodsFromExcelFiles(ocrResultsFolder);
            
            // Step 3: Display best methods summary and get overall best methods
            var overallBestMethods = DisplayEnhancedBestMethodsSummary(
                bestCosineMethods, 
                bestLevenshteinMethods, 
                bestJaroWinklerMethods, 
                bestJaccardMethods, 
                bestClusteringMethods);
            
            // Step 4: Export results including overall best methods
            ExportUtilities.ExportResults(
                outputFolderPath + "/OCR_Results", 
                extractedTexts, 
                new ConcurrentDictionary<string, string>(bestCosineMethods), 
                new ConcurrentDictionary<string, string>(bestLevenshteinMethods), 
                new ConcurrentDictionary<string, string>(bestClusteringMethods),
                new ConcurrentDictionary<string, string>(bestJaroWinklerMethods),
                new ConcurrentDictionary<string, string>(bestJaccardMethods),
                overallBestMethods);
            
            Console.WriteLine("\nSummary generated and exported successfully.");
        }
    }
} 