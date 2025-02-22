using System.Runtime.InteropServices;
using Emgu.CV;
using System.Diagnostics;

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
                ("deskew", ImagePreprocessing.Deskew)
            };

            // Initialize the OCR comparison and ensemble methods
            var ensembleOcr = new EnsembleOcr();
            // var ensembleOCRWithConfidence = new EnsembleOcrWithConfidence();
            var ocrComparison = new OcrComparison();
            
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

                // Excel file path
                string excelFilePath = Path.Combine(imageOcrResultFolder, $"Comparitative_Analysis_{fileNameWithoutExtension}.xlsx");

                // List to store execution time and memory usage data
                var preprocessingTimes = new List<(string ImageName, string Method, double TimeTaken, long MemoryUsage)>();
                var ocrTimes = new List<(string ImageName, string OCRTool, double TimeTaken, long MemoryUsage)>();
                
                // List to store OCR results (original + preprocessed)
                var ocrResults = new List<string>();

                // Process the original image with OCR tools
                string originalOcrToolFolder = Path.Combine(imageOcrResultFolder, "original");
                // Directory.CreateDirectory(originalOcrToolFolder);
                
                // OCR extraction for original image
                Stopwatch ocrStopwatch = Stopwatch.StartNew();
                GC.Collect();  // Force a garbage collection before measuring memory
                long memoryBeforeOcr = GC.GetTotalMemory(true);  // Force garbage collection
                OcrExtractionHelper.ProcessOcrForImage(originalImagePath, originalOcrToolFolder, ocrTool, isMacOs, isWindows);
                long memoryAfterOcr = GC.GetTotalMemory(true);  // Force garbage collection
                ocrStopwatch.Stop();
                ocrTimes.Add((fileNameWithoutExtension, "Original OCR", ocrStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterOcr - memoryBeforeOcr)));

                // Collect OCR results for the original image
                var originalOcrResults = OcrFileReader.ReadOcrResultsFromFiles(new List<string> { Path.Combine(imageOcrResultFolder, "original.txt") });
                ocrResults.AddRange(originalOcrResults);

                // Apply preprocessing methods and collect OCR results for each
                foreach (var (methodName, method) in preprocessMethods)
                {
                    Stopwatch preprocessStopwatch = Stopwatch.StartNew();
                    GC.Collect();  // Force a garbage collection before measuring memory
                    long memoryBeforePreprocess = GC.GetTotalMemory(true);  // Force garbage collection
                    string preprocessedImagePath = Path.Combine(imageProcessedFolder, $"{methodName}.jpg");
                    var preprocessedImage = method(imagePath); // Apply preprocessing technique
                    preprocessStopwatch.Stop();
                    long memoryAfterPreprocess = GC.GetTotalMemory(true);  // Force garbage collection
                    
                    preprocessingTimes.Add((fileNameWithoutExtension, methodName, preprocessStopwatch.Elapsed.TotalMilliseconds, memoryAfterPreprocess - memoryBeforePreprocess));

                    if (!preprocessedImage.IsEmpty)
                    {
                        preprocessedImage.Save(preprocessedImagePath); // Save the preprocessed image
                    }

                    // 2. Call OCR tools and save the results for each preprocessing technique
                    string ocrToolFolder = Path.Combine(imageOcrResultFolder, methodName);
                    // Directory.CreateDirectory(ocrToolFolder);

                    // OCR extraction for preprocessed image
                    Stopwatch ocrPreprocessStopwatch = Stopwatch.StartNew();
                    long memoryBeforeOcrPreprocess = GC.GetTotalMemory(false);
                    OcrExtractionHelper.ProcessOcrForImage(preprocessedImagePath, ocrToolFolder, ocrTool, isMacOs, isWindows);
                    long memoryAfterOcrPreprocess = GC.GetTotalMemory(false);
                    ocrPreprocessStopwatch.Stop();
                    ocrTimes.Add((fileNameWithoutExtension, $"{methodName} OCR", ocrPreprocessStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterOcrPreprocess - memoryBeforeOcrPreprocess)));

                    // Collect OCR results for this method
                    var ocrResultsForMethod = OcrFileReader.ReadOcrResultsFromFiles(new List<string> { Path.Combine(imageOcrResultFolder, $"{methodName}.txt") });
                    ocrResults.AddRange(ocrResultsForMethod);
                }

                // Save execution times to an Excel file
                ExecutionTimeLogger.SaveExecutionTimesToExcel(excelFilePath, preprocessingTimes, ocrTimes);
               
                // Save all OCR results to a file before sending to ensemble
                string allOcrResultsFilePath = Path.Combine(imageOcrResultFolder, "all_ocr_results.txt");
                File.WriteAllLines(allOcrResultsFilePath, ocrResults);
                
                // 3. Perform Ensemble OCR after all preprocessing is done
                string groundTruth = ensembleOcr.CombineUsingMajorityVoting(ocrResults);

                // 4. Save the final result to a file
                string groundTruthFilePath = Path.Combine(imageOcrResultFolder, "final_ocr_result.txt");
                File.WriteAllText(groundTruthFilePath, groundTruth);

                // Optionally compare OCR result with ground truth (if available)
                // Get both similarity results as strings
                var (levenshteinResult, cosineResult) = ocrComparison.CompareOcrResults(ocrResults, groundTruth); 
                /*
                Console.WriteLine("Levenshtein Similarity Results:\n");
                Console.WriteLine(levenshteinResult);
                Console.WriteLine("\nCosine Similarity Results:\n");
                Console.WriteLine(cosineResult);
                */
                ExecutionTimeLogger.ComparisionPlot(excelFilePath, levenshteinResult, cosineResult);
                Console.WriteLine($"OCR processing complete for image: {imagePath}");
            }));

            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
        }
    }
}
