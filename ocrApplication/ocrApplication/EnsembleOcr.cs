using System.Text.RegularExpressions;
using System.Text;
using System.Text.Json;

namespace ocrApplication
{
    /// <summary>
    /// Implements ensemble techniques to combine results from multiple OCR engines.
    /// Uses enhanced majority voting with text similarity analysis to improve recognition accuracy.
    /// </summary>
    public class EnsembleOcr
    {
        // Algorithm tuning parameters
        private const double SimilarityThreshold = 0.8;  // Threshold for considering sentences similar (0-1)
        private const int MaxWordDistance = 3;           // Maximum edit distance for words to be considered similar

        // API endpoint for external OCR processing.
        public readonly string ApiUrl = "http://127.0.0.1:5000/process_ocr"; // Default API URL
        
        /// <summary>
        /// Combines multiple OCR results into an optimized text using enhanced majority voting.
        /// Processes text sentence by sentence and selects the most likely correct version of each word.
        /// </summary>
        /// <param name="ocrResults">List of OCR results from different engines or preprocessing methods</param>
        /// <returns>Optimized text or empty string if no valid input</returns>
        public string CombineUsingMajorityVoting(List<string>? ocrResults)
        {
            // Return empty string for null or empty input
            if (ocrResults == null || ocrResults.Count == 0)
                return string.Empty;
            
            // Calculate mean length to filter out very short results that are likely OCR failures
            var meanLength = ocrResults.Average(text => text.Length);
            
            // Normalize and clean the OCR results
            var normalizedResults = ocrResults
                .Select(text => NormalizeText(text))
                .Where(text => !string.IsNullOrWhiteSpace(text) && text.Length >= meanLength / 2)
                .ToList();

            // Return empty string if all results were filtered out
            if (normalizedResults.Count == 0)
                return string.Empty;

            // Split normalized results into lines
            var allLines = normalizedResults.Select(text => text.Split('\n')).ToList();
            var maxLines = allLines.Max(lines => lines.Length);
            var finalLines = new List<string>();

            // Process each line position
            for (int linePos = 0; linePos < maxLines; linePos++)
            {
                // Collect all available lines at this position
                var linesAtPosition = allLines
                    .Where(lines => linePos < lines.Length)
                    .Select(lines => lines[linePos])
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                if (linesAtPosition.Count > 0)
                {
                    // Group identical lines and count their frequencies
                    var lineFrequencies = linesAtPosition
                        .GroupBy(line => line)
                        .Select(group => new { Text = group.Key, Count = group.Count() })
                        .OrderByDescending(item => item.Count)
                        .ThenBy(item => item.Text.Length) // Prefer shorter text when counts are equal
                        .ToList();

                    // Add the most frequent line
                    finalLines.Add(lineFrequencies.First().Text);
                }
            }

            // Join the lines back together
            return string.Join("\n", finalLines);
        }

        /// <summary>
        /// Generates an optimized version of similar sentences by analyzing word frequencies.
        /// For each word position, selects the most frequent word across all input sentences.
        /// </summary>
        /// <param name="sentences">List of similar sentences from different OCR results</param>
        /// <returns>Optimized sentence constructed from most reliable words</returns>
        private string ProcessSentence(List<string> sentences)
        {
            // Split each sentence into words for position-by-position comparison
            var wordGroups = sentences.Select(s => s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList()).ToList();
            
            // Determine the maximum number of words in any sentence
            int maxWords = wordGroups.Max(g => g.Count);
            
            // List to hold the final selected words
            var finalWords = new List<string>();
            
            // Process each word position across all sentences
            for (int pos = 0; pos < maxWords; pos++)
            {
                // Dictionary to track word frequencies at this position
                var wordsAtPosition = new Dictionary<string, WordScore>();
                
                // Collect all words at this position from all sentences
                foreach (var words in wordGroups)
                {
                    // Skip if this sentence doesn't have a word at this position
                    if (pos >= words.Count) continue;
                    
                    var currentWord = words[pos];
                    bool wordAdded = false;
                    
                    // Check if this word is similar to any existing word
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
                    
                    // Add as new word if no similar word was found
                    if (!wordAdded)
                    {
                        wordsAtPosition[currentWord] = new WordScore { Word = currentWord, Frequency = 1 };
                    }
                }
                
                // Select the best word for this position
                if (wordsAtPosition.Any())
                {
                    // Choose word with highest frequency, preferring shorter words when tied
                    // OCR errors tend to add characters rather than remove them
                    var bestWord = wordsAtPosition
                        .OrderByDescending(w => w.Value.Frequency)
                        .ThenBy(w => w.Key.Length)
                        .First().Key;
                    
                    finalWords.Add(bestWord);
                }
            }
            
            // Join the selected words to form the final sentence
            return string.Join(" ", finalWords);
        }

        /// <summary>
        /// Groups similar sentences from OCR results to handle variations in sentence detection.
        /// Uses text similarity metrics to identify sentences that represent the same content.
        /// </summary>
        /// <param name="texts">List of normalized OCR results</param>
        /// <returns>List of sentence groups with similar content</returns>
        private List<List<string>> SplitIntoSentences(List<string> texts)
        {
            // List to hold groups of similar sentences/lines
            var sentenceGroups = new List<List<string>>();
            
            // Process each OCR result
            foreach (var text in texts)
            {
                // Split text into lines first
                var lines = text.Split('\n');
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    // Split line into sentences using sentence-ending punctuation
                    var sentences = Regex.Split(line, @"(?<=[.!?])\s+")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();
                    
                    // If no sentence breaks found, treat the whole line as one sentence
                    if (sentences.Count == 0)
                    {
                        sentences.Add(line);
                    }
                    
                    // Process each sentence
                    foreach (var sentence in sentences)
                    {
                        // Find a group of similar sentences if one exists
                        var similarGroup = sentenceGroups.FirstOrDefault(g => 
                            g.Any(s => CalculateSimilarity(s, sentence) >= SimilarityThreshold));
                        
                        if (similarGroup != null)
                        {
                            // Add to existing group
                            similarGroup.Add(sentence);
                        }
                        else
                        {
                            // Create new group for unique sentence
                            sentenceGroups.Add(new List<string> { sentence });
                        }
                    }
                }
            }
            
            return sentenceGroups;
        }

        /// <summary>
        /// Standardizes text format to facilitate accurate comparison between OCR results.
        /// Handles spacing, punctuation, and removes special characters.
        /// </summary>
        /// <param name="text">Raw OCR result text</param>
        /// <returns>Normalized text with consistent formatting</returns>
        private string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Preserve line breaks by replacing them with a special marker
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = text.Split('\n');
            
            // Process each line individually
            for (int i = 0; i < lines.Length; i++)
            {
                // Replace multiple spaces with a single space
                lines[i] = Regex.Replace(lines[i], @"\s+", " ");
                
                // Standardize punctuation
                lines[i] = Regex.Replace(lines[i], @"[,;:]", ",");
                lines[i] = Regex.Replace(lines[i], @"[\.\?!]+", ".");
                
                // Remove special characters that could interfere with comparison
                lines[i] = Regex.Replace(lines[i], @"[^\w\s\.,!?]", "");
                
                lines[i] = lines[i].Trim();
            }
            
            // Rejoin lines, preserving empty lines as they might be meaningful for formatting
            return string.Join("\n", lines);
        }

        /// <summary>
        /// Determines if two words are similar based on edit distance.
        /// Accounts for typical OCR errors with length-based threshold adjustments.
        /// </summary>
        /// <param name="word1">First word</param>
        /// <param name="word2">Second word</param>
        /// <returns>True if words are considered similar</returns>
        private bool AreWordsSimilar(string word1, string word2)
        {
            // Handle empty inputs
            if (string.IsNullOrWhiteSpace(word1) || string.IsNullOrWhiteSpace(word2))
                return false;

            // Quick length check to avoid unnecessary calculations
            // If lengths differ too much, words are unlikely to be similar
            if (Math.Abs(word1.Length - word2.Length) > MaxWordDistance)
                return false;

            // For very short words (1-2 chars), require exact match
            if (word1.Length <= 2 || word2.Length <= 2)
                return word1.Equals(word2, StringComparison.OrdinalIgnoreCase);

            // Calculate edit distance between words
            int distance = CalculateLevenshteinDistance(word1.ToLower(), word2.ToLower());
            
            // Words are similar if the edit distance is below threshold
            return distance <= MaxWordDistance;
        }

        /// <summary>
        /// Calculates similarity between two strings using adaptive methods.
        /// Uses edit distance for short strings and word overlap for longer texts.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Similarity score (0-1) where 1 indicates identical texts</returns>
        private double CalculateSimilarity(string s1, string s2)
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

        /// <summary>
        /// Calculates Levenshtein distance (minimum edit distance) between two strings.
        /// Uses dynamic programming for efficient calculation.
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Minimum number of edits needed to transform one string to another</returns>
        private int CalculateLevenshteinDistance(string s1, string s2)
        {
            // Create distance matrix
            int[,] distance = new int[s1.Length + 1, s2.Length + 1];

            // Initialize first row and column
            for (int i = 0; i <= s1.Length; i++)
                distance[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                distance[0, j] = j;

            // Fill the rest of the matrix using dynamic programming
            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[s1.Length, s2.Length];
        }

        /// <summary>
        /// Tracks word frequency for the ensemble voting process.
        /// </summary>
        private class WordScore
        {
            public string Word { get; set; } = string.Empty;
            public int Frequency { get; set; }
        }

        /// <summary>
        /// Processes OCR texts through an external API with local fallback.
        /// Sends results to a configured API endpoint and falls back to local processing if the API fails.
        /// </summary>
        /// <param name="ocrTexts">List of OCR results to process</param>
        /// <returns>Combined OCR result from API or local processing</returns>
        public async Task<string> SendOcrTextsToApiAsync(List<string> ocrTexts)
        {
            if (ocrTexts == null)
                throw new ArgumentNullException(nameof(ocrTexts), "OCR texts list cannot be null");

            try
            {
                // Filter out empty results
                var validOcrTexts = ocrTexts.Where(text => !string.IsNullOrWhiteSpace(text)).ToList();
                if (validOcrTexts.Count == 0)
                    return string.Empty;

                // Create HTTP client with 10-minute timeout per image
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(validOcrTexts.Count * 10);
                
                // Prepare request data
                var requestBody = new
                {
                    texts = validOcrTexts
                };
                
                // Serialize to JSON and send request
                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(ApiUrl, jsonContent);
                
                // Process successful response
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    
                    // Parse JSON response
                    using JsonDocument doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("processed_text", out JsonElement processedText))
                    {
                        return processedText.GetString() ?? string.Empty;
                    }
                }
                
                // Fall back to majority voting if API returns unexpected response
                return CombineUsingMajorityVoting(validOcrTexts);
            }
            catch (Exception ex)
            {
                // Log error and fall back to local processing
                Console.WriteLine($"API error: {ex.Message}");
                return CombineUsingMajorityVoting(ocrTexts);
            }
        }
    }
}
