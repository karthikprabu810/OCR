namespace ocrApplication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Implements text similarity metrics for OCR result evaluation.
    /// Provides methods for Levenshtein, cosine, and word vector similarity analysis.
    /// </summary>
    public class OcrComparison
    {
        /// <summary>
        /// Calculates Levenshtein-based similarity between texts as a percentage.
        /// Higher values indicate more similar character sequences.
        /// </summary>
        /// <param name="ocrResult">OCR text to evaluate</param>
        /// <param name="groundTruth">Reference text for comparison</param>
        /// <returns>Similarity percentage (0-100)</returns>
        public double CalculateLevenshteinSimilarity(string ocrResult, string groundTruth)
        {
            // Handle empty strings
            if (string.IsNullOrEmpty(ocrResult) && string.IsNullOrEmpty(groundTruth))
            {
                // Two empty strings are identical, so return 100% similarity
                return 100.0;
            }
            else if (string.IsNullOrEmpty(ocrResult) || string.IsNullOrEmpty(groundTruth))
            {
                // One empty and one non-empty string have 0% similarity
                return 0.0;
            }

            // Calculate the Levenshtein distance between the two texts
            double distance = LevenshteinDistance(ocrResult, groundTruth);
            // Get the maximum possible distance (length of longer string)
            double maxLength = Math.Max(ocrResult.Length, groundTruth.Length);
            // Convert to a similarity percentage:
            // 1. Divide distance by max length to get a value between 0 and 1
            // 2. Subtract from 1 to invert (higher is better)
            // 3. Multiply by 100 to get a percentage
            // 4. Round to 3 decimal places for readability
            return Math.Round((1.0 - (distance / maxLength))*100,3);
        }

        /// <summary>
        /// Calculates edit distance between two strings.
        /// Counts minimum character insertions, deletions and substitutions needed.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Edit distance value</returns>
        private double LevenshteinDistance(string s1, string s2)
        {
            int n = s1.Length;
            int m = s2.Length;
            // Create distance matrix of size (n+1)×(m+1)
            double[,] d = new double[n + 1, m + 1];

            // Initialize first column and row with progressive distances
            // These represent deletions/insertions of all characters
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            // Fill the matrix using dynamic programming
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    // Determine cost of substitution
                    // If characters match, cost is 0, otherwise 1
                    double cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    
                    // Calculate minimum of three options:
                    // 1. Delete a character from s1
                    // 2. Insert a character into s1
                    // 3. Substitute a character in s1
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            // Return the final distance value
            return d[n, m];
        }

        /// <summary>
        /// Calculates cosine similarity between texts as a percentage.
        /// Measures word frequency pattern similarity regardless of text length.
        /// </summary>
        /// <param name="ocrResult">OCR text to evaluate</param>
        /// <param name="groundTruth">Reference text for comparison</param>
        /// <returns>Similarity percentage (0-100)</returns>
        public double CalculateCosineSimilarity(string ocrResult, string groundTruth)
        {
            // Convert texts to word frequency vectors
            var ocrVector = GetWordVector(ocrResult);
            var truthVector = GetWordVector(groundTruth);
            // Calculate cosine similarity between the vectors
            // and convert to percentage rounded to 3 decimal places
            return Math.Round(CosineSimilarity(ocrVector, truthVector)*100,3);
        }
        
        /// <summary>
        /// Calculates angle-based similarity between word frequency vectors.
        /// Returns values from 0 (completely different) to 1 (identical).
        /// </summary>
        /// <param name="vector1">First word frequency vector</param>
        /// <param name="vector2">Second word frequency vector</param>
        /// <returns>Similarity value (0-1)</returns>
        private double CosineSimilarity(Dictionary<string, double> vector1, Dictionary<string, double> vector2)
        {
            double dotProduct = 0.0;   // Dot product of the two vectors
            double norm1 = 0.0;        // Magnitude (norm) of the first vector
            double norm2 = 0.0;        // Magnitude (norm) of the second vector

            // Calculate dot product (sum of product of corresponding entries)
            // Only consider words that appear in both vectors
            foreach (var word in vector1.Keys)
            {
                if (vector2.ContainsKey(word))
                {
                    dotProduct += vector1[word] * vector2[word];
                }
            }

            // Calculate magnitude (squared) of first vector
            foreach (var val in vector1.Values)
            {
                norm1 += val * val;
            }

            // Calculate magnitude (squared) of second vector
            foreach (var val in vector2.Values)
            {
                norm2 += val * val;
            }

            // Return cosine similarity using the formula:
            // cos(θ) = (A·B) / (||A|| × ||B||)
            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
        
        /// <summary>
        /// Creates word frequency dictionary from text.
        /// Maps each unique word to its occurrence count.
        /// </summary>
        /// <param name="text">Input text</param>
        /// <returns>Word frequency dictionary</returns>
        public Dictionary<string, double> GetWordVector(string text)
        {
            // Initialize a dictionary to store word frequencies
            var wordVector = new Dictionary<string, double>();
            
            // Split the text into words using multiple delimiter characters
            // This removes punctuation and separates words
            var words = text.Split(new[] { ' ', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Process each word
            foreach (var word in words)
            {
                // Convert to lowercase for case-insensitive comparison
                var cleanedWord = word.ToLower();
                
                // Initialize word count if it doesn't exist
                if (!wordVector.ContainsKey(cleanedWord))
                {
                    wordVector[cleanedWord] = 0;
                }
                
                // Increment the word count
                wordVector[cleanedWord]++;
            }
            
            // Return the completed word frequency vector
            return wordVector;
        }
    }
}
