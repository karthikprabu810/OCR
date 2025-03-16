namespace ocrApplication;

/// <summary>
/// Orchestrates OCR processing across multiple engines.
/// Provides platform-specific image processing and result management.
/// </summary>
public static class OcrExtractionHelper
{
    /// <summary>
    /// Processes an image using platform-appropriate OCR engines and saves results.
    /// Applies OS-specific implementations (command-line for macOS, library for Windows).
    /// </summary>
    /// <param name="imagePath">Path to source image</param>
    /// <param name="ocrToolFolder">Output directory for OCR results</param>
    /// <param name="ocrTool">Configured OCR service provider</param>
    /// <param name="isMacOs">True if running on macOS</param>
    /// <param name="isWindows">True if running on Windows</param>
    /// <param name="methodName">Method name to save the file</param>
    public static void ProcessOcrForImage(string imagePath, string ocrToolFolder, OcrExtractionTools ocrTool, bool isMacOs, bool isWindows, string methodName)
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
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(ocrToolFolder), $"{methodName}.txt"), tesseractText);
            // Commented out to reduce console output
            // Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        } 
        
        
         /*
         /// <summary>
         /// Alternate OCR implementations (currently disabled)
         /// </summary>

        // --- IronOCR OCR ---
        // Use IronOCR on all platforms (commercial OCR library)

        // Process the image with IronOCR and get the extracted text
        string ironOcrText = ocrTool.ExtractTextUsingIronOcr(imagePath);

        // Save the IronOCR result to a separate file
        File.WriteAllText(Path.Combine(ocrToolFolder, $"{methodName}_iron_ocr.txt"), ironOcrText);

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