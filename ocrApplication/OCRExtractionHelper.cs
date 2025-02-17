namespace ocrApplication;

public static class OcrExtractionHelper
{
    public static void ProcessOcrForImage(string imagePath, string ocrToolFolder, OcrExtractionTools ocrTool, bool isMacOs, bool isWindows)
    {
        // --- Tesseract OCR ---
        if (isMacOs)
        {
            ocrTool.ExtractTextUsingTesseract(imagePath, ocrToolFolder);
            
            // Define the target text file path as the OCR result file will be moved there
            //string targetOcrFilePath = Path.Combine(ocrToolFolder, Path.GetFileNameWithoutExtension(imagePath) + ".txt");

// Move the file to the target folder and rename it to "original.txt" or preprocessed image name like "grayscale.txt"
            //File.Move(Path.Combine(Directory.GetParent(ocrToolFolder)!.FullName, Path.GetFileNameWithoutExtension(imagePath) + ".txt"), 
              //  targetOcrFilePath); // This assumes the extracted text was initially saved with the same name as the image

// Rename the file as "original.txt" or the corresponding name based on the preprocessing method (like "grayscale.txt")
            //string resultFileName = Path.GetFileNameWithoutExtension(imagePath); // Example: "grayscale"
            //string finalResultFilePath = Path.Combine(ocrToolFolder, $"{resultFileName}.txt");

// Move and rename the file to "original.txt" or "{method}.txt"
           // File.Move(targetOcrFilePath, targetOcrFilePath);
                
            Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        }

        if (isWindows)
        {
            string tesseractText = ocrTool.ExtractTextUsingTesseractWindowsNuGet(imagePath);
            File.WriteAllText(Path.Combine(ocrToolFolder, "tesseract.txt"), tesseractText);
            Console.WriteLine($"Tesseract OCR processed: {imagePath}");
        }
/*
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
        
        */
        
    }
}