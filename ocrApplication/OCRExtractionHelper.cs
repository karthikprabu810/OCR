namespace ocrApplication;

/// <summary>
/// Helper class for OCR extraction operations.
/// Provides methods to process images with various OCR tools and save the results.
/// </summary>
public static class OcrExtractionHelper
{
    /// <summary>
    /// Processes an image with multiple OCR engines and saves the results to separate files.
    /// This method applies different OCR techniques based on the operating system and available tools.
    /// </summary>
    /// <param name="imagePath">Path to the input image file</param>
    /// <param name="ocrToolFolder">Folder where OCR results will be saved</param>
    /// <param name="ocrTool">Instance of OcrExtractionTools with configured API keys</param>
    /// <param name="isMacOs">Flag indicating if running on macOS</param>
    /// <param name="isWindows">Flag indicating if running on Windows</param>
    public static void ProcessOcrForImage(string imagePath, string ocrToolFolder, OcrExtractionTools ocrTool, bool isMacOs, bool isWindows)
    {
        // --- Tesseract OCR ---
        // Use command-line Tesseract on macOS (typically available through Homebrew)
        if (isMacOs)
        {
            // Call the command-line version of Tesseract OCR
            // Results will be saved to the specified output folder by Tesseract itself
            ocrTool.ExtractTextUsingTesseract(imagePath, ocrToolFolder); 
            // Commented out to reduce console output
            // Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        }

        // Use Tesseract via .NET library on Windows (NuGet package)
        if (isWindows)
        {
            // Call the .NET library version of Tesseract OCR
            // Returns the extracted text as a string
            string tesseractText = ocrTool.ExtractTextUsingTesseractWindowsNuGet(imagePath);
            // Manually save the result to a file
            File.WriteAllText(Path.Combine(ocrToolFolder, "tesseract.txt"), tesseractText);
            // Commented out to reduce console output
            // Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        }
        
        
        /*
         /// <summary>
         /// Other Extraction Tools Not in Use
         /// </summary>
         
         
        // --- IronOCR OCR ---
        // Use IronOCR on all platforms (commercial OCR library)
        
        // Process the image with IronOCR and get the extracted text
        string ironOcrText = ocrTool.ExtractTextUsingIronOcr(imagePath);
        
        // Save the IronOCR result to a separate file
        File.WriteAllText(Path.Combine(ocrToolFolder, "iron-ocr.txt"), ironOcrText);
       
        // Log the completion of IronOCR processing
        Console.WriteLine($"IronOCR processed: {imagePath}");
       
       
       
        
        // --- Google Vision OCR ---
        // Use Google Cloud Vision API on all platforms
        
        // Call the Google Vision OCR API asynchronously but wait for the result
        string googleVisionOcrText = ocrTool.ExtractTextUsingGoogleVisionAsync(imagePath).Result;
       
        // Save the Google Vision result to a separate file
        File.WriteAllText(Path.Combine(ocrToolFolder, "google-vision.txt"), googleVisionOcrText);
       
        // Log the completion of Google Vision processing
        Console.WriteLine($"Google Vision OCR processed: {imagePath}");




        // --- OCR.Space API ---
        // Use OCR.Space API on all platforms
        
        // Call the OCR.Space API asynchronously but wait for the result
        string ocrSpaceOcrText = ocrTool.ExtractTextUsingOcrSpaceAsync(imagePath).Result;
        
        // Save the OCR.Space result to a separate file
        File.WriteAllText(Path.Combine(ocrToolFolder, "ocr-space.txt"), ocrSpaceOcrText);
        
        // Log the completion of OCR.Space processing
        Console.WriteLine($"OCR.Space processed: {imagePath}");
        
        // Combine all OCR results into a single file for easier access
        // This combines the outputs from all OCR engines for ensemble processing
        string combinedText = $"IronOCR:\n{ironOcrText}\n\nGoogle Vision:\n{googleVisionOcrText}\n\nOCR.Space:\n{ocrSpaceOcrText}";
        
        // Save the combined result to the output folder
        File.WriteAllText(Path.Combine(ocrToolFolder, "ocr_result.txt"), combinedText);
        */
    }
}