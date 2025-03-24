namespace ocrApplication;

/// <summary>
/// Configuration settings for OCR services and API credentials.
/// Stores paths, API keys, and usage limits loaded from JSON configuration.
/// </summary>
public class OcrConfig
{
    public required string TesseractPath { get; set; }           // Path to the Tesseract OCR executable
    public required string TesseractTessDataPath { get; set; }   // Path to Tesseract language data files
    public required string IronOcrLicenseKey { get; set; }       // License key for IronOCR library
    public required string GoogleVisionApiKey { get; set; }      // API key or credentials path for Google Vision
        
    // API usage tracking
    public int Counter { get; set; }                    // Current API call count
    public int Limit { get; set; }                      // Maximum allowed API calls
        
    public required string ApiUrl { get; set; }                  // Endpoint for external OCR processing
}
