using System.Text.RegularExpressions;

namespace ocrApplication
{
    public class EnsembleOcr
    {
        // Main method to combine OCR results using Majority Voting based on word positions
        public string CombineUsingMajorityVoting(List<string> ocrResults)
        {
        // Step 1: Split the lines into words and determine the maximum number of words in any line.
        var splitLines = ocrResults.Select(line => line.Split(new[] { ' ', '.', ',', '!', '?', ';', ':' }, StringSplitOptions.RemoveEmptyEntries).ToList()).ToList();
        int maxLength = splitLines.Max(line => line.Count);

        // Step 2: Create a list to store the most frequent word for each position
        List<string> mostFrequentWords = new List<string>();

        // Step 3: Iterate over each position from 0 to maxLength - 1
        for (int i = 0; i < maxLength; i++)
        {
            // Step 4: Use a dictionary to count the frequency of words at the current position across all lines
            Dictionary<string, int> wordVotes = new Dictionary<string, int>();
            bool allWordsPresent = true;
            List<string> wordsAtPosition = new List<string>();

            // Step 5: Iterate through each line and collect words at the current position
            foreach (var line in splitLines)
            {
                if (i < line.Count)
                {
                    wordsAtPosition.Add(line[i].ToLower());
                }
                else
                {
                    allWordsPresent = false;  // If a word is missing, flag this position
                    break;
                }
            }

            // Step 6: Only proceed if all lines have a word at the current position
            if (allWordsPresent)
            {
                // Count the frequency of each word for this position
                foreach (var word in wordsAtPosition)
                {
                    if (!wordVotes.ContainsKey(word))
                    {
                        wordVotes[word] = 0;
                    }
                    wordVotes[word]++;
                }

                // Find the most frequent word at this position
                if (wordVotes.Any())
                {
                    var maxVoteWord = wordVotes.OrderByDescending(w => w.Value).First().Key;
                    mostFrequentWords.Add(maxVoteWord);
                }
            }
        }

        // Step 7: Join the updated words into a single string to return, with a space separating words
        string result = string.Join(" ", mostFrequentWords);
        return result;
    }
    }
}
