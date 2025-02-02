namespace ocrApplication
{
    public class Program
    {
        static async Task Main()
        {
            // Define the folder path where images are stored (input folder)
            string folderPath = @"/Users/karthikprabu/Downloads/T2"; // Specify your folder path here

            // Define the main output folder for the OCR results
            string outputFolder = @"/Users/karthikprabu/Downloads/AA/";

            // Create a timestamp-based subfolder (e.g., "2025-02-01_12-30-00")
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string timestampFolder = Path.Combine(outputFolder, timestamp);

            // Define the subfolder name where OCR results will be saved
            string ocrResultFolder = "OcrResults";

            // Combine timestamp folder and OCR result folder
            string fullOutputPath = Path.Combine(timestampFolder, ocrResultFolder);

            // Create the full directory structure (timestamp -> ocrresult) if it doesn't exist
            if (!Directory.Exists(fullOutputPath))
            {
                Directory.CreateDirectory(fullOutputPath);
                Console.WriteLine($"Created output folder structure: {fullOutputPath}");
            }

            // Specify the config file path
            string configFilePath = @"/Users/karthikprabu/Documents/OCR/ocr_config.json";  // Specify the full path to the config file here.

            // Create an instance of the OcrExtractionTools with configuration values
            OcrExtractionTools ocrTool = new OcrExtractionTools(configFilePath);

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
                string imageOutputFolder = Path.Combine(fullOutputPath, fileNameWithoutExtension);  // Combine the subfolder and image name

                // Create the image-specific output folder if it doesn't exist
                if (!Directory.Exists(imageOutputFolder))
                {
                    Directory.CreateDirectory(imageOutputFolder);
                }

                // --- Tesseract OCR ---
                string tesseractOutputPath = Path.Combine(imageOutputFolder, "tesseract_output");
                ocrTool.ExtractTextUsingTesseract(imagePath, tesseractOutputPath);
                Console.WriteLine($"Tesseract OCR processed: {imagePath}");

                // --- IronOCR OCR ---
                string ironOcrOutputPath = Path.Combine(imageOutputFolder, "ironocr_output.txt");
                string ironOcrText = ocrTool.ExtractTextUsingIronOcr(imagePath);
                File.WriteAllText(ironOcrOutputPath, ironOcrText);
                Console.WriteLine($"IronOCR processed: {imagePath}");

                
                // --- Google Vision OCR ---
                string googleVisionOutputPath = Path.Combine(imageOutputFolder, "googlevision_output.txt");
                string googleVisionText = await ocrTool.ExtractTextUsingGoogleVisionAsync(imagePath);
                File.WriteAllText(googleVisionOutputPath, googleVisionText);
                Console.WriteLine($"Google Vision OCR processed: {imagePath}");

                // --- OCR.Space API ---
                string ocrSpaceOutputPath = Path.Combine(imageOutputFolder, "ocrspace_output.txt");
                string ocrSpaceText = await ocrTool.ExtractTextUsingOCRSpaceAsync(imagePath); // Call instance method
                File.WriteAllText(ocrSpaceOutputPath, ocrSpaceText);
                Console.WriteLine($"OCR.Space processed: {imagePath}"); 
                
            }

            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
        }
    }
}