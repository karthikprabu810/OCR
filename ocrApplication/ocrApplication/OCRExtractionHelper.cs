namespace ocrApplication;

/// <summary>
/// Orchestrates OCR processing across multiple engines.
/// Provides platform-specific image processing and result management.
/// This helper class abstracts the details of OCR implementation differences between operating systems.
/// </summary>
public static class OcrExtractionHelper
{
    // Flag declared as a class-level variable for License error
    public static bool IronOcrLicensingErrorOccurred;
    public static string IronOcrLicensingErrorMessage = string.Empty;
    public static bool VisionLicensingErrorOccurred;
    public static string VisionLicensingErrorMessage = string.Empty;
    
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
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(ocrToolFolder) ?? string.Empty, $"{methodName}.txt"), tesseractText);
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
         */

        /* IronOCR IMPLEMENTATION
         * 
         * // --- IronOCR OCR ---
         * // Use IronOCR on all platforms (commercial OCR library with good accuracy)
         *
         * try
         * {
         *   // Process the image with IronOCR and get the extracted text
         *   string ironOcrText = ocrTool.ExtractTextUsingIronOcr(imagePath);
         *
         *   // Save the IronOCR result to a separate file
         *   string ironOcrPath = ocrToolFolder.Substring(0, ocrToolFolder.LastIndexOf('/'));
         *   File.WriteAllText(Path.Combine(ocrToolFolder, $"{methodName}_iron_ocr.txt"), ironOcrText);
         * }
         * catch (Exception ex)
         * {
         *   // Handle any other general exceptions
         *   IronOcrLicensingErrorOccurred = true;
         *   IronOcrLicensingErrorMessage= ex.Message;
         * }
         */

        /* GOOGLE CLOUD VISION IMPLEMENTATION
         * 
         * // --- Google Vision OCR ---
         * // Use Google Cloud Vision API on all platforms (high accuracy cloud service)
         * try
         * {
         *   // Call the Google Vision OCR API asynchronously but wait for the result
         *   string googleVisionOcrText = ocrTool.ExtractTextUsingGoogleVisionAsync(imagePath).Result;
         *
         *   // Save the Google Vision result to a separate file
         *   string googleVisionPath = ocrToolFolder.Substring(0, ocrToolFolder.LastIndexOf('/'));
         *   File.WriteAllText(Path.Combine(googleVisionPath, $"{methodName}_google_vision.txt"), googleVisionOcrText);
         * }
         * catch (Exception ex)
         * {
         *   // Handle any other general exceptions
         *   VisionLicensingErrorOccurred = true;
         *   VisionLicensingErrorMessage= ex.Message;
         * }
         */
    }
}