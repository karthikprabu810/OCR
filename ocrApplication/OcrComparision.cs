namespace ocrApplication
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods to compare OCR results with ground truth or other OCR results.
    /// Implements different text similarity metrics including Levenshtein distance and Cosine similarity.
    /// </summary>
    public class OcrComparison
    {
        /// <summary>
        /// Calculates the similarity between an OCR result and ground truth using Levenshtein distance.
        /// Returns a percentage value where 100% means identical texts and 0% means completely different.
        /// This metric is based on character-level edit distance and is good for detecting spelling errors.
        /// </summary>
        /// <param name="ocrResult">The OCR result text to evaluate</param>
        /// <param name="groundTruth">The ground truth text to compare against</param>
        /// <returns>Similarity percentage between 0 and 100</returns>
        public double CalculateLevenshteinSimilarity(string ocrResult, string groundTruth)
        {
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
        /// Calculates the Levenshtein distance between two strings.
        /// Measures the minimum number of single-character edits (insertions, deletions, substitutions)
        /// required to change one string into the other.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>The edit distance as a double value</returns>
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
        /// Calculates the similarity between an OCR result and ground truth using Cosine similarity.
        /// Returns a percentage value where 100% means identical text content and 0% means no overlap.
        /// This metric is based on word frequency and is good for detecting content similarity
        /// regardless of word order or document length.
        /// </summary>
        /// <param name="ocrResult">The OCR result text to evaluate</param>
        /// <param name="groundTruth">The ground truth text to compare against</param>
        /// <returns>Similarity percentage between 0 and 100</returns>
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
        /// Calculates the cosine similarity between two word frequency vectors.
        /// Cosine similarity measures the cosine of the angle between vectors,
        /// representing how similar their orientations are in the vector space.
        /// </summary>
        /// <param name="vector1">First word frequency vector</param>
        /// <param name="vector2">Second word frequency vector</param>
        /// <returns>Similarity value between 0 and 1</returns>
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
        /// Converts a text string into a word frequency vector.
        /// Splits the text into words and counts the occurrence of each word.
        /// This creates a bag-of-words representation for similarity calculations.
        /// </summary>
        /// <param name="text">Input text string</param>
        /// <returns>Dictionary where keys are words and values are their frequencies</returns>
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
