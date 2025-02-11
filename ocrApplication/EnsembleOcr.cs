namespace ocrApplication;

using System.Collections.Generic;
using System.Linq;
public class EnsembleOcr
{
    public string CombineUsingMajorityVoting(List<string> ocrResults)
    {
        var wordFrequency = new Dictionary<string, int>();

        foreach (var result in ocrResults)
        {
            var words = result.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (wordFrequency.ContainsKey(word))
                {
                    wordFrequency[word]++;
                }
                else
                {
                    wordFrequency[word] = 1;
                }
            }
        }

        // Sort words by frequency and return the result with the most common words
        var sortedWords = wordFrequency.OrderByDescending(w => w.Value).Select(w => w.Key).ToArray();
        return string.Join(" ", sortedWords);
    }
}
