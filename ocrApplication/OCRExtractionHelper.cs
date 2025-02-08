namespace ocrApplication;

public static class OcrExtractionHelper
{
    public static void ProcessOcrForImage(string imagePath, string ocrToolFolder, OcrExtractionTools ocrTool, bool isMacOs, bool isWindows)
    {
        // --- Tesseract OCR ---
        if (isMacOs)
        {
            ocrTool.ExtractTextUsingTesseract(imagePath, ocrToolFolder);
            Console.WriteLine(ocrToolFolder);
            File.Move(Path.Combine(Directory.GetParent(ocrToolFolder)!.FullName, Path.GetFileName(ocrToolFolder) + ".txt"), 
                Path.Combine(ocrToolFolder, "tesseract.txt"));
                
            Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        }

        if (isWindows)
        {
            string tesseractText = ocrTool.ExtractTextUsingTesseractWindowsNuGet(imagePath);
            File.WriteAllText(Path.Combine(ocrToolFolder, "tesseract.txt"), tesseractText);
            Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        }

        // --- IronOCR OCR ---
        string ironOcrText = ocrTool.ExtractTextUsingIronOcr(imagePath);
        File.WriteAllText(Path.Combine(ocrToolFolder, "iron-ocr.txt"), ironOcrText);
        Console.WriteLine($"IronOCR processed: {imagePath}");

        
        // --- Google Vision OCR ---
        string googleVisionOcrText = ocrTool.ExtractTextUsingGoogleVisionAsync(imagePath).Result;
        File.WriteAllText(Path.Combine(ocrToolFolder, "google-vision.txt"), googleVisionOcrText);
        Console.WriteLine($"Google Vision OCR processed: {imagePath}");

        // --- OCR.Space API ---
        string ocrSpaceOcrText = ocrTool.ExtractTextUsingOcrSpaceAsync(imagePath).Result;
        File.WriteAllText(Path.Combine(ocrToolFolder, "ocr-space.txt"), ocrSpaceOcrText);
        Console.WriteLine($"OCR.Space processed: {imagePath}");
        
    }
}