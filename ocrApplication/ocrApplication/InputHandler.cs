using Emgu.CV;

namespace ocrApplication
{
    /// <summary>
    /// Handles user input for the OCR application, including preprocessing method selection
    /// and path specifications. Provides methods for collecting user input and validating paths.
    /// </summary>
    public static class InputHandler
    {
        /// <summary>
        /// Prompts the user for a valid folder path, ensuring that the input is not empty or whitespace.
        /// </summary>
        /// <param name="promptMessage">Message to display when prompting for input.</param>
        /// <returns>A valid folder path entered by the user.</returns>
        /// <exception cref="ArgumentNullException">Thrown if promptMessage is null.</exception>
        public static string GetFolderPath(string promptMessage)
        {
            if (string.IsNullOrEmpty(promptMessage))
                throw new ArgumentNullException(nameof(promptMessage));
                
            string? folderPath;
            do
            {
                // Show prompt and read user input
                Console.Write(promptMessage);
                folderPath = Console.ReadLine();
                
                // Continue prompting until valid input is received
            } while (string.IsNullOrWhiteSpace(folderPath));
            
            return folderPath;
        }
        
        /// <summary>
        /// Discovers all image files in a specified folder and its subfolders.
        /// Supports common image formats: PNG, JPG, and JPEG.
        /// </summary>
        /// <param name="inputFolderPath">The folder to search for images.</param>
        /// <returns>An array of image file paths found in the directory and subdirectories.</returns>
        /// <exception cref="ArgumentNullException">Thrown if inputFolderPath is null.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if inputFolderPath does not exist.</exception>
        public static string[] DiscoverImageFiles(string inputFolderPath)
        {
            if (string.IsNullOrEmpty(inputFolderPath))
                throw new ArgumentNullException(nameof(inputFolderPath));
                
            if (!Directory.Exists(inputFolderPath))
                throw new DirectoryNotFoundException($"Directory not found: {inputFolderPath}");
                
            // Find all image files in the input directory and subdirectories
            // Only include files with supported image extensions (case-insensitive)
            string[] imageFiles = Directory.GetFiles(inputFolderPath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                               file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                .ToArray();
                
            return imageFiles;
        }
        
        /// <summary>
        /// Gets all available preprocessing methods from the ImagePreprocessing class.
        /// Each method is returned as a tuple containing the method name and a function reference.
        /// </summary>
        /// <returns>Array of preprocessing methods with names and function references.</returns>
        public static (string Name, Func<string, Mat> Method)[] GetAllPreprocessingMethods()
        {
            // Return an array of tuples, each containing:
            // - Name: A string identifier for the preprocessing method
            // - Method: A function reference to the actual method in ImagePreprocessing
            return new (string Name, Func<string, Mat> Method)[]
            {
                // Basic image transformations
                ("Grayscale", ImagePreprocessing.ConvertToGrayscale),                     // Convert to grayscale (removes color)
                ("Gaussian_Filter", ImagePreprocessing.RemoveNoiseUsingGaussian),         // Gaussian blur for noise reduction
                ("Median_Filter", ImagePreprocessing.RemoveNoiseUsingMedian),             // Median blur for noise reduction
                
                // Thresholding methods
                ("Adaptive_Thresholding", ImagePreprocessing.AdaptiveThresholding),       // Adaptive thresholding for varying lighting
                ("Otsu_Binarization", ImagePreprocessing.OtsuBinarization),               // Automatic threshold detection
                
                // Enhancement methods
                ("Gamma_Correction", ImagePreprocessing.GammaCorrection),                 // Gamma correction for brightness adjustment
                ("Histogram_Equalization", ImagePreprocessing.HistogramEqualization),     // Contrast improvement
                ("LogTransform", ImagePreprocessing.LogTransform),                        // Enhance details in dark regions
                ("Normalize_Image", ImagePreprocessing.NormalizeImage),                   // Normalize pixel value range
                
                // Edge detection methods
                ("Canny_Edge", ImagePreprocessing.CannyEdgeDetection),                    // Edge detection using Canny algorithm
                ("Sobel_Edge_Detection", ImagePreprocessing.SobelEdgeDetection),          // Edge detection (gradient-based)
                ("Laplacian_Edge_Detection", ImagePreprocessing.LaplacianEdgeDetection),  // Edge detection (second derivative)
                
                // Morphological operations
                ("Dilation", ImagePreprocessing.Dilation),                                // Expands white regions (text enhancement)
                ("Erosion", ImagePreprocessing.Erosion),                                  // Shrinks white regions (noise removal)
                ("Morphological_Opening", ImagePreprocessing.Opening),                    // Erosion followed by dilation
                ("Morphological_Closing", ImagePreprocessing.Closing),                    // Dilation followed by erosion
                ("Morphological_Gradient", ImagePreprocessing.MorphologicalGradient),     // Difference between dilation and erosion
                ("TopHat", ImagePreprocessing.TopHat),                                    // Difference between original and opening
                ("BlackHat", ImagePreprocessing.BlackHat),                                // Difference between closing and original
                
                // Other transformations
                ("Deskew", ImagePreprocessing.Deskew),                                    // Rotation correction
                ("BilateralFilter", ImagePreprocessing.BilateralFilter),                  // Edge-preserving smoothing
                ("ConvertToHSV", ImagePreprocessing.ConvertToHsv)                         // Alternative color representation
            };
        }
        
        /// <summary>
        /// Prompts the user to select preprocessing methods to apply from the available options.
        /// Allows selection of specific methods, all methods, or no preprocessing.
        /// </summary>
        /// <param name="allPreprocessMethods">Array of all available preprocessing methods.</param>
        /// <returns>List of selected preprocessing methods.</returns>
        /// <exception cref="ArgumentNullException">Thrown if allPreprocessMethods is null.</exception>
        public static List<(string Name, Func<string, Mat> Method)> SelectPreprocessingMethods(
            (string Name, Func<string, Mat> Method)[] allPreprocessMethods)
        {
            if (allPreprocessMethods == null)
                throw new ArgumentNullException(nameof(allPreprocessMethods));
                
            // Display available preprocessing methods with numbering
            Console.WriteLine("\nAvailable preprocessing techniques:");
            Console.WriteLine("-------------------------------------");
            for (int i = 0; i < allPreprocessMethods.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {allPreprocessMethods[i].Name}");
            }
            
            // Explain input options to the user
            Console.WriteLine("\nEnter the numbers of preprocessing techniques you want to use (comma-separated, e.g., 1,3,5):");
            Console.WriteLine("Enter 'all' to use all techniques, or '0' to skip preprocessing and only use the original image:");
            
            // Get user input and initialize the result list
            string? userInput = Console.ReadLine();
            List<(string Name, Func<string, Mat> Method)> preprocessMethods = new List<(string Name, Func<string, Mat> Method)>();
            
            // Handle "all" option: use all available preprocessing methods
            if (userInput?.Trim().ToLower() == "all")
            {
                // Add all preprocessing methods to the selected list
                preprocessMethods = allPreprocessMethods.ToList();
                Console.WriteLine("\nAll preprocessing techniques will be applied.");
                Console.WriteLine("\nInitiating preprocessing techniques and extraction...");
            }
            // Handle "0" option: use no preprocessing, just the original image
            else if (userInput?.Trim() == "0")
            {
                // Return an empty list (no preprocessing methods selected)
                Console.WriteLine("\nNo preprocessing will be applied, only the original images will be processed.");
                Console.WriteLine("\nInitiating extraction...");
            }
            // Handle specific method selection
            else
            {
                // Parse the comma-separated indices
                var selectedIndices = userInput?.Split(',')
                    .Select(index => index.Trim())
                    .Where(index => int.TryParse(index, out _))
                    .Select(index => int.Parse(index) - 1) // Convert to 0-based index
                    .Where(index => index >= 0 && index < allPreprocessMethods.Length)
                    .ToList();
                
                // If no valid indices were provided, inform the user
                if (selectedIndices == null || selectedIndices.Count == 0)
                {
                    Console.WriteLine("\nNo valid preprocessing techniques selected. Only the original images will be processed.");
                    Console.WriteLine("\nInitiating extraction...");
                }
                else
                {
                    // Add the selected methods to the result list
                    foreach (var index in selectedIndices)
                    {
                        preprocessMethods.Add(allPreprocessMethods[index]);
                    }
                    
                    // Show the selected methods to the user
                    Console.WriteLine("\nSelected preprocessing techniques:");
                    foreach (var method in preprocessMethods)
                    {
                        Console.WriteLine($"- {method.Name}");
                    }
                    Console.WriteLine("\nInitiating preprocessing techniques and extraction...");
                }
            }
            
            return preprocessMethods;
        }
    }
} 