namespace ocrApplication;
/// <summary>
/// This class provides an alternative ensemble approach that weighs OCR results based on their
/// confidence scores to produce more accurate final text output.
/// </summary>
public class EnsembleOcrWithConfidence
{

    /// <summary>
    /// Combines multiple OCR results by weighing words based on their corresponding confidence scores.
    /// Words that appear in multiple OCR results with high confidence scores will have higher
    /// overall weights and are more likely to be included in the final output.
    /// </summary>
    /// <param name="ocrResults">List of text results from different OCR engines or preprocessing methods</param>
    /// <param name="confidences">List of confidence scores corresponding to each OCR result</param>
    /// <returns>Combined text output with highest confidence words</returns>
    /// <remarks>
    /// The algorithm works by assigning weights to each word based on the confidence score
    /// of the OCR result it came from. Words that appear in multiple results with high confidence
    /// will have higher accumulated weights and are more likely to be included in the final output.
    /// 
    /// The method assumes that the ocrResults and confidences lists have matching indices,
    /// where the confidence at index i corresponds to the OCR result at index i.
    /// </remarks>
    public string CombineWithConfidence(List<string> ocrResults, List<double> confidences)
    {
        // Dictionary to track words and their accumulated confidence scores
        var weightedResults = new Dictionary<string, double>();

        // Process each OCR result along with its confidence score
        for (int i = 0; i < ocrResults.Count; i++)
        {
            var result = ocrResults[i];
            var confidence = confidences[i];
            
            // Split the OCR result into individual words
            var words = result.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Accumulate confidence scores for each word
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
