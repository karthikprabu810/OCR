using Emgu.CV;
using ShellProgressBar;
using System.Diagnostics;

namespace ocrApplication
{
    /// <summary>
    /// Handles the execution of OCR processing for images, including preprocessing, OCR extraction,
    /// similarity measurement, and result analysis. This class encapsulates the complete OCR workflow
    /// for processing multiple images with various preprocessing methods.
    /// </summary>
    public class OcrProcessor
    {
        private float _progressPercentage;               // Tracks the current progress percentage of image processing
        private int _imagesProcessed;                    // Counts the number of images that have been processed
        private readonly bool _isMacOs;                  // Indicates whether the application is running on macOS platform
        private readonly bool _isWindows;                // Indicates whether the application is running on Windows platform
        private readonly OcrExtractionTools _ocrTool;    // The OCR extraction tool used for text recognition
        private readonly EnsembleOcr _ensembleOcr;       // Provides ensemble methods for combining multiple OCR results
        private readonly ProgressBar _progressBar;       // Progress bar for displaying processing status in the console
        
        // Dictionary to store extracted text for each image (thread-safe)
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _extractedTexts = 
            new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
            
        // Dictionary to track best preprocessing method by cosine similarity for each image (thread-safe)
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _bestPreprocessingMethods = 
            new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
            
        // Dictionary to track best preprocessing method by Levenshtein distance for each image (thread-safe)
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, string> _bestLevenshteinMethods = 
            new System.Collections.Concurrent.ConcurrentDictionary<string, string>();
            
        /// <summary>
        /// Initializes a new instance of the OcrProcessor class.
        /// </summary>
        /// <param name="ocrTool">The OCR extraction tool to use for processing.</param>
        /// <param name="isMacOs">Flag indicating if running on macOS.</param>
        /// <param name="isWindows">Flag indicating if running on Windows.</param>
        /// <param name="progressBar">Progress bar for displaying processing status.</param>
        public OcrProcessor(OcrExtractionTools ocrTool, bool isMacOs, bool isWindows, ProgressBar progressBar)
        {
            _ocrTool = ocrTool ?? throw new ArgumentNullException(nameof(ocrTool));
            _isMacOs = isMacOs;
            _isWindows = isWindows;
            _progressBar = progressBar ?? throw new ArgumentNullException(nameof(progressBar));
            _ensembleOcr = new EnsembleOcr();
        }

        /// <summary>
        /// Processes an array of images using specified preprocessing methods.
        /// For each image, this method:
        /// 1. Applies all selected preprocessing methods
        /// 2. Performs OCR on both original and preprocessed images
        /// 3. Measures performance metrics for each step
        /// 4. Combines results using ensemble methods
        /// 5. Determines the best preprocessing methods
        /// 6. Generates visualizations and reports
        /// </summary>
        /// <param name="imageFiles">Array of image file paths to process.</param>
        /// <param name="preprocessMethods">List of preprocessing methods to apply.</param>
        /// <param name="processedImagesFolder">Folder to save processed images.</param>
        /// <param name="ocrResultsFolder">Folder to save OCR results.</param>
        /// <returns>Dictionary containing extracted text for each image.</returns>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
        public async Task<System.Collections.Concurrent.ConcurrentDictionary<string, string>> ProcessImagesAsync(
            string[] imageFiles,
            List<(string Name, Func<string, Mat> Method)> preprocessMethods,
            string processedImagesFolder,
            string ocrResultsFolder)
        {
            // Validate parameters
            if (imageFiles == null) throw new ArgumentNullException(nameof(imageFiles));
            if (preprocessMethods == null) throw new ArgumentNullException(nameof(preprocessMethods));
            if (string.IsNullOrEmpty(processedImagesFolder)) throw new ArgumentNullException(nameof(processedImagesFolder));
            if (string.IsNullOrEmpty(ocrResultsFolder)) throw new ArgumentNullException(nameof(ocrResultsFolder));
            
            // Create list of method names for result reporting
            var ocrSteps = new List<string> { "Original" };
            ocrSteps.AddRange(preprocessMethods.Select(m => m.Name));

            // Process each image in parallel
            var tasks = imageFiles.Select(async (imagePath, index) =>
            {
                // Create image-specific output directories
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imagePath);
                var localPbar = _progressBar; // Use a local variable to avoid capturing the disposed progress bar
                localPbar.Message = $"Processing image: {fileNameWithoutExtension} ";
                
                // Create folders for this specific image
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
                
                // Measure performance metrics for original image OCR
                Stopwatch ocrStopwatch = Stopwatch.StartNew();
                GC.Collect(); // Force garbage collection before measurement
                long memoryBeforeOcr = GC.GetTotalMemory(true);
                
                // Perform OCR on the original image
                OcrExtractionHelper.ProcessOcrForImage(originalImagePath, originalOcrToolFolder, _ocrTool, _isMacOs, _isWindows, "original");
                
                long memoryAfterOcr = GC.GetTotalMemory(true);
                ocrStopwatch.Stop();
                
                // Record performance metrics for the original image
                ocrTimes.Add((fileNameWithoutExtension, "Original OCR", ocrStopwatch.Elapsed.TotalMilliseconds, Math.Abs(memoryAfterOcr - memoryBeforeOcr)));

                // Read the OCR results for the original image
                var originalOcrResults = OcrFileReader.ReadOcrResultsFromFiles(new List<string> { Path.Combine(imageOcrResultFolder, "original.txt") });
                ocrResults.AddRange(originalOcrResults);

                // Apply each selected preprocessing method and perform OCR
                foreach (var (methodName, method) in preprocessMethods)
                {
                    // Measure preprocessing performance
                    Stopwatch preprocessStopwatch = Stopwatch.StartNew();
                    GC.Collect(); // Force garbage collection before measurement
                    long memoryBeforePreprocess = GC.GetTotalMemory(true);
                    
                    // Apply the preprocessing method to the image
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
                    
                    // Perform OCR on the preprocessed image
                    OcrExtractionHelper.ProcessOcrForImage(preprocessedImagePath, ocrToolFolder, _ocrTool, _isMacOs, _isWindows, methodName);
                    
                    long memoryAfterOcrPreprocess = GC.GetTotalMemory(false);
                    ocrPreprocessStopwatch.Stop();
                    
                    // Record OCR performance metrics for this method
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
                
                // Generate ensemble OCR result using majority voting and API
                string groundTruth = await _ensembleOcr.SendOcrTextsToApiAsync(ocrResultForGroundTruth);
                _ = groundTruth; // Discard the API result as we're using majority voting instead
                
                // Use majority voting to determine the ground truth
                groundTruth = _ensembleOcr.CombineUsingMajorityVoting(ocrResultForGroundTruth);
                
                // Save the final ensemble OCR result
                string groundTruthFilePath = Path.Combine(imageOcrResultFolder, "final_ocr_result.txt");
                await File.WriteAllTextAsync(groundTruthFilePath, groundTruth);
                
                // Create similarity matrix generator for analysis
                TextSimilarity.SimilarityMatrixGenerator similarityMatrixGenerator = new TextSimilarity.SimilarityMatrixGenerator();

                // Create and visualize cosine similarity matrix
                await similarityMatrixGenerator.GenerateAndVisualizeOcrSimilarityMatrix(ocrResults, groundTruth, excelFilePath, ocrSteps);
                
                // Create and visualize Levenshtein distance similarity matrix
                await similarityMatrixGenerator.GenerateAndVisualizeOcrSimilarityMatrixLv(ocrResults, groundTruth, excelFilePath, ocrSteps);
                
                // Generate report on which preprocessing methods were most effective
                await similarityMatrixGenerator.GeneratePreprocessingEffectivenessReport(ocrResults, groundTruth, excelFilePath, ocrSteps);
                    
                // Create OcrComparison instance to determine best preprocessing methods
                var ocrComparison = new OcrComparison();
                
                // Find the best preprocessing method based on cosine similarity
                string bestPreprocessingMethod = await ocrComparison.FindBestPreprocessingMethod(ocrResults, groundTruth, ocrSteps);
                
                // Store the best method for later use if it's not the original image
                if (!string.IsNullOrEmpty(bestPreprocessingMethod) && bestPreprocessingMethod != "Original")
                {
                    // Use concurrent dictionary to avoid thread issues
                    _bestPreprocessingMethods.AddOrUpdate(
                        fileNameWithoutExtension,
                        bestPreprocessingMethod,
                        (_, existing) => bestPreprocessingMethod
                    );
                }
                
                // Find the best preprocessing method based on Levenshtein distance
                string bestLevenshteinMethod = await ocrComparison.FindBestLevenshteinMethod(ocrResults, groundTruth, ocrSteps);
                
                // Store the best Levenshtein method if it's not the original image
                if (!string.IsNullOrEmpty(bestLevenshteinMethod) && bestLevenshteinMethod != "Original")
                {
                    _bestLevenshteinMethods.AddOrUpdate(
                        fileNameWithoutExtension,
                        bestLevenshteinMethod,
                        (_, existing) => bestLevenshteinMethod
                    );
                }
                
                // Generate and visualize text embeddings for comparing OCR results
                var embeddings = similarityMatrixGenerator.GenerateTextEmbeddings(ocrResults, ocrSteps);
                ExecutionTimeLogger.CreateEmbeddingVisualization(excelFilePath, embeddings, ocrSteps);
            
                // Store the extracted text in the results dictionary
                _extractedTexts[imagePath] = groundTruth;
                
                // Update progress tracking
                _imagesProcessed = _imagesProcessed + 1;
                _progressPercentage = (_imagesProcessed) * 100f / imageFiles.Length;
                
                // Send progress percentage to stdout in a special format that GUI will recognize
                Console.WriteLine($"##PROGRESS:{_progressPercentage}");
                Console.WriteLine($"##PROCESSED:{_imagesProcessed}");
                
                // Update progress bar with completed message
                localPbar.Message = $"Completed";
                localPbar.Tick();
            });

            // Wait for all image processing tasks to complete
            await Task.WhenAll(tasks);
            
            // Return the dictionary of extracted texts
            return _extractedTexts;
        }
        
        /// <summary>
        /// Gets the dictionary of best preprocessing methods determined by cosine similarity.
        /// </summary>
        /// <returns>A concurrent dictionary mapping image names to their best preprocessing methods based on cosine similarity.</returns>
        public System.Collections.Concurrent.ConcurrentDictionary<string, string> GetBestPreprocessingMethods()
        {
            return _bestPreprocessingMethods;
        }
        
        /// <summary>
        /// Gets the dictionary of best preprocessing methods determined by Levenshtein distance.
        /// </summary>
        /// <returns>A concurrent dictionary mapping image names to their best preprocessing methods based on Levenshtein distance.</returns>
        public System.Collections.Concurrent.ConcurrentDictionary<string, string> GetBestLevenshteinMethods()
        {
            return _bestLevenshteinMethods;
        }
    }
} 