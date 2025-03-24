namespace ocrApplication
{
    using System;
    using System.Collections.Generic;
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
            return (dotProduct / (float)(Math.Sqrt(magnitude1) * Math.Sqrt(magnitude2)))*100;
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
        private int CalculateLevenshteinDistance(string text1, string text2)
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
            
            return (maxLength == 0 ? 1.0f : 1.0f - (distance / maxLength))*100;
        }

        /// <summary>
        /// Calculates the Jaro-Winkler similarity between two text strings.
        /// Jaro-Winkler similarity is a string metric that measures the edit distance between two strings,
        /// with a higher weighting for characters that match in the beginning of the strings.
        /// </summary>
        /// <param name="text1">The first text to compare.</param>
        /// <param name="text2">The second text to compare.</param>
        /// <returns>
        /// A float value between 0 and 100, where 100 means identical texts and 0 means completely different.
        /// </returns>
        public float CalculateJaroWinklerSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 100.0f; // Both empty means they're identical

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0f; // One empty and one not means they're completely different

            // Convert texts to lowercase to ensure case-insensitive comparison
            text1 = text1.ToLower();
            text2 = text2.ToLower();

            // Calculate Jaro similarity first
            float jaroSimilarity = CalculateJaroSimilarity(text1, text2);

            // Calculate the common prefix length (up to 4 characters)
            int prefixLength = 0;
            int maxPrefixLength = Math.Min(4, Math.Min(text1.Length, text2.Length));
            
            for (int i = 0; i < maxPrefixLength; i++)
            {
                if (text1[i] == text2[i])
                    prefixLength++;
                else
                    break;
            }

            // Apply the Winkler modification - gives more favorable ratings to strings that match from the beginning
            float scalingFactor = 0.1f; // Standard scaling factor for Jaro-Winkler
            float jaroWinklerSimilarity = jaroSimilarity + (prefixLength * scalingFactor * (1 - jaroSimilarity));

            // Convert to percentage scale (0-100)
            return jaroWinklerSimilarity * 100.0f;
        }

        /// <summary>
        /// Helper method to calculate Jaro similarity between two strings.
        /// This is used as part of the Jaro-Winkler calculation.
        /// </summary>
        /// <param name="s1">The first string.</param>
        /// <param name="s2">The second string.</param>
        /// <returns>
        /// A float value between 0 and 1, representing the Jaro similarity.
        /// </returns>
        private float CalculateJaroSimilarity(string s1, string s2)
        {
            // If both strings are empty, return 1.0 (100% similarity)
            if (s1.Length == 0 && s2.Length == 0)
                return 1.0f;

            // Calculate the matching window size
            int matchWindow = Math.Max(0, Math.Max(s1.Length, s2.Length) / 2 - 1);

            // Arrays to track which characters have been matched
            bool[] matched1 = new bool[s1.Length];
            bool[] matched2 = new bool[s2.Length];

            // Count matching characters
            int matchCount = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                // Calculate the start and end indices of the matching window
                int start = Math.Max(0, i - matchWindow);
                int end = Math.Min(i + matchWindow + 1, s2.Length);

                for (int j = start; j < end; j++)
                {
                    // If s2[j] has already been matched or characters don't match, continue
                    if (matched2[j] || s1[i] != s2[j])
                        continue;

                    // Mark characters as matched and increment the match count
                    matched1[i] = true;
                    matched2[j] = true;
                    matchCount++;
                    break;
                }
            }

            // If no characters match, return 0
            if (matchCount == 0)
                return 0.0f;

            // Count transpositions
            int transpositions = 0;
            int k = 0;

            for (int i = 0; i < s1.Length; i++)
            {
                // Skip non-matching characters
                if (!matched1[i])
                    continue;

                // Find the next matched character in s2
                while (!matched2[k])
                    k++;

                // If the characters don't match, it's a transposition
                if (s1[i] != s2[k])
                    transpositions++;

                k++;
            }

            // Calculate Jaro similarity using the formula
            float m = matchCount;
            transpositions /= 2; // Only half-transpositions are counted
            return (1.0f / 3.0f) * (m / s1.Length + m / s2.Length + (m - transpositions) / m);
        }

        /// <summary>
        /// Calculates the Jaccard similarity between two text strings.
        /// Jaccard similarity measures similarity between finite sample sets as the
        /// size of the intersection divided by the size of the union of the sets.
        /// </summary>
        /// <param name="text1">The first text to compare.</param>
        /// <param name="text2">The second text to compare.</param>
        /// <returns>
        /// A float value between 0 and 100, where 100 means identical texts and 0 means completely different.
        /// </returns>
        public float CalculateJaccardSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 100.0f; // Both empty means they're identical

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0f; // One empty and one not means they're completely different

            // Convert texts to lowercase to ensure case-insensitive comparison
            text1 = text1.ToLower();
            text2 = text2.ToLower();

            // Split texts into words
            string[] words1 = text1.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            string[] words2 = text2.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Create sets of words to efficiently determine intersection and union
            HashSet<string> set1 = new HashSet<string>(words1);
            HashSet<string> set2 = new HashSet<string>(words2);

            // Count words in intersection and union
            int intersection = 0;
            foreach (string word in set1)
            {
                if (set2.Contains(word))
                {
                    intersection++;
                }
            }

            // Calculate union size
            int union = set1.Count + set2.Count - intersection;

            // Avoid division by zero
            if (union == 0)
                return 100.0f; // If both sets are empty, they're identical

            // Calculate Jaccard similarity and convert to percentage (0-100)
            return ((float)intersection / union) * 100.0f;
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
        public Task<string> FindBestCosineMethod(List<string> ocrResults, string groundTruth, List<string> ocrSteps)
        {
            try
            {
                // Make sure we have valid inputs
                if (ocrResults == null || ocrResults.Count == 0 || 
                    string.IsNullOrEmpty(groundTruth) || 
                    ocrSteps == null || ocrSteps.Count == 0)
                {
                    return Task.FromResult("Original"); // Default to original if no valid data
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
                return Task.FromResult(bestMethodIndex >= 0 && bestMethodIndex < ocrSteps.Count 
                    ? ocrSteps[bestMethodIndex] 
                    : "Original");
            }
            catch (Exception ex)
            {
                // Log the error and return the default method
                Console.WriteLine($"Error finding best preprocessing method: {ex.Message}");
                return Task.FromResult("Original");
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
        public Task<string> FindBestLevenshteinMethod(List<string> ocrResults, string groundTruth, List<string> ocrSteps)
        {
            try
            {
                // Make sure we have valid inputs
                if (ocrResults == null || ocrResults.Count == 0 || 
                    string.IsNullOrEmpty(groundTruth) || 
                    ocrSteps == null || ocrSteps.Count == 0)
                {
                    return Task.FromResult("Original"); // Default to original if no valid data
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
                return Task.FromResult(bestMethodIndex >= 0 && bestMethodIndex < ocrSteps.Count 
                    ? ocrSteps[bestMethodIndex] 
                    : "Original");
            }
            catch (Exception ex)
            {
                // Log the error and return the default method
                Console.WriteLine($"Error finding best Levenshtein method: {ex.Message}");
                return Task.FromResult("Original");
            }
        }

        /// <summary>
        /// Finds the best preprocessing method based on Jaro-Winkler similarity comparison against a ground truth.
        /// This method compares each OCR result against the ground truth to determine which preprocessing
        /// method produces results with the highest Jaro-Winkler similarity score.
        /// </summary>
        /// <param name="ocrResults">List of OCR results from different preprocessing methods.</param>
        /// <param name="groundTruth">The ground truth text to compare against.</param>
        /// <param name="ocrSteps">List of preprocessing method names corresponding to the OCR results.</param>
        /// <returns>The name of the preprocessing method that produced the best Jaro-Winkler similarity.</returns>
        public Task<string> FindBestJaroWinklerMethod(List<string> ocrResults, string groundTruth, List<string> ocrSteps)
        {
            try
            {
                // Make sure we have valid inputs
                if (ocrResults == null || ocrResults.Count == 0 || 
                    string.IsNullOrEmpty(groundTruth) || 
                    ocrSteps == null || ocrSteps.Count == 0)
                {
                    return Task.FromResult("Original"); // Default to original if no valid data
                }

                float maxSimilarity = -1;
                int bestMethodIndex = -1;

                // Compare each OCR result against the ground truth
                for (int i = 0; i < ocrResults.Count && i < ocrSteps.Count; i++)
                {
                    if (string.IsNullOrEmpty(ocrResults[i]))
                        continue;  // Skip empty results

                    // Calculate Jaro-Winkler similarity between this OCR result and the ground truth
                    float similarity = CalculateJaroWinklerSimilarity(ocrResults[i], groundTruth);

                    // If this is the best match so far, update the maximum similarity and best method index
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestMethodIndex = i;
                    }
                }

                // Return the name of the best method, or "Original" if none found
                return Task.FromResult(bestMethodIndex >= 0 && bestMethodIndex < ocrSteps.Count 
                    ? ocrSteps[bestMethodIndex] 
                    : "Original");
            }
            catch (Exception ex)
            {
                // Log the error and return the default method
                Console.WriteLine($"Error finding best Jaro-Winkler method: {ex.Message}");
                return Task.FromResult("Original");
            }
        }

        /// <summary>
        /// Finds the best preprocessing method based on Jaccard similarity comparison against a ground truth.
        /// This method compares each OCR result against the ground truth to determine which preprocessing
        /// method produces results with the highest Jaccard similarity score.
        /// </summary>
        /// <param name="ocrResults">List of OCR results from different preprocessing methods.</param>
        /// <param name="groundTruth">The ground truth text to compare against.</param>
        /// <param name="ocrSteps">List of preprocessing method names corresponding to the OCR results.</param>
        /// <returns>The name of the preprocessing method that produced the best Jaccard similarity.</returns>
        public Task<string> FindBestJaccardMethod(List<string> ocrResults, string groundTruth, List<string> ocrSteps)
        {
            try
            {
                // Make sure we have valid inputs
                if (ocrResults == null || ocrResults.Count == 0 || 
                    string.IsNullOrEmpty(groundTruth) || 
                    ocrSteps == null || ocrSteps.Count == 0)
                {
                    return Task.FromResult("Original"); // Default to original if no valid data
                }

                float maxSimilarity = -1;
                int bestMethodIndex = -1;

                // Compare each OCR result against the ground truth
                for (int i = 0; i < ocrResults.Count && i < ocrSteps.Count; i++)
                {
                    if (string.IsNullOrEmpty(ocrResults[i]))
                        continue;  // Skip empty results

                    // Calculate Jaccard similarity between this OCR result and the ground truth
                    float similarity = CalculateJaccardSimilarity(ocrResults[i], groundTruth);

                    // If this is the best match so far, update the maximum similarity and best method index
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestMethodIndex = i;
                    }
                }

                // Return the name of the best method, or "Original" if none found
                return Task.FromResult(bestMethodIndex >= 0 && bestMethodIndex < ocrSteps.Count 
                    ? ocrSteps[bestMethodIndex] 
                    : "Original");
            }
            catch (Exception ex)
            {
                // Log the error and return the default method
                Console.WriteLine($"Error finding best Jaccard method: {ex.Message}");
                return Task.FromResult("Original");
            }
        }
        
        /// <summary>
        /// Calculates similarity between two strings using adaptive methods.
        /// Uses edit distance for short strings and word overlap for longer texts.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Similarity score (0-1) where 1 indicates identical texts</returns>
        public double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2))
                return 0;

            // For short strings, use Levenshtein distance-based similarity
            if (s1.Length < 10 || s2.Length < 10)
            {
                double maxLength = Math.Max(s1.Length, s2.Length);
                double distance = CalculateLevenshteinDistance(s1.ToLower(), s2.ToLower());
                return 1 - (distance / maxLength);
            }

            // For longer strings, use word-based similarity (cosine-like)
            var words1 = s1.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();
            var words2 = s2.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToHashSet();

            // Calculate set overlap coefficient
            int commonWords = words1.Intersect(words2).Count();
            return commonWords / Math.Sqrt(words1.Count * words2.Count);
        }

    }
}
