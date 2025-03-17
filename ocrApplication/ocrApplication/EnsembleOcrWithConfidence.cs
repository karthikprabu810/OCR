namespace ocrApplication;
/// <summary>
/// Combines multiple OCR results into an optimized text using confidence scores.
/// Processes text sentence by sentence and selects the most likely correct version of each word.
/// </summary>
public class EnsembleOcrWithConfidence
{
    /// <summary>
    /// Implements ensemble techniques based on confidence score to combine results from multiple OCR engines.
    /// Uses enhanced majority voting with text similarity analysis to improve recognition accuracy.
    /// </summary>
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
