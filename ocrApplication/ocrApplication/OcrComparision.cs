using System.Text;

namespace ocrApplication
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods for comparing OCR results using different similarity metrics.
    /// Includes functionality for calculating text similarity using cosine similarity 
    /// and Levenshtein distance to compare OCR outputs against ground truth.
    /// This class is essential for determining the best preprocessing methods for OCR.
    /// </summary>
    public class OcrComparison
    {
        /// <summary>
        /// Calculates the cosine similarity between two text strings.
        /// Cosine similarity measures the cosine of the angle between two vectors,
        /// representing how similar the two texts are regardless of their lengths.
        /// </summary>
        /// <param name="text1">The first text to compare.</param>
        /// <param name="text2">The second text to compare.</param>
        /// <returns>
        /// A float value between 0 and 1, where 1 represents identical texts and 0 represents completely different texts.
        /// </returns>
        public float CalculateCosineSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0;

            // Convert texts to lowercase to ensure case-insensitive comparison
            text1 = text1.ToLower();
            text2 = text2.ToLower();

            // Split texts into words
            string[] words1 = text1.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            string[] words2 = text2.Split(new char[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Create a unique set of all words that appear in either text
            HashSet<string> uniqueWords = new HashSet<string>(words1);
            uniqueWords.UnionWith(words2);

            // Create term frequency vectors for both texts
            Dictionary<string, int> freqVector1 = new Dictionary<string, int>();
            Dictionary<string, int> freqVector2 = new Dictionary<string, int>();

            // Calculate frequency for text1
            foreach (string word in words1)
            {
                if (freqVector1.ContainsKey(word))
                    freqVector1[word]++;
                else
                    freqVector1[word] = 1;
            }

            // Calculate frequency for text2
            foreach (string word in words2)
            {
                if (freqVector2.ContainsKey(word))
                    freqVector2[word]++;
                else
                    freqVector2[word] = 1;
            }

            // Calculate dot product and magnitudes
            float dotProduct = 0;
            float magnitude1 = 0;
            float magnitude2 = 0;

            // For each unique word, add to dot product and magnitudes
            foreach (string word in uniqueWords)
            {
                int count1 = freqVector1.ContainsKey(word) ? freqVector1[word] : 0;
                int count2 = freqVector2.ContainsKey(word) ? freqVector2[word] : 0;

                dotProduct += count1 * count2;
                magnitude1 += count1 * count1;
                magnitude2 += count2 * count2;
            }

            // Prevent division by zero
            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            // Calculate cosine similarity
            return dotProduct / (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2));
        }
        
        /// <summary>
        /// Creates a word frequency dictionary from text.
        /// Maps each unique word to its occurrence count, which is useful for text similarity calculations.
        /// </summary>
        /// <param name="text">Input text to analyze.</param>
        /// <returns>Dictionary mapping words to their frequency counts as double values.</returns>
        public Dictionary<string, double> GetWordVector(string text)
        {
            // Initialize a dictionary to store word frequencies
            var wordVector = new Dictionary<string, double>();
            
            if (string.IsNullOrEmpty(text))
                return wordVector;
            
            // Split the text into words using multiple delimiter characters
            // This removes punctuation and separates words
            var words = text.Split(new[] { ' ', '.', ',', ';', '!', '?', '\n', '\r', '\t' }, 
                StringSplitOptions.RemoveEmptyEntries);
            
            // Process each word
            foreach (var word in words)
            {
                // Convert to lowercase for case-insensitive comparison
                var cleanedWord = word.ToLower();
                
                // Add or increment the word count in the dictionary
                if (!wordVector.ContainsKey(cleanedWord))
                {
                    wordVector[cleanedWord] = 0;
                }
                
                wordVector[cleanedWord]++;
            }
            
            return wordVector;
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two text strings.
        /// Levenshtein distance is a measure of the minimum number of single-character edits 
        /// (insertions, deletions, or substitutions) required to change one text into another.
        /// </summary>
        /// <param name="text1">The first text to compare.</param>
        /// <param name="text2">The second text to compare.</param>
        /// <returns>
        /// An integer representing the minimum number of edits needed to transform text1 into text2.
        /// </returns>
        public int CalculateLevenshteinDistance(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1))
                return string.IsNullOrEmpty(text2) ? 0 : text2.Length;

            if (string.IsNullOrEmpty(text2))
                return text1.Length;

            // Convert to lowercase for case-insensitive comparison
            text1 = text1.ToLower();
            text2 = text2.ToLower();

            // Create a matrix to store the distances
            int[,] distance = new int[text1.Length + 1, text2.Length + 1];

            // Initialize the first column and row
            for (int i = 0; i <= text1.Length; i++)
                distance[i, 0] = i;

            for (int j = 0; j <= text2.Length; j++)
                distance[0, j] = j;

            // Fill the matrix
            for (int i = 1; i <= text1.Length; i++)
            {
                for (int j = 1; j <= text2.Length; j++)
                {
                    // Cost is 0 if characters are the same, 1 otherwise
                    int cost = (text2[j - 1] == text1[i - 1]) ? 0 : 1;

                    // Calculate minimum of three possible operations
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            // Return the Levenshtein distance
            return distance[text1.Length, text2.Length];
        }

        /// <summary>
        /// Calculates Levenshtein similarity as a percentage.
        /// Converts the Levenshtein distance into a similarity score between 0 and 1,
        /// where 1 means identical texts and 0 means completely different.
        /// </summary>
        /// <param name="text1">The first text to compare.</param>
        /// <param name="text2">The second text to compare.</param>
        /// <returns>
        /// A float value between 0 and 1, representing the Levenshtein similarity.
        /// </returns>
        public float CalculateLevenshteinSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0f; // Both empty means they're identical

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0f; // One empty and one not means they're completely different

            // Calculate Levenshtein distance
            int distance = CalculateLevenshteinDistance(text1, text2);

            // Convert distance to similarity score (1 - normalized distance)
            float maxLength = Math.Max(text1.Length, text2.Length);
            return maxLength == 0 ? 1.0f : 1.0f - (distance / maxLength);
        }

        /// <summary>
        /// Finds the best preprocessing method based on cosine similarity comparison against a ground truth.
        /// This method compares each OCR result against the ground truth to determine which preprocessing 
        /// method produces results most similar to the consensus ground truth.
        /// </summary>
        /// <param name="ocrResults">List of OCR results from different preprocessing methods.</param>
        /// <param name="groundTruth">The ground truth text to compare against.</param>
        /// <param name="ocrSteps">List of preprocessing method names corresponding to the OCR results.</param>
        /// <returns>The name of the preprocessing method that produced the best cosine similarity.</returns>
        public async Task<string> FindBestPreprocessingMethod(List<string> ocrResults, string groundTruth, List<string> ocrSteps)
        {
            try
            {
                // Make sure we have valid inputs
                if (ocrResults == null || ocrResults.Count == 0 || 
                    string.IsNullOrEmpty(groundTruth) || 
                    ocrSteps == null || ocrSteps.Count == 0)
                {
                    return "Original"; // Default to original if no valid data
                }

                float maxSimilarity = -1;
                int bestMethodIndex = -1;

                // Compare each OCR result against the ground truth
                for (int i = 0; i < ocrResults.Count && i < ocrSteps.Count; i++)
                {
                    if (string.IsNullOrEmpty(ocrResults[i]))
                        continue;  // Skip empty results

                    // Calculate similarity between this OCR result and the ground truth
                    float similarity = CalculateCosineSimilarity(ocrResults[i], groundTruth);

                    // If this is the best match so far, update the maximum similarity and best method index
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestMethodIndex = i;
                    }
                }

                // Return the name of the best method, or "Original" if none found
                return bestMethodIndex >= 0 && bestMethodIndex < ocrSteps.Count 
                    ? ocrSteps[bestMethodIndex] 
                    : "Original";
            }
            catch (Exception ex)
            {
                // Log the error and return the default method
                Console.WriteLine($"Error finding best preprocessing method: {ex.Message}");
                return "Original";
            }
        }

        /// <summary>
        /// Finds the best preprocessing method based on Levenshtein similarity comparison against a ground truth.
        /// This method compares each OCR result against the ground truth to determine which preprocessing
        /// method produces results with the highest Levenshtein similarity score.
        /// </summary>
        /// <param name="ocrResults">List of OCR results from different preprocessing methods.</param>
        /// <param name="groundTruth">The ground truth text to compare against.</param>
        /// <param name="ocrSteps">List of preprocessing method names corresponding to the OCR results.</param>
        /// <returns>The name of the preprocessing method that produced the best Levenshtein similarity.</returns>
        public async Task<string> FindBestLevenshteinMethod(List<string> ocrResults, string groundTruth, List<string> ocrSteps)
        {
            try
            {
                // Make sure we have valid inputs
                if (ocrResults == null || ocrResults.Count == 0 || 
                    string.IsNullOrEmpty(groundTruth) || 
                    ocrSteps == null || ocrSteps.Count == 0)
                {
                    return "Original"; // Default to original if no valid data
                }

                float maxSimilarity = -1;
                int bestMethodIndex = -1;

                // Compare each OCR result against the ground truth
                for (int i = 0; i < ocrResults.Count && i < ocrSteps.Count; i++)
                {
                    if (string.IsNullOrEmpty(ocrResults[i]))
                        continue;  // Skip empty results

                    // Calculate Levenshtein similarity between this OCR result and the ground truth
                    float similarity = CalculateLevenshteinSimilarity(ocrResults[i], groundTruth);

                    // If this is the best match so far, update the maximum similarity and best method index
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestMethodIndex = i;
                    }
                }

                // Return the name of the best method, or "Original" if none found
                return bestMethodIndex >= 0 && bestMethodIndex < ocrSteps.Count 
                    ? ocrSteps[bestMethodIndex] 
                    : "Original";
            }
            catch (Exception ex)
            {
                // Log the error and return the default method
                Console.WriteLine($"Error finding best Levenshtein method: {ex.Message}");
                return "Original";
            }
        }

        /// <summary>
        /// Determines the overall best preprocessing method by considering both cosine similarity
        /// and Levenshtein metrics. This method combines the results from both similarity measures
        /// to provide a more robust determination of the best method.
        /// </summary>
        /// <param name="cosineBestMethod">The best method according to cosine similarity.</param>
        /// <param name="levenshteinBestMethod">The best method according to Levenshtein similarity.</param>
        /// <returns>
        /// The best overall preprocessing method name. If both metrics agree, that method is returned.
        /// If they disagree, a priority-based decision is made, favoring established preprocessing techniques.
        /// </returns>
        public string DetermineOverallBestMethod(string cosineBestMethod, string levenshteinBestMethod)
        {
            // If both metrics agree, that's the overall best method
            if (cosineBestMethod == levenshteinBestMethod)
                return cosineBestMethod;
                
            // If one of the methods is null or empty, return the other one
            if (string.IsNullOrEmpty(cosineBestMethod))
                return levenshteinBestMethod ?? "Original";
                
            if (string.IsNullOrEmpty(levenshteinBestMethod))
                return cosineBestMethod;
            
            // Define priorities for preprocessing methods
            Dictionary<string, int> methodPriorities = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { "Adaptive_Thresholding", 10 },
                { "Grayscale", 9 },
                { "Deskew", 8 },
                { "Gaussian_Filter", 7 },
                { "Otsu_Binarization", 6 },
                { "Median_Filter", 5 },
                { "Gamma_Correction", 4 },
                { "Histogram_Equalization", 3 },
                { "Original", 2 }
                // Other methods have default priority 0
            };
            
            // Get priorities for both methods
            int cosinePriority = methodPriorities.ContainsKey(cosineBestMethod) ? methodPriorities[cosineBestMethod] : 0;
            int levenshteinPriority = methodPriorities.ContainsKey(levenshteinBestMethod) ? methodPriorities[levenshteinBestMethod] : 0;
            
            // Return method with higher priority
            return cosinePriority >= levenshteinPriority ? cosineBestMethod : levenshteinBestMethod;
        }
    }
}
