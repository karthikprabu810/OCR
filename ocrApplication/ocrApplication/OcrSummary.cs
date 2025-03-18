using System.Collections.Concurrent;


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
        /// Reads the best preprocessing methods from Excel files for each image.
        /// Analyzes results based on different similarity metrics (cosine, Levenshtein, clustering)
        /// and determines which preprocessing method performed best for each image.
        /// </summary>
        /// <param name="imageFiles">Array of image file paths that were processed</param>
        /// <param name="ocrResultsFolder">Folder containing the OCR result files</param>
        /// <returns>
        /// A tuple of three dictionaries mapping image paths to their best preprocessing methods based on:
        /// 1. Cosine similarity
        /// 2. Levenshtein distance
        /// 3. Clustering analysis
        /// </returns>
        /// <remarks>
        /// This method expects specific Excel files in the results folder that were generated
        /// during the OCR processing phase. If these files don't exist, empty dictionaries are returned.
        /// </remarks>
        public static (ConcurrentDictionary<string, string>, ConcurrentDictionary<string, string>, ConcurrentDictionary<string, string>) 
            ReadBestMethodsFromExcelFiles(string[] imageFiles, string ocrResultsFolder)
        {
            if (imageFiles == null)
                throw new ArgumentNullException(nameof(imageFiles));
                
            if (string.IsNullOrEmpty(ocrResultsFolder))
                throw new ArgumentNullException(nameof(ocrResultsFolder));
                
            // Create thread-safe dictionaries to store best methods
            var excelBestMethodsByCosine = new ConcurrentDictionary<string, string>();
            var excelBestMethodsByLevenshtein = new ConcurrentDictionary<string, string>();
            var excelBestMethodsByClustering = new ConcurrentDictionary<string, string>();
            
            // Attempt to read Excel files for each image
            foreach (var imagePath in imageFiles)
            {
                try
                {
                    // Get image name without extension
                    string imageName = Path.GetFileNameWithoutExtension(imagePath);
                    
                    // Construct path to the Excel file with comparative analysis
                    string excelFilePath = Path.Combine(ocrResultsFolder, imageName, $"Comparative_Analysis_{imageName}.xlsx");
                    
                    // Check if Excel file exists
                    if (File.Exists(excelFilePath))
                    {
                        // Create instance of ExcelFileReader
                        var fileData = new ExportUtilities.ExcelFileReader();
                        
                        // Read the best methods from Excel
                        var bestCosineSimilarityMethod = fileData.ReadBestCosineSimilarityMethodFromExcel(excelFilePath);
                        var bestLevenshteinMethod = fileData.ReadBestLevenshteinMethodFromExcel(excelFilePath);
                        var bestClusteringMethod = fileData.ReadBestClusteringMethodFromExcel(excelFilePath);
                        
                        // Add to dictionaries if not null or empty
                        if (!string.IsNullOrEmpty(bestCosineSimilarityMethod))
                        {
                            excelBestMethodsByCosine[imageName] = bestCosineSimilarityMethod;
                        }
                        
                        if (!string.IsNullOrEmpty(bestLevenshteinMethod))
                        {
                            excelBestMethodsByLevenshtein[imageName] = bestLevenshteinMethod;
                        }
                        
                        if (!string.IsNullOrEmpty(bestClusteringMethod))
                        {
                            excelBestMethodsByClustering[imageName] = bestClusteringMethod;
                        }
                    }
                }
                catch
                {
                    // If there's an error reading from Excel, continue with the next file
                    // Errors might include file access issues, corrupted Excel files, etc.
                    continue;
                }
            }
            
            // Return all three dictionaries as a tuple
            return (excelBestMethodsByCosine, excelBestMethodsByLevenshtein, excelBestMethodsByClustering);
        }
        
        /// <summary>
        /// Displays a summary of the best preprocessing methods for each image based on different metrics.
        /// Creates a console table showing which method performed best for each image according to
        /// cosine similarity, Levenshtein distance, and clustering analysis.
        /// </summary>
        /// <param name="bestCosineMethods">Dictionary mapping images to their best methods based on cosine similarity</param>
        /// <param name="bestLevenshteinMethods">Dictionary mapping images to their best methods based on Levenshtein distance</param>
        /// <param name="bestClusteringMethods">Dictionary mapping images to their best methods based on clustering analysis</param>
        /// <returns>
        /// A dictionary mapping each image to its overall best preprocessing method,
        /// determined by combining results from all metrics using a voting mechanism.
        /// </returns>
        /// <remarks>
        /// The overall best method is determined by a simplified voting system - if at least two metrics
        /// agree on the best method, that method is chosen. Otherwise, Levenshtein is used as the tiebreaker.
        /// </remarks>
        public static Dictionary<string, string> DisplayBestMethodsSummary(
            ConcurrentDictionary<string, string> bestCosineMethods,
            ConcurrentDictionary<string, string> bestLevenshteinMethods,
            ConcurrentDictionary<string, string> bestClusteringMethods)
        {
            if (bestCosineMethods == null)
                throw new ArgumentNullException(nameof(bestCosineMethods));
                
            if (bestLevenshteinMethods == null)
                throw new ArgumentNullException(nameof(bestLevenshteinMethods));
                
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
                .Union(bestClusteringMethods.Keys)
            );
            
            // Create an instance of OcrComparison for determining overall best method
            var ocrComparisonForOverall = new OcrComparison();
            
            // Dictionary to store overall best methods
            var overallBestMethods = new Dictionary<string, string>();
            
            // Print a formatted table header with column alignment
            Console.WriteLine("\n{0,-25} | {1,-18} | {2,-18} | {3,-18} | {4,-18}", 
                "Image", "Best by Cosine", "Best by Levenshtein", "Best by Clustering", "Overall Best");
            Console.WriteLine(new string('-', 108));
            
            // Process each image and display its best methods
            foreach (var imageName in allImageNames)
            {
                // Try to get the best methods from all dictionaries
                bestCosineMethods.TryGetValue(imageName, out string bestCosine);
                bestLevenshteinMethods.TryGetValue(imageName, out string bestLevenshtein);
                bestClusteringMethods.TryGetValue(imageName, out string bestClustering);
                
                // Determine overall best method using a simple voting mechanism
                string overallBestMethod = DetermineOverallBestMethod(bestCosine, bestLevenshtein, bestClustering);
                
                // Store the overall best method if it's not null or empty
                if (!string.IsNullOrEmpty(overallBestMethod))
                {
                    overallBestMethods[imageName] = overallBestMethod;
                }
                
                // Display the row in the table with proper formatting
                // Use "N/A" as fallback if a method is not available
                Console.WriteLine("{0,-25} | {1,-18} | {2,-18} | {3,-18} | {4,-18}", 
                    imageName, 
                    bestCosine ?? "N/A", 
                    bestLevenshtein ?? "N/A",
                    bestClustering ?? "N/A",
                    overallBestMethod ?? "N/A");
            }
            
            // Add a closing line to the table
            Console.WriteLine(new string('-', 108));
            
            return overallBestMethods;
        }
        
        /// <summary>
        /// Determines the overall best preprocessing method based on results from different metrics.
        /// Uses a voting system to find consensus among the three similarity metrics.
        /// </summary>
        /// <param name="bestCosineSimilarityMethod">Best method according to cosine similarity</param>
        /// <param name="bestLevenshteinMethod">Best method according to Levenshtein distance</param>
        /// <param name="bestClusteringMethod">Best method according to clustering analysis</param>
        /// <returns>
        /// The preprocessing method name that is determined to be best overall.
        /// If there's a tie or no consensus, Levenshtein is prioritized as it's generally most reliable for text.
        /// </returns>
        private static string DetermineOverallBestMethod(string bestCosineSimilarityMethod, string bestLevenshteinMethod, string bestClusteringMethod)
        {
            // Count occurrences of each method
            var methodCounts = new Dictionary<string, int>();
            
            // Add methods that are not null or empty
            if (!string.IsNullOrEmpty(bestCosineSimilarityMethod))
            {
                methodCounts[bestCosineSimilarityMethod] = methodCounts.GetValueOrDefault(bestCosineSimilarityMethod) + 1;
            }
            
            if (!string.IsNullOrEmpty(bestLevenshteinMethod))
            {
                methodCounts[bestLevenshteinMethod] = methodCounts.GetValueOrDefault(bestLevenshteinMethod) + 1;
            }
            
            if (!string.IsNullOrEmpty(bestClusteringMethod))
            {
                methodCounts[bestClusteringMethod] = methodCounts.GetValueOrDefault(bestClusteringMethod) + 1;
            }
            
            // Return the method with the highest count, or the first if tied
            return methodCounts.OrderByDescending(x => x.Value).FirstOrDefault().Key ?? "N/A";
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
            var (bestCosineMethods, bestLevenshteinMethods, bestClusteringMethods) = ReadBestMethodsFromExcelFiles(imageFiles, ocrResultsFolder);
            
            // Step 3: Display best methods summary and get overall best methods
            var overallBestMethods = DisplayBestMethodsSummary(bestCosineMethods, bestLevenshteinMethods, bestClusteringMethods);
            
            // Step 4: Export results including overall best methods
            ExportUtilities.ExportResults(
                outputFolderPath + "/OCR_Results", 
                extractedTexts, 
                bestCosineMethods, 
                bestLevenshteinMethods, 
                bestClusteringMethods,
                overallBestMethods);
            
            Console.WriteLine("\nSummary generated and exported successfully.");
        }
    }
} 