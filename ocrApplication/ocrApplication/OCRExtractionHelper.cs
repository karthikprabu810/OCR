namespace ocrApplication;

/// <summary>
/// Orchestrates OCR processing across multiple engines.
/// Provides platform-specific image processing and result management.
/// This helper class abstracts the details of OCR implementation differences between operating systems.
/// </summary>
public static class OcrExtractionHelper
{
    /// <summary>
    /// Processes an image using platform-appropriate OCR engines and saves results.
    /// Applies OS-specific implementations (command-line for macOS, library for Windows).
    /// </summary>
    /// <param name="imagePath">Path to source image to be processed</param>
    /// <param name="ocrToolFolder">Output directory where OCR results will be saved</param>
    /// <param name="ocrTool">Configured OCR service provider with necessary API keys and settings</param>
    /// <param name="isMacOs">True if running on macOS platform</param>
    /// <param name="isWindows">True if running on Windows platform</param>
    /// <param name="methodName">Preprocessing method name used in output filename</param>
    /// <remarks>
    /// This method currently only uses Tesseract OCR, but has additional OCR engines
    /// implemented and commented out for future use or extension.
    /// </remarks>
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
        
        /* ALTERNATIVE OCR IMPLEMENTATIONS
         * 
         * The following OCR implementations are currently disabled but fully implemented.
         * They can be enabled as needed to provide additional OCR engines for ensemble processing.
         * Each provides different strengths and may perform better on certain types of images.
         * 
         * When enabling these engines:
         * 1. Ensure proper API keys are configured in config.json
         * 2. Consider the cost implications of commercial APIs
         * 3. Uncomment the relevant sections
         * 4. Update the EnsembleOcr class to work with these additional results
         */

        /* IronOCR IMPLEMENTATION
         * 
         * // --- IronOCR OCR ---
         * // Use IronOCR on all platforms (commercial OCR library with good accuracy)
         *
         * // Process the image with IronOCR and get the extracted text
         * string ironOcrText = ocrTool.ExtractTextUsingIronOcr(imagePath);
         *
         * // Save the IronOCR result to a separate file
         * File.WriteAllText(Path.Combine(ocrToolFolder, $"{methodName}_iron_ocr.txt"), ironOcrText);
         *
         * // Log the completion of IronOCR processing
         * Console.WriteLine($"IronOCR processed: {imagePath}");
         */

        /* GOOGLE CLOUD VISION IMPLEMENTATION
         * 
         * // --- Google Vision OCR ---
         * // Use Google Cloud Vision API on all platforms (high accuracy cloud service)
         *
         * // Call the Google Vision OCR API asynchronously but wait for the result
         * string googleVisionOcrText = ocrTool.ExtractTextUsingGoogleVisionAsync(imagePath).Result;
         *
         * // Save the Google Vision result to a separate file
         * File.WriteAllText(Path.Combine(ocrToolFolder, "google-vision.txt"), googleVisionOcrText);
         *
         * // Log the completion of Google Vision processing
         * Console.WriteLine($"Google Vision OCR processed: {imagePath}");
         */

        /* OCR.SPACE API IMPLEMENTATION
         * 
         * // --- OCR.Space API ---
         * // Use OCR.Space API on all platforms (alternative cloud OCR service)
         *
         * // Call the OCR.Space API asynchronously but wait for the result
         * string ocrSpaceOcrText = ocrTool.ExtractTextUsingOcrSpaceAsync(imagePath).Result;
         *
         * // Save the OCR.Space result to a separate file
         * File.WriteAllText(Path.Combine(ocrToolFolder, "ocr-space.txt"), ocrSpaceOcrText);
         *
         * // Log the completion of OCR.Space processing
         * Console.WriteLine($"OCR.Space processed: {imagePath}");
         */


    }
}