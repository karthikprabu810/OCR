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
    
    /*
       // Add the first line chart for Levenshtein Similarity
       var chart1 = worksheet.Drawings.AddChart("LevenshteinChart", eChartType.Line);
       chart1.SetPosition(5, 0, 1, 0); // Set position on the sheet (top left)
       chart1.SetSize(600, 400); // Set size of the chart
       chart1.Series.Add(worksheet.Cells["B2:B12"], worksheet.Cells["A2:A12"]); // Levenshtein
       chart1.Title.Text = "Levenshtein Similarity Results"; // Customize chart title

       // Add the second line chart for Cosine Similarity
       var chart2 = worksheet.Drawings.AddChart("CosineChart", eChartType.Line);
       chart2.SetPosition(15, 0, 1, 0); // Set position below the first chart (bottom)
       chart2.SetSize(600, 400); // Set size of the chart
       chart2.Series.Add(worksheet.Cells["C2:C12"], worksheet.Cells["A2:A12"]); // Cosine
       chart2.Title.Text = "Cosine Similarity Results"; // Customize chart title
     */
}
