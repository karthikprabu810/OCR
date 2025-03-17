using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.Drawing;

namespace unitTestProject
{
    /// <summary>
    /// Provides utility methods for OCR application testing, including test file and image generation.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Creates a temporary file with specified content for testing purposes.
        /// </summary>
        /// <param name="content">Content to write to the file</param>
        /// <param name="extension">File extension (defaults to .txt)</param>
        /// <returns>Full path to the created temporary file</returns>
        public static string CreateTempFile(string content, string extension = ".txt")
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"ocr_test_{Guid.NewGuid():N}{extension}");
            File.WriteAllText(tempFile, content);
            return tempFile;
        }
        
        /// <summary>
        /// Generates a test image containing specified text for OCR testing.
        /// </summary>
        /// <param name="text">Text to render in the image</param>
        /// <param name="width">Image width in pixels</param>
        /// <param name="height">Image height in pixels</param>
        /// <returns>Path to the generated test image</returns>
        public static string CreateTestImage(string text, int width = 400, int height = 200)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), $"ocr_test_image_{Guid.NewGuid():N}.png");
            
            // Create a blank white image
            Mat img = new Mat(height, width, DepthType.Cv8U, 3);
            img.SetTo(new MCvScalar(255, 255, 255)); // White background
            
            // Add text to the image
            CvInvoke.PutText(
                img,
                text,
                new Point(20, height / 2), // Position the text
                FontFace.HersheyDuplex,
                1.0, // Font scale
                new MCvScalar(0, 0, 0), // Black text
                2 // Thickness
            );
            
            // Save the image
            CvInvoke.Imwrite(tempFile, img);
            
            return tempFile;
        }
        
        /// <summary>
        /// Locates a test configuration file for OCR settings.
        /// Uses the ConfigLocator class to find the config.json file.
        /// </summary>
        /// <returns>Path to the configuration file</returns>
        /// <exception cref="FileNotFoundException">Thrown if config file cannot be found</exception>
        public static string CreateTestConfig()
        {
            return ocrApplication.ConfigLocator.FindConfigFile();
        }
        
        /// <summary>
        /// Deletes a test file if it exists.
        /// </summary>
        /// <param name="filePath">Path to the file to delete</param>
        private static void DeleteTestFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        
        /// <summary>
        /// Removes all specified temporary test files.
        /// </summary>
        /// <param name="filePaths">Array of file paths to delete</param>
        public static void CleanupTestFiles(params string[] filePaths)
        {
            foreach (var path in filePaths)
            {
                DeleteTestFile(path);
            }
        }
    }
} 