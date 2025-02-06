using System.Runtime.InteropServices;
using Emgu.CV;

namespace ocrApplication
{
    public class Program
    {
        static async Task Main()
        {
            bool isMacOs = RuntimeInformation.OSDescription.Contains("Darwin", StringComparison.OrdinalIgnoreCase);
            bool isWindows = RuntimeInformation.OSDescription.Contains("Windows", StringComparison.OrdinalIgnoreCase);
            
            // Specify the config file path
            string configFilePath = @"/Users/karthikprabu/Documents/OCR/ocr_config.json";  // Specify the full path to the config file here.

            // Create an instance of the OcrExtractionTools with configuration values
            OcrExtractionTools ocrTool = new OcrExtractionTools(configFilePath);
            
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            
            // Define the folder paths
            string inputFolderPath = @"/Users/karthikprabu/Downloads/T2";  // Original images folder
            string outputFolderPath = @$"/Users/karthikprabu/Downloads/AA/{timestamp}";  // Output folder
            
            // Folder for processed images and OCR results
            string processedImagesFolder = Path.Combine(outputFolderPath, "processed_images");
            string ocrResultsFolder = Path.Combine(outputFolderPath, "ocr_results");
            
            // Create necessary folders if they don't exist
            Directory.CreateDirectory(processedImagesFolder);
            Directory.CreateDirectory(ocrResultsFolder);

            // Get all image files in the input folder
            string[] imageFiles = Directory.GetFiles(inputFolderPath, "*.*", SearchOption.AllDirectories)
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

            // Define preprocessing methods and their names
            var preprocessMethods = new (string Name, Func<string, Mat> Method)[]
            {
                ("grayscale", ImagePreprocessing.ConvertToGrayscale),
                ("gaussian", ImagePreprocessing.RemoveNoiseUsingGaussian),
                ("median", ImagePreprocessing.RemoveNoiseUsingMedian),
                ("adaptive_thresholding", ImagePreprocessing.AdaptiveThresholding),
                ("gamma_correction", ImagePreprocessing.GammaCorrection),
                ("canny_edge", ImagePreprocessing.CannyEdgeDetection),
                ("dilation", ImagePreprocessing.Dilation),
                ("erosion", ImagePreprocessing.Erosion),
                ("otsu_binarization", ImagePreprocessing.OtsuBinarization),
                // ("deskew", ImagePreprocessing.Deskew)
            };
            
            // Process each image in parallel
            await Task.WhenAll(imageFiles.Select(async imagePath =>
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
                string imageProcessedFolder = Path.Combine(processedImagesFolder, fileNameWithoutExtension);
                string imageOcrResultFolder = Path.Combine(ocrResultsFolder, fileNameWithoutExtension);

                Directory.CreateDirectory(imageProcessedFolder);
                Directory.CreateDirectory(imageOcrResultFolder);

                // 1. Process the original image and save it to processed_images folder
                string originalImagePath = Path.Combine(imageProcessedFolder, "original.jpg");
                File.Copy(imagePath, originalImagePath, true); // Copy original image

                // Process the original image with OCR tools
                string originalOcrToolFolder = Path.Combine(imageOcrResultFolder, "original");
                Directory.CreateDirectory(originalOcrToolFolder);

                // OCR extraction for original image
                ProcessOcrForImage(originalImagePath, originalOcrToolFolder, ocrTool, isMacOs, isWindows);

                // 2. Apply preprocessing techniques and save each preprocessed image
                foreach (var (methodName, method) in preprocessMethods)
                {
                    string preprocessedImagePath = Path.Combine(imageProcessedFolder, $"{methodName}.jpg");
                    var preprocessedImage = method(imagePath); // Apply preprocessing technique
                    if (!preprocessedImage.IsEmpty)
                    {
                        preprocessedImage.Save(preprocessedImagePath); // Save the preprocessed image
                    }

                    // 3. Call OCR tools and save the results for each preprocessing technique
                    string ocrToolFolder = Path.Combine(imageOcrResultFolder, methodName);
                    Directory.CreateDirectory(ocrToolFolder);

                    // OCR extraction for preprocessed image
                    ProcessOcrForImage(preprocessedImagePath, ocrToolFolder, ocrTool, isMacOs, isWindows);
                }

            }));

            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
        }
        
        // Helper method for processing OCR for both original and preprocessed images
        private static void ProcessOcrForImage(string imagePath, string ocrToolFolder, OcrExtractionTools ocrTool, bool isMacOs, bool isWindows)
        {
            // --- Tesseract OCR ---
            if (isMacOs)
            {
                ocrTool.ExtractTextUsingTesseract(imagePath, ocrToolFolder);
                Console.WriteLine(ocrToolFolder);
                File.Move(Path.Combine(Directory.GetParent(ocrToolFolder).FullName, Path.GetFileName(ocrToolFolder) + ".txt"), 
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
            File.WriteAllText(Path.Combine(ocrToolFolder, "ironocr.txt"), ironOcrText);
            Console.WriteLine($"IronOCR processed: {imagePath}");

            /*
            // --- Google Vision OCR ---
            string googleVisionOcrText = ocrTool.ExtractTextUsingGoogleVisionAsync(imagePath).Result;
            File.WriteAllText(Path.Combine(ocrToolFolder, "googlevision.txt"), googleVisionOcrText);
            Console.WriteLine($"Google Vision OCR processed: {imagePath}");

            // --- OCR.Space API ---
            string ocrSpaceOcrText = ocrTool.ExtractTextUsingOCRSpaceAsync(imagePath).Result;
            File.WriteAllText(Path.Combine(ocrToolFolder, "ocrspace.txt"), ocrSpaceOcrText);
            Console.WriteLine($"OCR.Space processed: {imagePath}");
            */
        }
    }
}



/*// Process each image file 
   foreach (var imagePath in imageFiles)
   {
       // Extract the file name (without extension) and create subfolders for processed images and OCR results
       string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
       string imageProcessedFolder = Path.Combine(processedImagesFolder, fileNameWithoutExtension);
       string imageOcrResultFolder = Path.Combine(ocrResultsFolder, fileNameWithoutExtension);

       // Create subfolders for processed images and OCR results
       Directory.CreateDirectory(imageProcessedFolder);
       Directory.CreateDirectory(imageOcrResultFolder);

       // 1. Process the original image and save it to processed_images folder
       string originalImagePath = Path.Combine(imageProcessedFolder, "original.jpg");
       File.Copy(imagePath, originalImagePath, true); // Copy original image
       
       // Process the original image with OCR tools
       string originalOcrToolFolder = Path.Combine(imageOcrResultFolder, "original");
       Directory.CreateDirectory(originalOcrToolFolder); // Folder for OCR results for the original image

       // OCR extraction for original image (same as for preprocessed images)
       ProcessOcrForImage(originalImagePath, originalOcrToolFolder, ocrTool, isMacOs, isWindows);

       // 2. Apply preprocessing techniques and save each preprocessed image
       foreach (var (methodName, method) in preprocessMethods)
       {
           string preprocessedImagePath = Path.Combine(imageProcessedFolder, $"{methodName}.jpg");
           var preprocessedImage = method(imagePath);  // Apply preprocessing technique
           if (!preprocessedImage.IsEmpty)
           {
               preprocessedImage.Save(preprocessedImagePath);  // Save the preprocessed image
           }

           // 3. Call OCR tools and save the results for each preprocessing technique
           string ocrToolFolder = Path.Combine(imageOcrResultFolder, methodName); // Folder for OCR tool results per method
           Directory.CreateDirectory(ocrToolFolder); // Create the folder for the method
           
           // OCR extraction for preprocessed image
           ProcessOcrForImage(preprocessedImagePath, ocrToolFolder, ocrTool, isMacOs, isWindows);

           
       }*/