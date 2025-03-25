# Extending the OCR Application  

This document provides detailed instructions for developers who want to extend the OCR application by adding new OCR engines, preprocessing techniques, similarity metrics, or output formats. The system follows a modular architecture, making it easy to integrate new features without modifying core functionality.  

## 1️⃣ Adding a New OCR Engine 

To integrate a new OCR engine (e.g., a different API or a custom model), follow these steps:  

### Step 1: Implement the OCR Extraction Method
Modify `OcrExtractionTools.cs` (located in `ocrApplication/OcrExtractionTools.cs`) and add a new method to handle the text extraction.  

```csharp
public async Task<string> ExtractTextWithNewOcr(string imagePath)
{
    // Load the image
    var image = new Bitmap(imagePath);

    // Call the new OCR engine API or library method
    string extractedText = NewOcrEngine.RecognizeText(image);

    return extractedText;
}
```

### Step 2: Register the New OCR Engine in the Processing Pipeline
Modify `OCRExtractionHelper.cs` (in `ocrApplication/OCRExtractionHelper.cs`) to include the new OCR method in the selection process.  

```csharp
Dictionary<string, Func<string, Task<string>>> ocrMethods = new()
{
    { "Tesseract", ExtractTextWithTesseractNuget },
    { "Google Vision", ExtractTextUsingGoogleVisionAsync },
    {try
          {
            // Process the image with IronOCR and get the extracted text
            string newOcrText = ocrTool.ExtractTextUsingNewOcr(imagePath);
         
            // Save the IronOCR result to a separate file
            File.WriteAllText(Path.Combine(ocrToolFolder, $"{methodName}_new_ocr.txt"), newOcrText);
          }
          catch (Exception ex)
          {
            // Handle any other general exceptions
          } 
          
        }// Add the new OCR engine here
};
```

### Step 3: Update Configuration (if API-based OCR is used)
If the new OCR engine requires an API, add its credentials to `config.json`.  

```json
{
    "NewOcrApiKey": "YOUR_API_KEY",
    "NewOcrEndpoint": "https://api.newocr.com/recognize"
}
```

### Step 4: Test the New OCR Engine
Run the following command to ensure the new OCR engine works as expected:  

```bash
dotnet test
```

## 2️⃣ Adding a New Preprocessing Technique

To improve OCR accuracy, you may want to add a new image preprocessing method.  

### Step 1: Implement the New Preprocessing Function*
Add the function inside `ImagePreprocessing.cs` (located in `ocrApplication/ImagePreprocessing.cs`).  

```csharp
public static Mat ApplySharpening(Mat inputImage)
{
    Mat sharpened = new Mat();
    Mat kernel = new Mat(3, 3, MatType.CV_32F, new float[]
    {
        -1, -1, -1,
        -1,  9, -1,
        -1, -1, -1
    });

    Cv2.Filter2D(inputImage, sharpened, inputImage.Depth(), kernel);
    return sharpened;
}
```

### Step 2: Register the New Preprocessing Method
Modify `SelectPreprocessingMethods()` in `OcrProcessor.cs` to include the new function.  

```csharp
(string Name, Func<string, Mat> Method)[] allPreprocessMethods =
{
    ("Grayscale", ApplyGrayscale),
    ("Binarization", ApplyOtsuThresholding),
    ("Sharpening", ApplySharpening) // Add the new preprocessing method here
};
```

### Step 3: Test the New Preprocessing Method
Run the following command:  

```bash
dotnet test
```

## **3️⃣ Adding a New Similarity Metric**  

To improve OCR evaluation, you may want to introduce a new similarity metric.  

### Step 1: Implement the Metric in the Similarity Module
Modify `OcrComparison.cs` (located in `ocrApplication/OcrComparison.cs`) and add the function.  

```csharp
public double CalculateDamerauLevenshteinSimilarity(string text1, string text2)
{
    int distance = DamerauLevenshtein.Distance(text1, text2);
    int maxLength = Math.Max(text1.Length, text2.Length);
    return 1.0 - (double)distance / maxLength;
}
```

### Step 2: Register the Similarity Metric  
Modify `CompareOcrOutputs()` in `OcrProcessor.cs` to include the new metric.  

```csharp
Dictionary<string, Func<string, string, double>> similarityMetrics = new()
{
    { "Levenshtein", CalculateLevenshteinSimilarity },
    { "Cosine", CalculateCosineSimilarity },
    { "Damerau-Levenshtein", CalculateDamerauLevenshteinSimilarity } // Add new metric here
};
```

### Step 3: Update Visualization Module (if needed)  
Modify `SimilarityMatrixGenerator.cs` to include the new metric in heatmap generation.  

### Step 4: Test the New Similarity Metric
```bash
dotnet test
```

## 4️⃣ Adding a New Output Format  

If you want the OCR results exported in a different format (e.g., JSON, CSV, or custom reports), follow these steps:  

### Step 1: Implement the Export Function
Modify `ExportUtilities.cs` (located in `ocrApplication/ExportUtilities.cs`).  

```csharp
public static void ExportToJson(string outputPath, Dictionary<string, string> results)
{
    string jsonOutput = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(Path.Combine(outputPath, "ocr_results.json"), jsonOutput);
}
```

### Step 2: Register the Export Function  
Modify `ExportResults()` to include the new export format.  

```csharp
public static void ExportResults(string outputPath, Dictionary<string, string> extractedTexts)
{
    ExportToJson(outputPath, extractedTexts);
}
```

## 5️⃣ Testing Your Changes  

After adding a new OCR engine, preprocessing method, or similarity metric, ensure everything is working correctly:  

### **Run Unit Tests**  
```bash
cd unitTestProject
dotnet test
```

### **Run the Application with a Sample Image Folder**  
```bash
dotnet run -- --input "test_images/" --output "results/"
```

## 6️⃣ Best Practices for Extending the System*

✅ Keep all new methods **modular and reusable** (avoid modifying core logic).  
✅ **Use dependency injection** for integrating external OCR engines.  
✅ Follow **naming conventions** similar to existing functions.  
✅ Update **documentation & code comments** when adding new features.  
✅ Run **unit tests** after making changes.  

## Conclusion

By following these steps, you can seamlessly extend the OCR application to support additional text recognition engines, preprocessing techniques, similarity metrics, and output formats.  

For further improvements, consider contributing to the main repository or submitting a pull request.