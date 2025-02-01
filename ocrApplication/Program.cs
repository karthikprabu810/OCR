namespace ocrApplication;

class Program
{
    static void Main()
    {
        // Define the folder path where images are stored (input folder)
        string folderPath = @"/Users/karthikprabu/Downloads/seLab/seProj/trainData/"; // Specify your folder path here

        // Define the main output folder for the OCR results
        string outputFolder = @"/Users/karthikprabu/Downloads/AA/";

        // Create a timestamp-based subfolder (e.g., "2025-02-01_12-30-00")
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string timestampFolder = Path.Combine(outputFolder, timestamp);

        // Define the subfolder name where Tesseract OCR results will be saved
        string ocrResultFolder = "TesseractOcrResult";

        // Combine timestamp folder and OCR result folder
        string fullOutputPath = Path.Combine(timestampFolder, ocrResultFolder);

        // Create the full directory structure (timestamp -> ocrresult) if it doesn't exist
        if (!Directory.Exists(fullOutputPath))
        {
            Directory.CreateDirectory(fullOutputPath);
            Console.WriteLine($"Created output folder structure: {fullOutputPath}");
        }

        // Create an instance of OcrExtractionTools
        OcrExtractionTools ocrTool = new OcrExtractionTools();

        // Get all image files in the folder and subfolders (e.g., .png, .jpg, .jpeg files)
        string[] imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                           file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        // Check if any images were found in the directory
        if (imageFiles.Length == 0)
        {
            Console.WriteLine("No image files found in the specified folder or subfolders.");
            return;
        }

        // Process each image file in the folder and subfolders
        foreach (var imagePath in imageFiles)
        {
            // Construct an output path for each image (inside the timestamp/ocrresult folder)
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
            string outputPath = Path.Combine(fullOutputPath, fileNameWithoutExtension);  // Combine the subfolder and image name

            // Perform OCR extraction using Tesseract
            ocrTool.ExtractTextUsingTesseract(imagePath, outputPath);
        }

        Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
    }
}