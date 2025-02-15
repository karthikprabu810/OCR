using System.Text.RegularExpressions;

namespace ocrApplication
{
    public class EnsembleOcr
    {
        // Main method to combine OCR results using Majority Voting based on word positions
        public string CombineUsingMajorityVoting(List<string> ocrResults)
        {
            // Split each result into lines
            var lines = ocrResults.Select(result => result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)).ToArray();

            // Ensure all lines are of the same length (i.e., there is a consistent number of lines)
            int lineCount = lines.Max(line => line.Length);
            var finalLines = new List<string>();

            // Iterate through each line's word positions
            for (int i = 0; i < lineCount; i++)
            {
                var lineWords = new List<string>();

                // Get the words at the current line and position across all OCR results
                var wordVotes = new Dictionary<string, int>();

                foreach (var result in lines)
                {
                    if (i < result.Length)
                    {
                        // Tokenize the current line into words
                        var words = TokenizeWords(result[i]);

                        // For each word in the line, count its frequency
                        foreach (var word in words)
                        {
                            if (wordVotes.ContainsKey(word))
                            {
                                wordVotes[word]++;
                            }
                            else
                            {
                                wordVotes[word] = 1;
                            }
                        }
                    }
                }

                // Determine the word with the maximum votes
                var mostFrequentWord = wordVotes.OrderByDescending(w => w.Value).ThenBy(w => w.Key).FirstOrDefault().Key;
                lineWords.Add(mostFrequentWord);

                // Join the words in the line to form the final line and add it to the result
                finalLines.Add(string.Join(" ", lineWords));
            }

            // Combine all the lines into the final output
            return string.Join("\n", finalLines);
        }

        // Tokenize the OCR result into individual words
        private List<string> TokenizeWords(string text)
        {
            // Use a regular expression to split the text by any non-word character (non-alphanumeric)
            var words = Regex.Split(text.ToLower(), @"\W+");

            // Filter out empty words and any words that are too short (less than 3 characters, for example)
            return words.Where(w => !string.IsNullOrWhiteSpace(w) && w.Length > 2).ToList();
        }
    }
}
