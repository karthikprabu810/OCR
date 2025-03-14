using System.Runtime.InteropServices;
using Emgu.CV;
using System.Diagnostics;
using ShellProgressBar;

namespace ocrApplication
{
    /// <summary>
    /// Main program class that orchestrates the OCR processing workflow.
    /// Handles loading images, applying preprocessing techniques, performing OCR, and generating reports.
    /// </summary>
    public static class Program
    {
        private static float _progressPercentage;
        private static int _imagesProcessed;
        
        /// <summary>
        /// Main entry point for the application. Processes all images in the specified folder
        /// using various preprocessing techniques and OCR methods.
        /// </summary>
        /// <returns>Task representing the asynchronous operation</returns>
        static async Task Main()
        {
            // Detect the operating system for platform-specific OCR implementations
            bool isMacOs = RuntimeInformation.OSDescription.Contains("Darwin", StringComparison.OrdinalIgnoreCase);
            bool isWindows = RuntimeInformation.OSDescription.Contains("Windows", StringComparison.OrdinalIgnoreCase);

            // Configuration file with API keys and path settings for OCR services
            string configFilePath = @"/Users/karthikprabu/Documents/OCR/ocrApplication/ocrApplication/ocr_config.json";

            // Initialize OCR tools with configuration settings
            OcrExtractionTools ocrTool = new OcrExtractionTools(configFilePath);

            // Create timestamp-based output folders to prevent overwriting previous results
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Define input and output directories/Users/karthikprabu/Downloads/Output
            
            string? inputFolderPath ;
            do
            {
                Console.Write("Enter the input folder path: ");
                inputFolderPath = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(inputFolderPath)); // Keep asking until the input is not null, empty, or whitespace

            string? outputFolderPath ;
            do
            {
                Console.Write("Enter the output folder path: ");
                outputFolderPath = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(outputFolderPath)); // Keep asking until the input is not null, empty, or whitespace

            outputFolderPath = Path.Combine(outputFolderPath, timestamp);
            
            // string inputFolderPath = @"/Users/karthikprabu/Documents/OCR/ocrApplication/Input";
            // string outputFolderPath = @$"/Users/karthikprabu/Downloads/AA/{timestamp}";

            // Create directory structure for storing results
            string processedImagesFolder = Path.Combine(outputFolderPath, "processed_images");
            string ocrResultsFolder = Path.Combine(outputFolderPath, "ocr_results");

            // Create output directories
            Directory.CreateDirectory(processedImagesFolder);
            Directory.CreateDirectory(ocrResultsFolder);

            // Find all image files in the input directory and subdirectories
            string[] imageFiles = Directory.GetFiles(inputFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Verify images were found before proceeding
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("No image files found in the specified folder or subfolders.");
                return;
            }

            // Define all available preprocessing methods
            var allPreprocessMethods = new (string Name, Func<string, Mat> Method)[]
            {
                ("Grayscale", ImagePreprocessing.ConvertToGrayscale),                   // Basic grayscale conversion
                ("Gaussian_Filter", ImagePreprocessing.RemoveNoiseUsingGaussian),              // Gaussian blur for noise reduction
                ("Median_Filter", ImagePreprocessing.RemoveNoiseUsingMedian),                  // Median blur for noise reduction
                ("Adaptive_Thresholding", ImagePreprocessing.AdaptiveThresholding),     // Adaptive thresholding for varying lighting
                ("Gamma_Correction", ImagePreprocessing.GammaCorrection),               // Gamma correction for brightness adjustment
                ("Canny_Edge", ImagePreprocessing.CannyEdgeDetection),                  // Edge detection
                ("Dilation", ImagePreprocessing.Dilation),                              // Expands white regions (text enhancement)
                ("Erosion", ImagePreprocessing.Erosion),                                // Shrinks white regions (noise removal)
                ("Otsu_Binarization", ImagePreprocessing.OtsuBinarization),             // Automatic threshold detection
                ("Deskew", ImagePreprocessing.Deskew),                                  // Rotation correction
                ("Histogram_Equalization", ImagePreprocessing.HistogramEqualization),   // Contrast improvement
                ("Sobel_Edge_Detection", ImagePreprocessing.SobelEdgeDetection),        // Edge detection (gradient-based)
                ("BilateralFilter", ImagePreprocessing.BilateralFilter),                // Edge-preserving smoothing
                ("Laplacian_Edge_Detection", ImagePreprocessing.LaplacianEdgeDetection),// Edge detection (second derivative)
                ("Normalize_Image", ImagePreprocessing.NormalizeImage),                 // Normalize pixel value range
                ("Morphological_Opening", ImagePreprocessing.Opening),                  // Erosion followed by dilation
                ("Morphological_Closing", ImagePreprocessing.Closing),                  // Dilation followed by erosion
                ("Morphological_Gradient", ImagePreprocessing.MorphologicalGradient),   // Difference between dilation and erosion
                ("LogTransform", ImagePreprocessing.LogTransform),                      // Enhance details in dark regions
                ("ConvertToHSV", ImagePreprocessing.ConvertToHsv),                      // Alternative color representation
                ("TopHat", ImagePreprocessing.TopHat),                                  // Difference between original and opening
                ("BlackHat", ImagePreprocessing.BlackHat)                               // Difference between closing and original
            };

            // Prompt user to select preprocessing methods
            Console.WriteLine("\nAvailable preprocessing techniques:");
            Console.WriteLine("-------------------------------------");
            for (int i = 0; i < allPreprocessMethods.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {allPreprocessMethods[i].Name}");
            }
            
            Console.WriteLine("\nEnter the numbers of preprocessing techniques you want to use (comma-separated, e.g., 1,3,5):");
            Console.WriteLine("Enter 'all' to use all techniques, or '0' to skip preprocessing and only use the original image:");
            
            string? userInput = Console.ReadLine();
            List<(string Name, Func<string, Mat> Method)> preprocessMethods = new List<(string Name, Func<string, Mat> Method)>();
            
            if (userInput?.Trim().ToLower() == "all")
            {
                // Use all preprocessing methods
                preprocessMethods = allPreprocessMethods.ToList();
                Console.WriteLine("\nAll preprocessing techniques will be applied.");
                Console.WriteLine("\nInitiating preprocessing techniques and extraction...");
            }
            else if (userInput?.Trim() == "0")
            {
                // No preprocessing, just use original images
                Console.WriteLine("\nNo preprocessing will be applied, only the original images will be processed.");
                Console.WriteLine("\nInitiating extraction...");
            }
            else
            {
                // Parse user input for selected methods
                var selectedIndices = userInput?.Split(',')
                    .Select(index => index.Trim())
                    .Where(index => int.TryParse(index, out _))
                    .Select(index => int.Parse(index) - 1) // Convert to 0-based index
                    .Where(index => index >= 0 && index < allPreprocessMethods.Length)
                    .ToList();
                
                if (selectedIndices.Count == 0)
                {
                    Console.WriteLine("\nNo valid preprocessing techniques selected. Only the original images will be processed.");
                    Console.WriteLine("\nInitiating extraction...");
                }
                else
                {
                    foreach (var index in selectedIndices)
                    {
                        preprocessMethods.Add(allPreprocessMethods[index]);
                    }
                    
                    Console.WriteLine("\nSelected preprocessing techniques:");
                    foreach (var method in preprocessMethods)
                    {
                        Console.WriteLine($"- {method.Name}");
                    }
                    Console.WriteLine("\nInitiating preprocessing techniques and extraction...");
                }
            }
            
            // Create list of method names for result reporting
            var ocrSteps = new List<string> { "Original" };
            ocrSteps.AddRange(preprocessMethods.Select(m => m.Name));

            // Initialize ensemble OCR system for combining results using majority voting
            var ensembleOcr = new EnsembleOcr();
           
            // Dictionary to store extracted text for each image (thread-safe)
            var extractedTexts = new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
            
            // Initialize progress bar
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            };
            using (var pbar = new ProgressBar(imageFiles.Length, "Processing Images", options))
            {
                var tasks = imageFiles.Select(async (imagePath, index) =>
                {
                    // Create image-specific output directories
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
                    var localPbar = pbar; // Use a local variable to avoid capturing the disposed progress bar
                    localPbar.Message = $"Processing image: {fileNameWithoutExtension} ({index + 1}/{imageFiles.Length})";
                    
                    string imageProcessedFolder = Path.Combine(processedImagesFolder, fileNameWithoutExtension);
                    string imageOcrResultFolder = Path.Combine(ocrResultsFolder, fileNameWithoutExtension);

                    Directory.CreateDirectory(imageProcessedFolder);
                    Directory.CreateDirectory(imageOcrResultFolder);

                    // Copy original image to processed folder for reference
                    string originalImagePath = Path.Combine(imageProcessedFolder, "original.jpg");
                    File.Copy(imagePath, originalImagePath, true);

                    // Define Excel file for storing comparative analysis results
                    string excelFilePath = Path.Combine(imageOcrResultFolder, $"Comparative_Analysis_{fileNameWithoutExtension}.xlsx");

                    // Initialize performance tracking metrics
                    var preprocessingTimes = new List<(string ImageName, string Method, double TimeTaken, long MemoryUsage)>();
                    var ocrTimes = new List<(string ImageName, string OCRTool, double TimeTaken, long MemoryUsage)>();
                    
                    // Collection to store all OCR results for comparison
                    var ocrResults = new List<string>();

                    // Process original image with OCR
                    string originalOcrToolFolder = Path.Combine(imageOcrResultFolder, "original");
                    Directory.CreateDirectory(originalOcrToolFolder);
                    
                    // Measure performance metrics for original image OCR
                    Stopwatch ocrStopwatch = Stopwatch.StartNew();
                    GC.Collect();
                    long memoryBeforeOcr = GC.GetTotalMemory(true);
                    OcrExtractionHelper.ProcessOcrForImage(originalImagePath, originalOcrToolFolder, ocrTool, isMacOs, isWindows);
                    long memoryAfterOcr = GC.GetTotalMemory(true);
                    ocrStopwatch.Stop();
                    ocrTimes.Add((fileNameWithoutExtension, "Original OCR", ocrStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterOcr - memoryBeforeOcr)));

                    // Read the OCR results for the original image
                    var originalOcrResults = OcrFileReader.ReadOcrResultsFromFiles(new List<string> { Path.Combine(imageOcrResultFolder, "original.txt") });
                    ocrResults.AddRange(originalOcrResults);

                    // Apply each selected preprocessing method and perform OCR
                    foreach (var (methodName, method) in preprocessMethods)
                    {
                        // Measure preprocessing performance
                        Stopwatch preprocessStopwatch = Stopwatch.StartNew();
                        GC.Collect();
                        long memoryBeforePreprocess = GC.GetTotalMemory(true);
                        string preprocessedImagePath = Path.Combine(imageProcessedFolder, $"{methodName}.jpg");
                        var preprocessedImage = method(imagePath);
                        preprocessStopwatch.Stop();
                        long memoryAfterPreprocess = GC.GetTotalMemory(true);
                        
                        // Record preprocessing metrics
                        preprocessingTimes.Add((fileNameWithoutExtension, methodName, preprocessStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterPreprocess - memoryBeforePreprocess)));

                        // Save preprocessed image if valid
                        if (!preprocessedImage.IsEmpty)
                        {
                            preprocessedImage.Save(preprocessedImagePath);
                        }
                        
                        // Perform OCR on the preprocessed image
                        string ocrToolFolder = Path.Combine(imageOcrResultFolder, methodName);
                        
                        // Measure OCR performance for this preprocessing method
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

                    // Save performance metrics to Excel for analysis
                    ExecutionTimeLogger.SaveExecutionTimesToExcel(excelFilePath, preprocessingTimes, ocrTimes);

                    // Filter out empty OCR results to improve ensemble accuracy
                    var ocrResultForGroundTruth = ocrResults.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
                    
                    // Save all raw OCR results for reference
                    string allOcrResultsFilePath = Path.Combine(imageOcrResultFolder, "all_ocr_results.txt");
                    await File.WriteAllLinesAsync(allOcrResultsFilePath, ocrResults);
                    
                    // Generate ensemble OCR result using majority voting
                    string groundTruth = await ensembleOcr.SendOcrTextsToApiAsync(ocrResultForGroundTruth);
                    _ = groundTruth;
                    groundTruth = ensembleOcr.CombineUsingMajorityVoting(ocrResultForGroundTruth);
                    
                    // Save the final ensemble OCR result
                    string groundTruthFilePath = Path.Combine(imageOcrResultFolder, "final_ocr_result.txt");
                    await File.WriteAllTextAsync(groundTruthFilePath, groundTruth);
                    
                    // Generate similarity matrices to compare OCR results
                    TextSimilarity.SimilarityMatrixGenerator similarityMatrixGenerator = new TextSimilarity.SimilarityMatrixGenerator();

                    // Create cosine similarity matrix visualization
                    await similarityMatrixGenerator.GenerateAndVisualizeOcrSimilarityMatrix(ocrResults, groundTruth, excelFilePath, ocrSteps);
                    // Create Levenshtein distance similarity matrix visualization
                    await similarityMatrixGenerator.GenerateAndVisualizeOcrSimilarityMatrixLv(ocrResults, groundTruth, excelFilePath, ocrSteps);
                    
                    // Generate report on which preprocessing methods were most effective
                    await similarityMatrixGenerator.GeneratePreprocessingEffectivenessReport(ocrResults, groundTruth, excelFilePath, ocrSteps);
                      
                    // Generate and visualize text embeddings for comparing OCR results
                    var embeddings = similarityMatrixGenerator.GenerateTextEmbeddings(ocrResults, ocrSteps);
                    ExecutionTimeLogger.CreateEmbeddingVisualization(excelFilePath, embeddings, ocrSteps);
                
                    // Store the extracted text instead of printing it now
                    extractedTexts[imagePath] = groundTruth;
                    
                    // Calculate and update progress percentage
                    _imagesProcessed = _imagesProcessed+ 1;
                    _progressPercentage = (_imagesProcessed) * 100f / imageFiles.Length;
                    
                    // Send progress percentage to stdout in a special format that GUI will recognize
                    Console.WriteLine($"##PROGRESS:{_progressPercentage}");
                    Console.WriteLine($"##PROCESSED:{_imagesProcessed}");
                    
                    // Update progress bar with completed message
                    localPbar.Message = $"Completed: {fileNameWithoutExtension} ({index + 1}/{imageFiles.Length})";
                    
                    // Update progress bar
                    localPbar.Tick();
                });

                await Task.WhenAll(tasks);
            }
            
            // Log completion for all images
            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
            
            
            // Display all extracted texts together
            Console.WriteLine("\n==================================================");
            Console.WriteLine("SUMMARY OF EXTRACTED TEXT FROM ALL IMAGES");
            Console.WriteLine("==================================================");
            foreach (var entry in extractedTexts)
            {
                Console.WriteLine($"\nImage: {Path.GetFileName(entry.Key)}");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"Extracted text: {entry.Value}");
                Console.WriteLine("--------------------------------------------------");
            }
            
            // Call ExportResults to allow user to select export type
            ExportUtilities.ExportResults(outputFolderPath + "/OCR_Results", extractedTexts);

            Console.WriteLine("\nProcessing complete.");
        }
    }
}
