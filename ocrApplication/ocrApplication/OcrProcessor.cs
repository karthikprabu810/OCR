using Emgu.CV;
using ShellProgressBar;
using System.Diagnostics;
using System.Collections.Concurrent;

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
        private readonly ConcurrentDictionary<string, string> _extractedTexts = 
            new ConcurrentDictionary<string, string>();
            
        // Dictionary to track best preprocessing method by cosine similarity for each image (thread-safe)
        private readonly ConcurrentDictionary<string, string> _bestPreprocessingMethods = 
            new ConcurrentDictionary<string, string>();
            
        // Dictionary to track best preprocessing method by Levenshtein distance for each image (thread-safe)
        private readonly ConcurrentDictionary<string, string> _bestLevenshteinMethods = 
            new ConcurrentDictionary<string, string>();
            
        // Dictionary to track best preprocessing method by clustering for each image (thread-safe)
        private readonly ConcurrentDictionary<string, string> _bestClusteringMethods = 
            new ConcurrentDictionary<string, string>();
            
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
        public async Task<ConcurrentDictionary<string, string>> ProcessImagesAsync(
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
                
                // Apply additional filtering to remove redundant lines
                groundTruth = _ensembleOcr.FilterRedundantLines(groundTruth);
                
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
                
                // Store the best method for later use (even if it's "Original")
                if (!string.IsNullOrEmpty(bestPreprocessingMethod))
                {
                    // Use concurrent dictionary to avoid thread issues
                    _bestPreprocessingMethods.AddOrUpdate(
                        fileNameWithoutExtension,
                        bestPreprocessingMethod,
                        (_, _) => bestPreprocessingMethod
                    );
                }
                
                // Find the best preprocessing method based on Levenshtein distance
                string bestLevenshteinMethod = await ocrComparison.FindBestLevenshteinMethod(ocrResults, groundTruth, ocrSteps);
                
                // Store the best Levenshtein method (even if it's "Original")
                if (!string.IsNullOrEmpty(bestLevenshteinMethod))
                {
                    _bestLevenshteinMethods.AddOrUpdate(
                        fileNameWithoutExtension,
                        bestLevenshteinMethod,
                        (_, _) => bestLevenshteinMethod
                    );
                }
                
                // Generate and visualize text embeddings for comparing OCR results
                var embeddings = similarityMatrixGenerator.GenerateTextEmbeddings(ocrResults, ocrSteps);
                ExecutionTimeLogger.CreateEmbeddingVisualization(excelFilePath, embeddings, ocrSteps);
                
                // Perform clustering analysis on preprocessed images
                try
                {
                    var clusterAnalysis = new ClusterAnalysis();
                    var featureVectors = new List<double[]>();
                    
                    // Extract feature vectors for "Original" image
                    string originalImgPath = Path.Combine(imageProcessedFolder, "original.jpg");
                    Mat originalImage = CvInvoke.Imread(originalImgPath);
                    double[] originalFeatures = ExtractFeatures(originalImage);
                    featureVectors.Add(originalFeatures);
                    
                    // Extract features from each preprocessed image
                    var preprocessingMethodNames = new List<string> { "Original" };
                    foreach (var (methodName, _) in preprocessMethods)
                    {
                        preprocessingMethodNames.Add(methodName);
                        string preprocessedImagePath = Path.Combine(imageProcessedFolder, $"{methodName}.jpg");
                        
                        if (File.Exists(preprocessedImagePath))
                        {
                            Mat preprocessedImage = CvInvoke.Imread(preprocessedImagePath);
                            double[] features = ExtractFeatures(preprocessedImage);
                            featureVectors.Add(features);
                        }
                    }
                    
                    // Perform clustering and get results with individual silhouette scores
                    var (clusterLabels, overallSilhouetteScore, individualSilhouetteScores) = 
                        clusterAnalysis.PerformClustering(featureVectors, 
                            numClusters: Math.Min(3, preprocessingMethodNames.Count));
                    
                    // Calculate method quality score based on both clustering and silhouette score
                    // Higher score = better method
                    var methodScores = new Dictionary<string, double>();
                    for (int i = 0; i < Math.Min(preprocessingMethodNames.Count, individualSilhouetteScores.Length); i++)
                    {
                        double score = individualSilhouetteScores[i];
                        // Ensure we don't use invalid scores
                        if (!double.IsNaN(score) && !double.IsInfinity(score))
                        {
                            methodScores[preprocessingMethodNames[i]] = score;
                        }
                    }
                    
                    // Determine best preprocessing method based on clustering and silhouette scores
                    string bestClusterMethod = DetermineBestClusterMethod(
                        clusterLabels.Skip(1).ToArray(), // Skip original image's cluster
                        preprocessMethods,
                        bestPreprocessingMethod,
                        bestLevenshteinMethod,
                        methodScores);
                    
                    // Save clustering results to Excel with individual silhouette scores
                    ExecutionTimeLogger.SaveClusteringResultsToExcel(
                        excelFilePath, 
                        clusterLabels, 
                        overallSilhouetteScore,
                        individualSilhouetteScores, 
                        "clusterAnalysis", 
                        bestClusterMethod, 
                        preprocessingMethodNames);
                        
                    // Store the best clustering method in the dictionary
                    if (!string.IsNullOrEmpty(bestClusterMethod))
                    {
                        _bestClusteringMethods.AddOrUpdate(
                            fileNameWithoutExtension,
                            bestClusterMethod,
                            (_, _) => bestClusterMethod
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during clustering analysis: {ex.Message}");
                }
            
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
        public ConcurrentDictionary<string, string> GetBestPreprocessingMethods()
        {
            return _bestPreprocessingMethods;
        }
        
        /// <summary>
        /// Gets the dictionary of best preprocessing methods determined by Levenshtein distance.
        /// </summary>
        /// <returns>A concurrent dictionary mapping image names to their best preprocessing methods based on Levenshtein distance.</returns>
        public ConcurrentDictionary<string, string> GetBestLevenshteinMethods()
        {
            return _bestLevenshteinMethods;
        }

        /// <summary>
        /// Gets the dictionary of best preprocessing methods determined by clustering analysis.
        /// </summary>
        /// <returns>A concurrent dictionary mapping image names to their best preprocessing methods based on clustering.</returns>
        public ConcurrentDictionary<string, string> GetBestClusteringMethods()
        {
            return _bestClusteringMethods;
        }

        /// <summary>
        /// Extracts feature vectors from an image.
        /// </summary>
        /// <param name="image">The image to extract features from.</param>
        /// <returns>A feature vector representing the image.</returns>
        private double[] ExtractFeatures(Mat image)
        {
            var clusterAnalysis = new ClusterAnalysis();
            return clusterAnalysis.ExtractFeatures(image);
        }

        /// <summary>
        /// Determines the best preprocessing method based on clustering results.
        /// </summary>
        /// <param name="clusterLabels">Cluster labels for each preprocessing method.</param>
        /// <param name="preprocessMethods">List of preprocessing methods.</param>
        /// <param name="bestCosineSimilarityMethod">Best preprocessing method based on cosine similarity.</param>
        /// <param name="bestLevenshteinMethod">Best preprocessing method based on Levenshtein distance.</param>
        /// <param name="methodScores">Dictionary of silhouette scores for each method.</param>
        /// <returns>The name of the best preprocessing method.</returns>
        private string DetermineBestClusterMethod(
            int[] clusterLabels, 
            List<(string Name, Func<string, Mat> Method)> preprocessMethods,
            string bestCosineSimilarityMethod,
            string bestLevenshteinMethod,
            Dictionary<string, double> methodScores = null)
        {
            // If we have individual method scores, prioritize methods with high silhouette scores
            if (methodScores != null && methodScores.Count > 0)
            {
                // Get methods with positive silhouette scores (well-clustered)
                var goodMethods = methodScores.Where(m => m.Value > 0.3).OrderByDescending(m => m.Value).ToList();
                
                if (goodMethods.Count > 0)
                {
                    // If best text-based method has good clustering, prioritize it
                    if (!string.IsNullOrEmpty(bestCosineSimilarityMethod) && 
                        methodScores.TryGetValue(bestCosineSimilarityMethod, out double cosineScore) && 
                        cosineScore > 0.1)
                    {
                        return bestCosineSimilarityMethod;
                    }
                    
                    if (!string.IsNullOrEmpty(bestLevenshteinMethod) && 
                        methodScores.TryGetValue(bestLevenshteinMethod, out double levenScore) && 
                        levenScore > 0.1)
                    {
                        return bestLevenshteinMethod;
                    }
                    
                    // Otherwise, return the method with highest silhouette score
                    return goodMethods[0].Key;
                }
            }
            
            // If no good methods found by silhouette score, use text similarity methods if available
            if (!string.IsNullOrEmpty(bestCosineSimilarityMethod))
            {
                return bestCosineSimilarityMethod;
            }
            
            if (!string.IsNullOrEmpty(bestLevenshteinMethod))
            {
                return bestLevenshteinMethod;
            }
            
            // If no best method is determined yet, pick the method with the most frequently occurring cluster
            if (clusterLabels.Length > 0)
            {
                // Find the most frequent cluster
                var clusterCounts = new Dictionary<int, int>();
                for (int i = 0; i < clusterLabels.Length; i++)
                {
                    if (!clusterCounts.ContainsKey(clusterLabels[i]))
                    {
                        clusterCounts[clusterLabels[i]] = 0;
                    }
                    clusterCounts[clusterLabels[i]]++;
                }
                
                // Get the cluster with the most elements
                int mostFrequentCluster = clusterCounts.OrderByDescending(kv => kv.Value).First().Key;
                
                // Return the first preprocessing method in that cluster
                for (int i = 0; i < clusterLabels.Length && i < preprocessMethods.Count; i++)
                {
                    if (clusterLabels[i] == mostFrequentCluster)
                    {
                        return preprocessMethods[i].Name;
                    }
                }
            }
            
            // Default to first method if something goes wrong
            return preprocessMethods.Count > 0 ? preprocessMethods[0].Name : "Original";
        }
    }
} 