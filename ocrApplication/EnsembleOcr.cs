using System.Text.RegularExpressions;

namespace ocrApplication
{
    public class EnsembleOcr
    {
        private const double SimilarityThreshold = 0.8; // Threshold for Levenshtein similarity
        private const int MaxWordDistance = 3; // Maximum edit distance for words to be considered similar

        // Main method to combine OCR results using enhanced majority voting
        public string CombineUsingMajorityVoting(List<string> ocrResults)
        {
            if (ocrResults == null || ocrResults.Count == 0)
                return string.Empty;
            
            var meanLength = ocrResults.Average(text => text.Length);
            
            // Normalize and clean the OCR results
            var normalizedResults = ocrResults
                .Select(text => NormalizeText(text))
                .Where(text => !string.IsNullOrWhiteSpace(text)&& text.Length >= meanLength / 2)
                .ToList();

            if (normalizedResults.Count == 0)
                return string.Empty;

            // Split into sentences and process each sentence
            var sentences = SplitIntoSentences(normalizedResults);
            var processedSentences = new List<string>();

            foreach (var sentenceGroup in sentences)
            {
                var processedSentence = ProcessSentence(sentenceGroup);
                if (!string.IsNullOrWhiteSpace(processedSentence))
                {
                    processedSentences.Add(processedSentence);
                }
            }

            return string.Join(" ", processedSentences);
        }

        private string ProcessSentence(List<string> sentences)
        {
            // Split sentences into words
            var wordGroups = sentences.Select(s => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()).ToList();
            
            // Get the maximum number of words in any sentence
            int maxWords = wordGroups.Max(g => g.Count);
            
            var finalWords = new List<string>();
            
            // Process each word position
            for (int pos = 0; pos < maxWords; pos++)
            {
                var wordsAtPosition = new Dictionary<string, WordScore>();
                
                // Collect words at this position from all sentences
                foreach (var words in wordGroups)
                {
                    if (pos >= words.Count) continue;
                    
                    var currentWord = words[pos];
                    bool wordAdded = false;
                    
                    // Check for similar existing words
                    foreach (var existingWord in wordsAtPosition.Keys.ToList())
                    {
                        if (AreWordsSimilar(currentWord, existingWord))
                        {
                            wordsAtPosition[existingWord].Frequency++;
                            wordAdded = true;
                            break;
                        }
                    }
                    
                    // Add new word if no similar word found
                    if (!wordAdded)
                    {
                        wordsAtPosition[currentWord] = new WordScore { Word = currentWord, Frequency = 1 };
                    }
                }
                
                // Select the best word for this position
                if (wordsAtPosition.Any())
                {
                    var bestWord = wordsAtPosition
                        .OrderByDescending(w => w.Value.Frequency)
                        .ThenBy(w => w.Key.Length) // Prefer shorter words when frequencies are equal
                        .First().Key;
                    
                    finalWords.Add(bestWord);
                }
            }
            
            return string.Join(" ", finalWords);
        }

        private List<List<string>> SplitIntoSentences(List<string> texts)
        {
            var sentenceGroups = new List<List<string>>();
            var sentenceEndings = new[] { '.', '!', '?' };
            
            foreach (var text in texts)
            {
                var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToList();
                
                foreach (var sentence in sentences)
                {
                    // Find or create a group for similar sentences
                    var similarGroup = sentenceGroups.FirstOrDefault(g => 
                        g.Any(s => CalculateSimilarity(s, sentence) >= SimilarityThreshold));
                    
                    if (similarGroup != null)
                    {
                        similarGroup.Add(sentence);
                    }
                    else
                    {
                        sentenceGroups.Add(new List<string> { sentence });
                    }
                }
            }
            
            return sentenceGroups;
        }

        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Remove multiple spaces and normalize whitespace
            text = Regex.Replace(text, @"\s+", " ");
            
            // Normalize punctuation
            text = Regex.Replace(text, @"[,;:]", ",");
            text = Regex.Replace(text, @"[\.\?!]+", ".");
            
            // Remove special characters but keep basic punctuation
            text = Regex.Replace(text, @"[^\w\s\.,!?]", "");
            
            return text.Trim();
        }

        private bool AreWordsSimilar(string word1, string word2)
        {
            if (string.IsNullOrWhiteSpace(word1) || string.IsNullOrWhiteSpace(word2))
                return false;

            // Quick length check
            if (Math.Abs(word1.Length - word2.Length) > MaxWordDistance)
                return false;

            // Calculate Levenshtein distance
            int distance = CalculateLevenshteinDistance(word1.ToLower(), word2.ToLower());
            return distance <= MaxWordDistance;
        }

        private double CalculateSimilarity(string s1, string s2)
        {
            if (string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2))
                return 0;

            int distance = CalculateLevenshteinDistance(s1.ToLower(), s2.ToLower());
            int maxLength = Math.Max(s1.Length, s2.Length);
            return 1 - ((double)distance / maxLength);
        }

        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,      // deletion
                        d[i, j - 1] + 1),      // insertion
                        d[i - 1, j - 1] + cost); // substitution
                }
            }

            return d[s1.Length, s2.Length];
        }

        private class WordScore
        {
            public string Word { get; set; }
            public int Frequency { get; set; }
            public double Confidence { get; set; }
        }
    }
}
