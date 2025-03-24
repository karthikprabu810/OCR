using System.Collections.Concurrent;

namespace ocrApplication
{
    /// <summary>
    /// Provides functionality for summarizing and displaying OCR processing results.
    /// Handles reading the best methods from Excel files, displaying summary tables, 
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
            
            // Get the console window width
            int windowWidth = Console.WindowWidth;
            // Calculate the width for each of the 7 columns
            int columnWidth = windowWidth / 7;
                
            // Create the format string dynamically based on the column width
            string formatString = string.Format(
                "{{0,-{0}}} | {{1,-{0}}} | {{2,-{0}}} | {{3,{0}}} | {{4,-{0}}} | {{5,-{0}}} | {{6,-{0}}}",
                columnWidth
            );
            
            // Print a formatted table header with column alignment
            Console.WriteLine(formatString, 
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
                Console.WriteLine(formatString, 
                    imageName, 
                    bestCosine, 
                    bestLevenshtein,
                    bestJaroWinkler,
                    bestJaccard,
                    bestClustering,
                    overallBestMethod);
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
    }
} 