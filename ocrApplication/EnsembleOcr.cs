using System.Text.RegularExpressions;

namespace ocrApplication
{
    /// <summary>
    /// Implements ensemble techniques for combining OCR results from multiple engines.
    /// Uses improved majority voting with word similarity measures to produce a more accurate final text.
    /// </summary>
    public class EnsembleOcr
    {
        // Constants for tuning the ensemble algorithm
        private const double SimilarityThreshold = 0.8;  // Threshold for considering two sentences similar (0-1 scale)
        private const int MaxWordDistance = 3;           // Maximum edit distance for words to be considered similar
                                                         // Higher values are more lenient in word matching

        /// <summary>
        /// Combines multiple OCR results into a single optimized text using enhanced majority voting.
        /// The method processes text sentence by sentence and word by word to select the most likely correct version.
        /// </summary>
        /// <param name="ocrResults">List of OCR results from different engines or preprocessing methods</param>
        /// <returns>Combined and optimized text, or empty string if no valid input</returns>
        public string CombineUsingMajorityVoting(List<string>? ocrResults)
        {
            // Return empty string for null or empty input
            if (ocrResults == null || ocrResults.Count == 0)
                return string.Empty;
            
            // Calculate mean length of OCR results for filtering out very short results
            // This helps ignore severely truncated or failed OCR attempts
            var meanLength = ocrResults.Average(text => text.Length);
            
            // Normalize and clean the OCR results
            // 1. Apply text normalization to standardize format
            // 2. Filter out empty texts and ones that are too short (likely OCR failures)
            var normalizedResults = ocrResults
                .Select(text => NormalizeText(text))
                .Where(text => !string.IsNullOrWhiteSpace(text) && text.Length >= meanLength / 2)
                .ToList();

            // If all OCR results were invalid or filtered out, return empty string
            if (normalizedResults.Count == 0)
                return string.Empty;

            // Split OCR results into sentences for more accurate processing
            // Each group in 'sentences' contains similar sentences from different OCR results
            var sentences = SplitIntoSentences(normalizedResults);
            var processedSentences = new List<string>();

            // Process each sentence group to create an optimized version
            foreach (var sentenceGroup in sentences)
            {
                var processedSentence = ProcessSentence(sentenceGroup);
                if (!string.IsNullOrWhiteSpace(processedSentence))
                {
                    processedSentences.Add(processedSentence);
                }
            }

            // Join all processed sentences with spaces to form the final text
            return string.Join(" ", processedSentences);
        }

        /// <summary>
        /// Processes a group of similar sentences to generate an optimized version.
        /// Compares words at each position across all sentences and selects the most frequent one.
        /// </summary>
        /// <param name="sentences">List of similar sentences from different OCR results</param>
        /// <returns>Optimized sentence created from the most reliable words</returns>
        private string ProcessSentence(List<string> sentences)
        {
            // Split each sentence into words for word-by-word processing
            var wordGroups = sentences.Select(s => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()).ToList();
            
            // Determine the maximum word count across all sentences
            // This helps process sentences of different lengths
            int maxWords = wordGroups.Max(g => g.Count);
            
            // List to hold the final selected words
            var finalWords = new List<string>();
            
            // Process each word position across all sentences
            for (int pos = 0; pos < maxWords; pos++)
            {
                // Dictionary to track word frequencies and scores
                // Key: word, Value: object with frequency and confidence data
                var wordsAtPosition = new Dictionary<string, WordScore>();
                
                // Collect all words at this position from all sentences
                foreach (var words in wordGroups)
                {
                    // Skip if this sentence doesn't have a word at this position
                    if (pos >= words.Count) continue;
                    
                    var currentWord = words[pos];
                    bool wordAdded = false;
                    
                    // Check if this word is similar to any word already in our dictionary
                    // This handles slight OCR variations (e.g., "hello" vs "hel1o")
                    foreach (var existingWord in wordsAtPosition.Keys.ToList())
                    {
                        if (AreWordsSimilar(currentWord, existingWord))
                        {
                            // Increment frequency for similar word
                            wordsAtPosition[existingWord].Frequency++;
                            wordAdded = true;
                            break;
                        }
                    }
                    
                    // If no similar word was found, add this as a new word
                    if (!wordAdded)
                    {
                        wordsAtPosition[currentWord] = new WordScore { Word = currentWord, Frequency = 1 };
                    }
                }
                
                // Select the best word for this position based on frequency
                if (wordsAtPosition.Any())
                {
                    // Choose word with highest frequency
                    // If multiple words have the same frequency, prefer shorter words
                    // (OCR tends to add rather than remove characters in errors)
                    var bestWord = wordsAtPosition
                        .OrderByDescending(w => w.Value.Frequency)
                        .ThenBy(w => w.Key.Length) // Prefer shorter words when frequencies are equal
                        .First().Key;
                    
                    finalWords.Add(bestWord);
                }
            }
            
            // Join the selected words to form the final sentence
            return string.Join(" ", finalWords);
        }

        /// <summary>
        /// Splits OCR results into groups of similar sentences for processing.
        /// Groups sentences based on similarity to handle variations in sentence detection.
        /// </summary>
        /// <param name="texts">List of normalized OCR results</param>
        /// <returns>List of sentence groups, where each group contains similar sentences</returns>
        private List<List<string>> SplitIntoSentences(List<string> texts)
        {
            // List to hold groups of similar sentences
            var sentenceGroups = new List<List<string>>();
            
            // Process each OCR result
            foreach (var text in texts)
            {
                // Split text into sentences using regex
                // Looks for sentence-ending punctuation followed by spaces
                var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToList();
                
                // Process each sentence in this OCR result
                foreach (var sentence in sentences)
                {
                    // Find a group of similar sentences, if one exists
                    // Uses the similarity threshold to determine if sentences match
                    var similarGroup = sentenceGroups.FirstOrDefault(g => 
                        g.Any(s => CalculateSimilarity(s, sentence) >= SimilarityThreshold));
                    
                    if (similarGroup != null)
                    {
                        // Add to existing group if similar sentence found
                        similarGroup.Add(sentence);
                    }
                    else
                    {
                        // Create new group if this is a unique sentence
                        sentenceGroups.Add(new List<string> { sentence });
                    }
                }
            }
            
            // Return all sentence groups
            return sentenceGroups;
        }

        /// <summary>
        /// Normalizes text by standardizing spacing, punctuation, and removing special characters.
        /// This makes it easier to compare text from different OCR engines.
        /// </summary>
        /// <param name="text">Raw OCR result text</param>
        /// <returns>Normalized text with standardized formatting</returns>
        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Replace multiple spaces with a single space
            // This handles inconsistent spacing from different OCR engines
            text = Regex.Replace(text, @"\s+", " ");
            
            // Standardize punctuation to simplify comparison
            // Convert various forms of commas/semicolons to simple commas
            text = Regex.Replace(text, @"[,;:]", ",");
            // Convert various forms of sentence endings to simple periods
            text = Regex.Replace(text, @"[\.\?!]+", ".");
            
            // Remove special characters that might cause comparison issues
            // Keep only alphanumeric chars and basic punctuation
            text = Regex.Replace(text, @"[^\w\s\.,!?]", "");
            
            return text.Trim();
        }

        /// <summary>
        /// Determines if two words are similar based on Levenshtein edit distance.
        /// Used to match slightly misspelled words from different OCR results.
        /// </summary>
        /// <param name="word1">First word to compare</param>
        /// <param name="word2">Second word to compare</param>
        /// <returns>True if words are deemed similar, false otherwise</returns>
        private bool AreWordsSimilar(string word1, string word2)
        {
            // Handle empty inputs
            if (string.IsNullOrWhiteSpace(word1) || string.IsNullOrWhiteSpace(word2))
                return false;

            // Quick length check - if lengths differ too much, words are unlikely to be similar
            // This optimization avoids expensive Levenshtein distance calculation
            if (Math.Abs(word1.Length - word2.Length) > MaxWordDistance)
                return false;

            // Calculate Levenshtein distance (number of edits to transform one word to another)
            // Compare lowercase to ignore case differences
            int distance = CalculateLevenshteinDistance(word1.ToLower(), word2.ToLower());
            // Words are similar if the edit distance is within our threshold
            return distance <= MaxWordDistance;
        }

        /// <summary>
        /// Calculates the similarity between two strings as a value from 0 to 1.
        /// Based on normalized Levenshtein distance - higher values indicate greater similarity.
        /// </summary>
        /// <param name="s1">First string to compare</param>
        /// <param name="s2">Second string to compare</param>
        /// <returns>Similarity score between 0 (completely different) and 1 (identical)</returns>
        private double CalculateSimilarity(string s1, string s2)
        {
            // Handle empty inputs
            if (string.IsNullOrWhiteSpace(s1) || string.IsNullOrWhiteSpace(s2))
                return 0;

            // Calculate Levenshtein edit distance between lowercase versions
            int distance = CalculateLevenshteinDistance(s1.ToLower(), s2.ToLower());
            // Get maximum possible distance (length of longer string)
            int maxLength = Math.Max(s1.Length, s2.Length);
            // Convert distance to similarity by normalizing and inverting
            // 0 distance = 1.0 similarity, max distance = 0.0 similarity
            return 1 - ((double)distance / maxLength);
        }

        /// <summary>
        /// Calculates the Levenshtein distance between two strings.
        /// Measures the minimum number of single-character edits needed to change one string to another.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Edit distance (integer representing number of changes needed)</returns>
        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            // Create a matrix to store distances between all prefixes of both strings
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            // Initialize first column - distance from empty string to s1 prefixes
            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;

            // Initialize first row - distance from empty string to s2 prefixes
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            // Fill the rest of the matrix using dynamic programming
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    // Cost is 0 if characters are the same, 1 if different
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    
                    // Calculate minimum cost among three operations:
                    // 1. Delete a character from s1
                    // 2. Insert a character into s1
                    // 3. Substitute a character in s1
                    d[i, j] = Math.Min(Math.Min(
                        d[i - 1, j] + 1,          // deletion
                        d[i, j - 1] + 1),         // insertion
                        d[i - 1, j - 1] + cost);  // substitution
                }
            }

            // Return the final distance between the complete strings
            return d[s1.Length, s2.Length];
        }

        
        
        /// <summary>
        /// Private class to track word statistics for voting algorithm.
        /// Stores information about word frequency and confidence across OCR results.
        /// </summary>
        private class WordScore
        {
            public string Word { get; set; }            // The word text itself
            public int Frequency { get; set; }          // Number of times this word appears across OCR results
            
            // public double Confidence { get; set; }      // Confidence score for this word (not currently used but available for future enhancements)
        }
    }
}
