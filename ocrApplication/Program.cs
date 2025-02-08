﻿using System.Runtime.InteropServices;
using Emgu.CV;

namespace ocrApplication
{
    public static class Program
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
            await Task.WhenAll(imageFiles.Select(imagePath =>
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
                OcrExtractionHelper.ProcessOcrForImage(originalImagePath, originalOcrToolFolder, ocrTool, isMacOs, isWindows);

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
                    OcrExtractionHelper.ProcessOcrForImage(preprocessedImagePath, ocrToolFolder, ocrTool, isMacOs, isWindows);
                }

                return Task.CompletedTask;
            }));

            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
        }
    }
}
