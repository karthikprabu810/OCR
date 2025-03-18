using System.Runtime.InteropServices;
using ShellProgressBar;
using OfficeOpenXml;
using System.Collections.Concurrent;

namespace ocrApplication
{
    /// <summary>
    /// Main program class that orchestrates the OCR processing workflow.
    /// Handles loading images, applying preprocessing techniques, performing OCR, and generating reports.
    /// This class serves as the entry point for the application and coordinates all major components.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Main entry point for the application. Processes all images in the specified folder
        /// using various preprocessing techniques and OCR methods.
        /// 
        /// The workflow includes:
        /// 1. Reading input and output folder paths from user or command line arguments
        /// 2. Detecting operating system for platform-specific OCR implementations
        /// 3. Loading OCR configuration
        /// 4. Finding image files to process
        /// 5. Selecting preprocessing methods from args or user input
        /// 6. Processing each image with the selected methods
        /// 7. Generating summary reports and visualizations
        /// </summary>
        /// <param name="args">Command line arguments: [inputFolder] [outputFolder] [preprocessingMethods]</param>
        /// <returns>Task representing the asynchronous operation.</returns>
        static async Task Main(string[] args)
        {
            // Set EPPlus license context for Excel operations
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            // Detect the operating system for platform-specific OCR implementations
            // Different OCR implementations are used depending on the platform
            bool isMacOs = RuntimeInformation.OSDescription.Contains("Darwin", StringComparison.OrdinalIgnoreCase);
            bool isWindows = RuntimeInformation.OSDescription.Contains("Windows", StringComparison.OrdinalIgnoreCase);

            // ConfigLocator automatically searches for the config file in various locations
            string configFilePath;
            try
            {
                configFilePath = ConfigLocator.FindConfigFile();
            }
            catch (FileNotFoundException ex)
            {
                // If configuration is missing, inform the user and exit
                Console.WriteLine(ex.Message);
                return;
            }

            // Initialize OCR tools with configuration settings
            OcrExtractionTools ocrTool = new OcrExtractionTools(configFilePath);

            // Create timestamp-based output folders to prevent overwriting previous results
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Variables to hold input/output paths and preprocessing methods
            string inputFolderPath;
            string outputFolderPath;
            List<(string Name, Func<string, Emgu.CV.Mat> Method)> selectedPreprocessMethods;
            
            // Parse command line arguments if provided
            if (args.Length > 0 && args[0] != "-h" && args[0] != "--help" && args[0] != "/?")
            {
                // Get input folder from args or prompt if invalid
                inputFolderPath = args[0];
                if (!Directory.Exists(inputFolderPath))
                {
                    Console.WriteLine($"Error: Input folder '{inputFolderPath}' does not exist.");
                    inputFolderPath = InputHandler.GetFolderPath("Enter a valid input folder path: ");
                }
                
                // Get output folder from args or prompt if args insufficient
                outputFolderPath = args.Length > 1 ? args[1] : InputHandler.GetFolderPath("Enter the output folder path: ");
                if (!Directory.Exists(outputFolderPath))
                {
                    try
                    {
                        Directory.CreateDirectory(outputFolderPath);
                        Console.WriteLine($"Created output directory: {outputFolderPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating output directory: {ex.Message}");
                        outputFolderPath = InputHandler.GetFolderPath("Enter a valid output folder path: ");
                    }
                }
                
                // Get all preprocessing methods
                var allPreprocessMethods = InputHandler.GetAllPreprocessingMethods();
                
                // Get preprocessing methods from args or interactive selection
                if (args.Length > 2)
                {
                    // Parse preprocessing methods from command line
                    string[] methodsInput = args[2].Split(',');
                    var tempMethods = new List<(string Name, Func<string, Emgu.CV.Mat> Method)>();
                    
                    // Convert method indices or names to actual method tuples
                    foreach (string method in methodsInput)
                    {
                        if (int.TryParse(method, out int methodIndex) && methodIndex > 0 && methodIndex <= allPreprocessMethods.Length)
                        {
                            tempMethods.Add(allPreprocessMethods[methodIndex - 1]);
                        }
                        else
                        {
                            // Find by name
                            var matchingMethod = allPreprocessMethods.FirstOrDefault(m => 
                                string.Equals(m.Name, method, StringComparison.OrdinalIgnoreCase));
                            
                            if (matchingMethod != default)
                            {
                                tempMethods.Add(matchingMethod);
                            }
                        }
                    }
                    
                    // If no valid methods were provided, fall back to interactive selection
                    if (tempMethods.Count == 0)
                    {
                        Console.WriteLine("No valid preprocessing methods specified. Please select from available methods:");
                        selectedPreprocessMethods = InputHandler.SelectPreprocessingMethods(allPreprocessMethods);
                    }
                    else
                    {
                        selectedPreprocessMethods = tempMethods;
                        
                        Console.WriteLine("Selected preprocessing methods:");
                        for (int i = 0; i < selectedPreprocessMethods.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {selectedPreprocessMethods[i].Name}");
                        }
                        Console.WriteLine();
                    }
                }
                else
                {
                    // No preprocessing methods in args, use interactive selection
                    selectedPreprocessMethods = InputHandler.SelectPreprocessingMethods(allPreprocessMethods);
                }
            }
            else if (args.Length > 0) // Handle help flags
            {
                DisplayHelp();
                return;
            }
            else
            {
                // No command line args provided, get all inputs interactively
                inputFolderPath = InputHandler.GetFolderPath("Enter the input folder path: ");
                outputFolderPath = InputHandler.GetFolderPath("Enter the output folder path: ");
                
                // Get all available preprocessing methods
                var allPreprocessMethods = InputHandler.GetAllPreprocessingMethods();
                
                // Let user select which preprocessing methods to use
                selectedPreprocessMethods = InputHandler.SelectPreprocessingMethods(allPreprocessMethods);
            }
            
            // Add timestamp to output path to ensure uniqueness
            outputFolderPath = Path.Combine(outputFolderPath, timestamp);

            // Create directory structure for storing results
            // Different types of outputs are organized in subfolders
            string processedImagesFolder = Path.Combine(outputFolderPath, "processed_images");
            string ocrResultsFolder = Path.Combine(outputFolderPath, "ocr_results");

            // Create output directories
            Directory.CreateDirectory(processedImagesFolder);
            Directory.CreateDirectory(ocrResultsFolder);

            // Find all image files in the input directory and subdirectories
            // InputHandler.DiscoverImageFiles recursively searches for supported image types
            string[] imageFiles = InputHandler.DiscoverImageFiles(inputFolderPath);

            // Verify images were found before proceeding
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("No image files found in the specified folder or subfolders.");
                return;
            }
            /*
            // Display summary of what will be processed
            Console.WriteLine($"\nReady to process {imageFiles.Length} images from {inputFolderPath}");
            Console.WriteLine($"Results will be saved to {outputFolderPath}");
            Console.WriteLine($"Using {selectedPreprocessMethods.Count} preprocessing methods: {string.Join(", ", selectedPreprocessMethods.Select(m => m.Name))}");
            Console.WriteLine("Press any key to continue or Ctrl+C to cancel...");
            Console.ReadKey();
            Console.WriteLine("\nStarting processing...\n");
            */
            // ShellProgressBar provides visual feedback during long-running operations
            var options = new ProgressBarOptions
            {
                ForegroundColor = ConsoleColor.Yellow,
                ForegroundColorDone = ConsoleColor.DarkGreen,
                BackgroundColor = ConsoleColor.DarkGray,
                BackgroundCharacter = '\u2593'
            };
            
            // Use the using statement to ensure proper disposal of progress bar
            using (var pbar = new ProgressBar(imageFiles.Length, "Processing Images", options))
            {
                // OcrProcessor orchestrates the entire OCR workflow
                var ocrProcessor = new OcrProcessor(ocrTool, isMacOs, isWindows, pbar);
                
                // This is the main processing loop that applies preprocessing and OCR
                var extractedTexts = await ocrProcessor.ProcessImagesAsync(
                    imageFiles, 
                    selectedPreprocessMethods, 
                    processedImagesFolder, 
                    ocrResultsFolder);
            
                // Log completion for all images
                Console.WriteLine("\n\n\nOCR processing complete for all images in the folder and its subfolders.");
            
                // These dictionaries store the best method for each image based on different metrics
                ConcurrentDictionary<string, string> bestPreprocessingMethods = ocrProcessor.GetBestPreprocessingMethods();
                ConcurrentDictionary<string, string> bestLevenshteinMethods = ocrProcessor.GetBestLevenshteinMethods();
                ConcurrentDictionary<string, string> bestClusteringMethods = ocrProcessor.GetBestClusteringMethods();
                
                // OcrSummary provides methods for summarizing and visualizing results
                OcrSummary.DisplayExtractedTexts(extractedTexts);
                
                // Use the OcrSummary class to generate and display summary information
                // Pass the best methods directly from OcrProcessor instead of reading from Excel
                OcrSummary.DisplayBestMethodsSummary(bestPreprocessingMethods, bestLevenshteinMethods, bestClusteringMethods);
                
                // Export results to various formats (Excel, PDF, etc.)
                // ExportUtilities handles the details of exporting to different formats
                ExportUtilities.ExportResults(
                    outputFolderPath + "/OCR_Results", 
                    extractedTexts, 
                    bestPreprocessingMethods, 
                    bestLevenshteinMethods, 
                    bestClusteringMethods,
                    new Dictionary<string, string>() // Empty dictionary as we're not using it
                );
            }

            // Final message to indicate all processing is complete
            Console.WriteLine("\n\n\nProcessing complete.");
        }
        
        /// <summary>
        /// Displays command-line usage information for the application.
        /// </summary>
        private static void DisplayHelp()
        {
            Console.WriteLine("\nOCR Application Usage:");
            Console.WriteLine("----------------------");
            Console.WriteLine("Run without arguments for interactive mode:");
            Console.WriteLine("  dotnet ocrApplication.dll");
            Console.WriteLine("\nOr specify arguments for batch processing:");
            Console.WriteLine("  dotnet ocrApplication.dll [inputFolder] [outputFolder] [preprocessingMethods]");
            Console.WriteLine("\nWhere:");
            Console.WriteLine("  [inputFolder]          - Path to folder containing images to process");
            Console.WriteLine("  [outputFolder]         - Path where results will be saved");
            Console.WriteLine("  [preprocessingMethods] - Comma-separated list of preprocessing method numbers or names");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  dotnet ocrApplication.dll C:\\Images C:\\Results 1,3,5");
            Console.WriteLine("  dotnet ocrApplication.dll ~/Documents/Images ~/Documents/Results \"Grayscale,Binarization,Noise Removal\"");
            Console.WriteLine("\nUse -h, --help, or /? to display this help information");
        }
    }
}