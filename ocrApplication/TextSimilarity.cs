namespace ocrApplication;

public class TextSimilarity
{
    // Method for calculating agreement between OCR results
    public double CalculateAgreement(List<string> ocrResults)
    {
        var wordSets = ocrResults.Select(result => new HashSet<string>(result.Split(' '))).ToList();
        var intersection = wordSets.Aggregate((set1, set2) => set1.Intersect(set2).ToHashSet());
        return (double)intersection.Count / ocrResults[0].Split(' ').Length;
    }
}
