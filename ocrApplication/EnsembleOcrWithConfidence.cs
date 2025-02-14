namespace ocrApplication;

public class EnsembleOcrWithConfidence
{
    public string CombineWithConfidence(List<string> ocrResults, List<double> confidences)
    {
        var weightedResults = new Dictionary<string, double>();

        for (int i = 0; i < ocrResults.Count; i++)
        {
            var result = ocrResults[i];
            var confidence = confidences[i];
            var words = result.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in words)
            {
                if (weightedResults.ContainsKey(word))
                {
                    weightedResults[word] += confidence; // Accumulate confidence for each word
                }
                else
                {
                    weightedResults[word] = confidence;
                }
            }
        }

        // Sort words by the accumulated confidence and return the result with the highest confidence
        var sortedWords = weightedResults.OrderByDescending(w => w.Value).Select(w => w.Key).ToArray();
        return string.Join(" ", sortedWords);
    }
}
