using System.Runtime.InteropServices;
using ShellProgressBar;
using OfficeOpenXml;

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
        /// 1. Reading input and output folder paths from user
        /// 2. Detecting operating system for platform-specific OCR implementations
        /// 3. Loading OCR configuration
        /// 4. Finding image files to process
        /// 5. Selecting preprocessing methods
        /// 6. Processing each image with the selected methods
        /// 7. Generating summary reports and visualizations
        /// </summary>
        /// <returns>Task representing the asynchronous operation.</returns>
        static async Task Main()
        {
            // Set EPPlus license context for Excel operations
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            // Detect the operating system for platform-specific OCR implementations
            bool isMacOs = RuntimeInformation.OSDescription.Contains("Darwin", StringComparison.OrdinalIgnoreCase);
            bool isWindows = RuntimeInformation.OSDescription.Contains("Windows", StringComparison.OrdinalIgnoreCase);

            // Find config file using the ConfigLocator utility
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

            // Get input and output folder paths from user
            string inputFolderPath = InputHandler.GetFolderPath("Enter the input folder path: ");
            string outputFolderPath = InputHandler.GetFolderPath("Enter the output folder path: ");
            
            // Add timestamp to output path to ensure uniqueness
            outputFolderPath = Path.Combine(outputFolderPath, timestamp);

            // Create directory structure for storing results
            string processedImagesFolder = Path.Combine(outputFolderPath, "processed_images");
            string ocrResultsFolder = Path.Combine(outputFolderPath, "ocr_results");

            // Create output directories
            Directory.CreateDirectory(processedImagesFolder);
            Directory.CreateDirectory(ocrResultsFolder);

            // Find all image files in the input directory and subdirectories
            string[] imageFiles = InputHandler.DiscoverImageFiles(inputFolderPath);

            // Verify images were found before proceeding
            if (imageFiles.Length == 0)
            {
                Console.WriteLine("No image files found in the specified folder or subfolders.");
                return;
            }

            // Get all available preprocessing methods
            var allPreprocessMethods = InputHandler.GetAllPreprocessingMethods();
            
            // Let user select which preprocessing methods to use
            var preprocessMethods = InputHandler.SelectPreprocessingMethods(allPreprocessMethods);
            
            // Initialize progress bar for display with custom styling
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
                // Create the OCR processor with the necessary dependencies
                var ocrProcessor = new OcrProcessor(ocrTool, isMacOs, isWindows, pbar);
                
                // Process all images with selected preprocessing methods
                var extractedTexts = await ocrProcessor.ProcessImagesAsync(
                    imageFiles, 
                    preprocessMethods, 
                    processedImagesFolder, 
                    ocrResultsFolder);
            
            // Log completion for all images
            Console.WriteLine("OCR processing complete for all images in the folder and its subfolders.");
            
                // Use the OcrSummary class to generate and display summary information
                OcrSummary.GenerateAndExportSummary(imageFiles, ocrResultsFolder, outputFolderPath, extractedTexts);
            }

            // Final message to indicate all processing is complete
            Console.WriteLine("\nProcessing complete.");
        }
    }
}