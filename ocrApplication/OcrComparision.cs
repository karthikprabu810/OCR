namespace ocrApplication;

using System;
using System.Collections.Generic;

public class OcrComparison
{
    public (List<double> levenshteinResults, List<double> cosineResults) CompareOcrResults(List<string> ocrResults, string groundTruth)
    {
        // Initialize the lists to store results
        List<double> levenshteinResults = new List<double>();
        List<double> cosineResults = new List<double>();

        foreach (var result in ocrResults)
        {
            // Calculate Levenshtein similarity for each OCR result
            double levenshteinSimilarity = CalculateLevenshteinSimilarity(result, groundTruth) * 100;
            levenshteinResults.Add(levenshteinSimilarity); // Add the result to the list
        
            // Calculate Cosine similarity for each OCR result
            double cosineSimilarity = CalculateCosineSimilarity(result, groundTruth) * 100;
            cosineResults.Add(cosineSimilarity); // Add the result to the list
        }

        // Return the lists of Levenshtein and Cosine similarities
        return (levenshteinResults, cosineResults);
    }
    /*public (string levenshteinResult, string cosineResult) CompareOcrResults(List<string> ocrResults, string groundTruth)
        {
            // Initialize the lists to store results
            List<double> levenshteinResults = new List<double>();
            List<double> cosineResults = new List<double>();
            
            string levenshteinResult = "";
            string cosineResult = "";
            foreach (var result in ocrResults)
            {
                // Calculate Levenshtein similarity for each OCR result
                double levenshteinSimilarity = CalculateLevenshteinSimilarity(result, groundTruth)*100;
                //totalLevenshteinSimilarity += levenshteinSimilarity;
                levenshteinResult += $"{levenshteinSimilarity:F4}\n";
                
                // Calculate Cosine similarity for each OCR result
                double cosineSimilarity = CalculateCosineSimilarity(result, groundTruth)*100;
                //totalCosineSimilarity += cosineSimilarity;
                cosineResult += $"{cosineSimilarity:F4}\n";
            }

            // Calculate the average similarity for both methods
            //double averageLevenshteinSimilarity = totalLevenshteinSimilarity / ocrResults.Count;
            //double averageCosineSimilarity = totalCosineSimilarity / ocrResults.Count;

            // Append average results to both strings
            //levenshteinResult += $"\nAverage Levenshtein Similarity: {averageLevenshteinSimilarity:F4}\n";
            //cosineResult += $"\nAverage Cosine Similarity: {averageCosineSimilarity:F4}\n";

            return (levenshteinResult, cosineResult);
        }*/

        // Levenshtein Distance-based Similarity
        private double CalculateLevenshteinSimilarity(string ocrResult, string groundTruth)
        {
            int distance = LevenshteinDistance(ocrResult, groundTruth);
            int maxLength = Math.Max(ocrResult.Length, groundTruth.Length);

            // A similarity score between 0 and 1, where 1 means identical strings.
            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int n = s1.Length;
            int m = s2.Length;
            int[,] d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        // Cosine Similarity-based Calculation
        private double CalculateCosineSimilarity(string ocrResult, string groundTruth)
        {
            var ocrVector = GetWordVector(ocrResult);
            var truthVector = GetWordVector(groundTruth);

            return CosineSimilarity(ocrVector, truthVector);
        }

        private Dictionary<string, int> GetWordVector(string text)
        {
            var wordVector = new Dictionary<string, int>();
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

        private double CosineSimilarity(Dictionary<string, int> vector1, Dictionary<string, int> vector2)
        {
            var dotProduct = 0.0;
            var norm1 = 0.0;
            var norm2 = 0.0;

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
}
