using System.Runtime.InteropServices;
using Emgu.CV;
using System.Diagnostics;

namespace ocrApplication
{
    /// <summary>
    /// Main program class that orchestrates the OCR processing workflow.
    /// Handles loading images, applying preprocessing techniques, performing OCR, and generating reports.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application. Processes all images in the specified folder
        /// using various preprocessing techniques and OCR methods.
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        static async Task Main()
        {
            // Detect the operating system to apply platform-specific OCR techniques
            // Darwin indicates macOS, used to determine which OCR commands to run
            bool isMacOs = RuntimeInformation.OSDescription.Contains("Darwin", StringComparison.OrdinalIgnoreCase);
            // Check if running on Windows for Windows-specific OCR methods
            bool isWindows = RuntimeInformation.OSDescription.Contains("Windows", StringComparison.OrdinalIgnoreCase);

            // Path to the configuration file containing API keys and paths for OCR tools
            // Edit this path to match your system configuration
            string configFilePath = @"/Users/karthikprabu/Documents/OCR/ocr_config.json";

            // Initialize OCR extraction tools with settings from the config file
            // This includes Tesseract paths, API keys for cloud services, etc.
            OcrExtractionTools ocrTool = new OcrExtractionTools(configFilePath);

            // Generate timestamp for creating unique output folder names
            // Prevents overwriting previous results when running multiple times
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Define input and output folder paths
            // Input folder contains the images to process
            string inputFolderPath = @"/Users/karthikprabu/Downloads/T2";
            // Output folder will contain all results organized by timestamp
            string outputFolderPath = @$"/Users/karthikprabu/Downloads/AA/{timestamp}";

            // Create subdirectories for organizing output files
            // Folder for storing processed (preprocessed) images
            string processedImagesFolder = Path.Combine(outputFolderPath, "processed_images");
            // Folder for storing OCR results for each image and preprocessing method
            string ocrResultsFolder = Path.Combine(outputFolderPath, "ocr_results");

            // Create the output directories if they don't already exist
            Directory.CreateDirectory(processedImagesFolder);
            Directory.CreateDirectory(ocrResultsFolder);

            // Get all image files in the input folder, including subdirectories
            // Filter to include only common image file formats: PNG, JPG, JPEG
            // Case-insensitive matching ensures we catch all variations of extensions
            string[] imageFiles = Directory.GetFiles(inputFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Verify that we found at least one image to process
            // If no images found, display message and exit
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("No image files found in the specified folder or subfolders.");
                return;
            }

            // Define all preprocessing methods to apply to each image
            // Each entry contains a name for the method and a function reference
            // The tuple format allows tracking both the method name and the actual function
            var preprocessMethods = new (string Name, Func<string, Mat> Method)[]
            {
                ("grayscale", ImagePreprocessing.ConvertToGrayscale),                   // Basic grayscale conversion - converts color image to grayscale
                ("gaussian", ImagePreprocessing.RemoveNoiseUsingGaussian),              // Gaussian blur for noise reduction - smooths the image using Gaussian filter
                ("median", ImagePreprocessing.RemoveNoiseUsingMedian),                  // Median blur for noise reduction - better at preserving edges than Gaussian
                ("adaptive_thresholding", ImagePreprocessing.AdaptiveThresholding),     // Adaptive thresholding - applies local thresholding for varying illumination
                ("gamma_correction", ImagePreprocessing.GammaCorrection),               // Gamma correction - adjusts image brightness non-linearly
                ("canny_edge", ImagePreprocessing.CannyEdgeDetection),                  // Canny edge detection - identifies edges in the image
                ("dilation", ImagePreprocessing.Dilation),                              // Dilation - expands white regions, useful for text enhancement
                ("erosion", ImagePreprocessing.Erosion),                                // Erosion - shrinks white regions, useful for removing small noise
                ("otsu_binarization", ImagePreprocessing.OtsuBinarization),             // Otsu's binarization - automatically determines optimal threshold
                ("deskew", ImagePreprocessing.Deskew),                                  // Deskew - corrects image rotation to align text horizontally
                ("HistogramEqualization", ImagePreprocessing.HistogramEqualization),    // Histogram equalization - improves contrast in the image
                ("SobelEdgeDetection", ImagePreprocessing.SobelEdgeDetection),          // Sobel edge detection - another method for detecting edges
                ("BilateralFilter", ImagePreprocessing.BilateralFilter),                // Bilateral filter - edge-preserving smoothing filter
                ("LaplacianEdgeDetection", ImagePreprocessing.LaplacianEdgeDetection),  // Laplacian edge detection - highlights regions of rapid intensity change
                ("NormalizeImage", ImagePreprocessing.NormalizeImage),                  // Normalize image - scales pixel values to use full dynamic range
                ("Opening", ImagePreprocessing.Opening),                                // Opening - erosion followed by dilation, removes small objects
                ("Closing", ImagePreprocessing.Closing),                                // Closing - dilation followed by erosion, fills small holes
                ("MorphologicalGradient", ImagePreprocessing.MorphologicalGradient),    // Morphological gradient - difference between dilation and erosion
                ("LogTransform", ImagePreprocessing.LogTransform),                      // Log transform - enhances details in darker regions
                ("ConvertToHSV", ImagePreprocessing.ConvertToHsv),                      // Convert to HSV color space - alternative color representation
                ("TopHat", ImagePreprocessing.TopHat),                                  // Top Hat transform - difference between original and opening
                ("BlackHat", ImagePreprocessing.BlackHat)                               // Black Hat transform - difference between closing and original
            };
            
            // Create a list of OCR step names for reporting and visualization
            // Starts with Original OCR (no preprocessing) then adds an entry for each preprocessing method
            // Adds "methodName" for each preprocessing method to track in reports
            var ocrSteps = new List<string> { "Original" };
            ocrSteps.AddRange(preprocessMethods.Select(m => m.Name));

            
            // Initialize the ensemble OCR system for combining results from different methods
            // This will use majority voting to determine the final OCR text
            var ensembleOcr = new EnsembleOcr();
           
            // Process each image in parallel to improve performance
            // Task.WhenAll awaits completion of all parallel tasks
            await Task.WhenAll(imageFiles.Select(async imagePath =>
            {
                // Extract the filename without extension for folder naming
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
                // Create image-specific folders for processed images and OCR results
                string imageProcessedFolder = Path.Combine(processedImagesFolder, fileNameWithoutExtension);
                string imageOcrResultFolder = Path.Combine(ocrResultsFolder, fileNameWithoutExtension);

                // Create the image-specific directories
                Directory.CreateDirectory(imageProcessedFolder);
                Directory.CreateDirectory(imageOcrResultFolder);

                // 1. Process the original image and save it to processed_images folder
                // Copy the original image to the processing folder for reference
                string originalImagePath = Path.Combine(imageProcessedFolder, "original.jpg");
                File.Copy(imagePath, originalImagePath, true); // true allows overwriting existing file

                // Define the Excel file path for this image's comparative analysis
                // Will contain execution times, memory usage, and similarity matrices
                string excelFilePath = Path.Combine(imageOcrResultFolder, $"Comparative_Analysis_{fileNameWithoutExtension}.xlsx");

                // Lists to store execution time and memory usage data for reporting
                // Tracks both preprocessing and OCR performance metrics
                var preprocessingTimes = new List<(string ImageName, string Method, double TimeTaken, long MemoryUsage)>();
                var ocrTimes = new List<(string ImageName, string OCRTool, double TimeTaken, long MemoryUsage)>();
                
                // List to store OCR results from all methods (original + preprocessed)
                // Will be used for ensemble OCR and similarity analysis
                var ocrResults = new List<string>();

                // Process the original image with OCR tools
                // Create a specific folder for original image OCR results
                string originalOcrToolFolder = Path.Combine(imageOcrResultFolder, "original");
                Directory.CreateDirectory(originalOcrToolFolder);
                
                // OCR extraction for original image with performance measurement
                Stopwatch ocrStopwatch = Stopwatch.StartNew();                                  // Start stopwatch to measure OCR processing time
                GC.Collect();                                                                   // Force garbage collection to get accurate memory measurements
                long memoryBeforeOcr = GC.GetTotalMemory(true);                  // Get memory usage before OCR processing
                OcrExtractionHelper.ProcessOcrForImage(originalImagePath, originalOcrToolFolder, ocrTool, isMacOs, isWindows); // Process the original image with OCR using the selected tools
                long memoryAfterOcr = GC.GetTotalMemory(true);                   // Get memory usage after OCR processing
                ocrStopwatch.Stop();                                                            // Stop the stopwatch to calculate elapsed time
                // Record OCR time and memory usage for the original image
                ocrTimes.Add((fileNameWithoutExtension, "Original OCR", ocrStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterOcr - memoryBeforeOcr)));

                // Collect OCR results for the original image
                // Reads the text from the OCR output file
                var originalOcrResults = OcrFileReader.ReadOcrResultsFromFiles(new List<string> { Path.Combine(imageOcrResultFolder, "original.txt") });
                ocrResults.AddRange(originalOcrResults);                                      // Add original OCR results to the collection of all results

                // Apply each preprocessing method and collect OCR results for each
                foreach (var (methodName, method) in preprocessMethods)
                {
                    Stopwatch preprocessStopwatch = Stopwatch.StartNew();                    // Start measuring preprocessing performance
                    GC.Collect();                                                            // Force garbage collection for accurate memory measurement
                    long memoryBeforePreprocess = GC.GetTotalMemory(true);    // Get memory usage before preprocessing
                    string preprocessedImagePath = Path.Combine(imageProcessedFolder, $"{methodName}.jpg"); // Define path for the preprocessed image output
                    var preprocessedImage = method(imagePath);                          // Apply the preprocessing technique to the original image
                    preprocessStopwatch.Stop();                                             // Stop timing the preprocessing
                    long memoryAfterPreprocess = GC.GetTotalMemory(true);    // Get memory usage after preprocessing
                    
                    // Record preprocessing time and memory usage
                    preprocessingTimes.Add((fileNameWithoutExtension, methodName, preprocessStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterPreprocess - memoryBeforePreprocess)));

                    // Only proceed if preprocessing produced a valid image
                    // Some methods might fail for certain images
                    if (!preprocessedImage.IsEmpty)
                    {
                        preprocessedImage.Save(preprocessedImagePath);                          // Save the preprocessed image for visual comparison
                    }

                    // 2. Call OCR tools and save the results for each preprocessing technique
                    // Create folder for this specific preprocessing method's OCR results
                    string ocrToolFolder = Path.Combine(imageOcrResultFolder, methodName);
                    
                    Stopwatch ocrPreprocessStopwatch = Stopwatch.StartNew();                   // OCR extraction for preprocessed image with performance measurement
                    long memoryBeforeOcrPreprocess = GC.GetTotalMemory(false);  // Get memory usage before OCR
                    OcrExtractionHelper.ProcessOcrForImage(preprocessedImagePath, ocrToolFolder, ocrTool, isMacOs, isWindows); // Process the preprocessed image with OCR
                    long memoryAfterOcrPreprocess = GC.GetTotalMemory(false);   // Get memory usage after OCR
                    ocrPreprocessStopwatch.Stop();                                             // Stop timing the OCR process
                    // Record OCR time and memory usage for this preprocessing method
                    ocrTimes.Add((fileNameWithoutExtension, $"{methodName} OCR", ocrPreprocessStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterOcrPreprocess - memoryBeforeOcrPreprocess)));

                    // Collect OCR results for this preprocessing method
                    var ocrResultsForMethod = OcrFileReader.ReadOcrResultsFromFiles(new List<string> { Path.Combine(imageOcrResultFolder, $"{methodName}.txt") });
                    ocrResults.AddRange(ocrResultsForMethod);       // Add to the collection of all OCR results
                }

                // Save execution times and memory usage data to an Excel file
                // Creates a report for comparing performance of different methods
                ExecutionTimeLogger.SaveExecutionTimesToExcel(excelFilePath, preprocessingTimes, ocrTimes);

                // Filter out empty OCR results to improve ensemble accuracy
                // Empty or whitespace-only results could skew the voting
                var ocrResultForGroundTruth = ocrResults.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
                
                // Save all OCR results to a file before sending to ensemble
                // Useful for debugging and manual inspection
                string allOcrResultsFilePath = Path.Combine(imageOcrResultFolder, "all_ocr_results.txt");
                await File.WriteAllLinesAsync(allOcrResultsFilePath, ocrResults);
                
                // 3. Perform Ensemble OCR after all preprocessing is done
                // Combine all OCR results using majority voting algorithm
                string groundTruth = ensembleOcr.CombineUsingMajorityVoting(ocrResultForGroundTruth);

                // 4. Save the final result to a file
                // This is the best OCR result after combining all preprocessing methods
                string groundTruthFilePath = Path.Combine(imageOcrResultFolder, "final_ocr_result.txt");
                await File.WriteAllTextAsync(groundTruthFilePath, groundTruth);
                
                // Create a similarity matrix generator for comparing OCR results
                // Used to visualize how similar results are from different methods
                TextSimilarity.SimilarityMatrixGenerator similarityMatrixGenerator = new TextSimilarity.SimilarityMatrixGenerator();

                // Generate and visualize OCR similarity matrix with heatmap
                // Creates cosine similarity matrix to show text similarity between methods
                await similarityMatrixGenerator.GenerateAndVisualizeOcrSimilarityMatrix(ocrResults, groundTruth, excelFilePath, ocrSteps);
                // Creates Levenshtein distance-based similarity matrix (edit distance)
                await similarityMatrixGenerator.GenerateAndVisualizeOcrSimilarityMatrixLv(ocrResults, groundTruth, excelFilePath, ocrSteps);
                
                // Generate preprocessing effectiveness report
                // Shows which preprocessing methods performed best
                await similarityMatrixGenerator.GeneratePreprocessingEffectivenessReport(ocrResults, groundTruth, excelFilePath, ocrSteps);
                  
                // Log completion message for this image
                Console.WriteLine($"OCR processing complete for image: {imagePath}");
                Console.WriteLine($"-----------------------------------------------------------");
              
                // Generate text embeddings and create visualization
                // Creates vector representations of text for comparison and visualization
                var embeddings = similarityMatrixGenerator.GenerateTextEmbeddings(ocrResults, ocrSteps);
                // Creates visual representation of text embeddings in Excel
                ExecutionTimeLogger.CreateEmbeddingVisualization(excelFilePath, embeddings, ocrSteps);
                
            }));

            // Log completion message for all images
            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
        }
    }
}
