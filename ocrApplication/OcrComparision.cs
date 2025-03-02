namespace ocrApplication
{
    using System;
    using System.Collections.Generic;

    public class OcrComparison
    {
        
        // Levenshtein Distance-based Similarity (non-static)
        public double CalculateLevenshteinSimilarity(string ocrResult, string groundTruth)
        {
            double distance = LevenshteinDistance(ocrResult, groundTruth);
            double maxLength = Math.Max(ocrResult.Length, groundTruth.Length);
            return Math.Round((1.0 - (distance / maxLength))*100,3);
        }

        // Levenshtein distance (non-static, double type)
        private double LevenshteinDistance(string s1, string s2)
        {
            int n = s1.Length;
            int m = s2.Length;
            double[,] d = new double[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    double cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        // Cosine Similarity-based Calculation (non-static)
        public double CalculateCosineSimilarity(string ocrResult, string groundTruth)
        {
            var ocrVector = GetWordVector(ocrResult);
            var truthVector = GetWordVector(groundTruth);
            return Math.Round(CosineSimilarity(ocrVector, truthVector)*100,3);
        }
        
        // Calculate cosine similarity between two word vectors (non-static)
        private double CosineSimilarity(Dictionary<string, double> vector1, Dictionary<string, double> vector2)
        {
            double dotProduct = 0.0;
            double norm1 = 0.0;
            double norm2 = 0.0;

            foreach (var word in vector1.Keys)
            {
                if (vector2.ContainsKey(word))
                {
                    dotProduct += vector1[word] * vector2[word];
                }
            }

            foreach (var val in vector1.Values)
            {
                norm1 += val * val;
            }

            foreach (var val in vector2.Values)
            {
                norm2 += val * val;
            }

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
        
        // Convert a string into a word vector (non-static)
        public Dictionary<string, double> GetWordVector(string text)
        {
            var wordVector = new Dictionary<string, double>();
            var words = text.Split(new[] { ' ', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                var cleanedWord = word.ToLower();
                if (!wordVector.ContainsKey(cleanedWord))
                {
                    wordVector[cleanedWord] = 0;
                }
                wordVector[cleanedWord]++;
            }
            return wordVector;
        }
    }
}
