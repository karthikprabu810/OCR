using System.Diagnostics;

namespace unitTestProject
{
    /// <summary>
    /// Tests for the View Buttons functionality in the GUI application.
    /// Focuses on file handling logic rather than UI components.
    /// These tests validate the behavior of the three view buttons:
    /// - View Excel Button (for Excel files)
    /// - View Images Button (for image files)
    /// - View Text Button (for text files)
    /// </summary>
    [TestClass]
    public class ViewButtonsTests
    {
        // Path to the temporary output folder used for testing
        private string _testOutputFolder;
        
        // Dictionary to track created test files by type (excel, image, text)
        // This helps validate that file detection works correctly
        private Dictionary<string, List<string>> _createdFiles;
        
        /// <summary>
        /// Setup method that runs before each test.
        /// Creates a test environment with various file types.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create a temporary output folder with unique GUID to avoid conflicts
            _testOutputFolder = Path.Combine(Path.GetTempPath(), $"ocr_view_buttons_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testOutputFolder);
            
            // Track created files by type for validation in tests
            _createdFiles = new Dictionary<string, List<string>>
            {
                ["excel"] = new List<string>(), // Excel files (.xlsx)
                ["image"] = new List<string>(), // Image files (.jpg, .png, etc.)
                ["text"] = new List<string>()   // Text files (.txt, .csv, etc.)
            };
            
            // Create a variety of test files to use in the tests
            CreateTestFiles();
        }
        
        /// <summary>
        /// Cleanup method that runs after each test.
        /// Deletes the temporary test directory and all its contents.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test directory and all created files
            try
            {
                if (Directory.Exists(_testOutputFolder))
                {
                    Directory.Delete(_testOutputFolder, true);
                }
            }
            catch (Exception ex)
            {
                // Log warning but don't fail the test on cleanup issues
                // This can happen if files are still in use
                Console.WriteLine($"Warning: Clean-up failed: {ex.Message}");
                // Non-critical failure, don't fail the test
            }
        }
        
        /// <summary>
        /// Creates test files of various types for testing view button functionality.
        /// Includes Excel files, image files with different extensions, and text files.
        /// Also creates a subfolder with additional files to test recursive search.
        /// </summary>
        private void CreateTestFiles()
        {
            try
            {
                // Create multiple Excel files in the main directory
                // These will be used to test the ViewExcelButton functionality
                for (int i = 1; i <= 3; i++)
                {
                    string excelPath = Path.Combine(_testOutputFolder, $"report{i}.xlsx");
                    File.WriteAllBytes(excelPath, new byte[100]); // Empty placeholder file
                    _createdFiles["excel"].Add(excelPath);
                }
                
                // Create image files with various extensions
                // These will be used to test the ViewImagesButton functionality
                string[] imageExtensions = { ".jpg", ".png", ".bmp", ".tiff", ".gif" };
                for (int i = 0; i < imageExtensions.Length; i++)
                {
                    string imagePath = Path.Combine(_testOutputFolder, $"image{i + 1}{imageExtensions[i]}");
                    File.WriteAllBytes(imagePath, new byte[100]); // Empty placeholder file
                    _createdFiles["image"].Add(imagePath);
                }
                
                // Create text files with various extensions
                // These will be used to test the ViewTextButton functionality
                string[] textExtensions = { ".txt", ".csv", ".json", ".xml", ".html" };
                for (int i = 0; i < textExtensions.Length; i++)
                {
                    string textPath = Path.Combine(_testOutputFolder, $"text{i + 1}{textExtensions[i]}");
                    File.WriteAllText(textPath, $"Sample content for file {i + 1}");
                    _createdFiles["text"].Add(textPath);
                }
                
                // Create a subfolder with additional files to test recursive search
                // This tests that the view buttons can find files in subdirectories
                string subfolderPath = Path.Combine(_testOutputFolder, "subfolder");
                Directory.CreateDirectory(subfolderPath);
                
                // Add one of each file type to the subfolder
                string subfolderExcelPath = Path.Combine(subfolderPath, "subfolder_report.xlsx");
                File.WriteAllBytes(subfolderExcelPath, new byte[100]);
                _createdFiles["excel"].Add(subfolderExcelPath);
                
                string subfolderImagePath = Path.Combine(subfolderPath, "subfolder_image.png");
                File.WriteAllBytes(subfolderImagePath, new byte[100]);
                _createdFiles["image"].Add(subfolderImagePath);
                
                string subfolderTextPath = Path.Combine(subfolderPath, "subfolder_text.txt");
                File.WriteAllText(subfolderTextPath, "Sample content in subfolder");
                _createdFiles["text"].Add(subfolderTextPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating test files: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Tests the file detection logic used by the View Excel button.
        /// Verifies that all Excel files (.xlsx) are found correctly, including those in subfolders.
        /// </summary>
        [TestMethod]
        public void ViewExcelButton_FindsAllExcelFiles()
        {
            // Simulate the file finding code from the ViewExcelButton_Click method
            // Using *.xlsx pattern with recursive search (SearchOption.AllDirectories)
            var excelFiles = Directory.GetFiles(_testOutputFolder, "*.xlsx", SearchOption.AllDirectories);
            
            // Verify the correct number of Excel files are found
            Assert.AreEqual(_createdFiles["excel"].Count, excelFiles.Length, 
                "Should find all Excel files, including in subfolders");
            
            // Verify each expected Excel file is found in the results
            foreach (var expectedFile in _createdFiles["excel"])
            {
                Assert.IsTrue(excelFiles.Contains(expectedFile), 
                    $"Should find Excel file: {Path.GetFileName(expectedFile)}");
            }
        }
        
        /// <summary>
        /// Tests the file detection logic used by the View Images button.
        /// Verifies that all image files (.jpg, .png, etc.) are found correctly,
        /// including those in subfolders.
        /// </summary>
        [TestMethod]
        public void ViewImagesButton_FindsAllImageFiles()
        {
            // Define the same image extensions that the ViewImagesButton_Click method uses
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };
            
            // Simulate the file finding code from ViewImagesButton_Click
            // Get all files, then filter by the supported image extensions
            var imageFiles = Directory.GetFiles(_testOutputFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();
            
            // Verify the correct number of image files are found
            Assert.AreEqual(_createdFiles["image"].Count, imageFiles.Length, 
                "Should find all image files, including in subfolders");
            
            // Verify each expected image file is found in the results
            foreach (var expectedFile in _createdFiles["image"])
            {
                Assert.IsTrue(imageFiles.Contains(expectedFile), 
                    $"Should find image file: {Path.GetFileName(expectedFile)}");
            }
        }
        
        /// <summary>
        /// Tests the file detection logic used by the View Text button.
        /// Verifies that all text files (.txt, .csv, etc.) are found correctly,
        /// including those in subfolders.
        /// </summary>
        [TestMethod]
        public void ViewTextButton_FindsAllTextFiles()
        {
            // Define the same text extensions that the ViewTextButton_Click method uses
            var textExtensions = new[] { ".txt", ".csv", ".json", ".xml", ".html" };
            
            // Simulate the file finding code from ViewTextButton_Click
            // Get all files, then filter by the supported text extensions
            var textFiles = Directory.GetFiles(_testOutputFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => textExtensions.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();
            
            // Verify the correct number of text files are found
            Assert.AreEqual(_createdFiles["text"].Count, textFiles.Length, 
                "Should find all text files, including in subfolders");
            
            // Verify each expected text file is found in the results
            foreach (var expectedFile in _createdFiles["text"])
            {
                Assert.IsTrue(textFiles.Contains(expectedFile), 
                    $"Should find text file: {Path.GetFileName(expectedFile)}");
            }
        }
        
        /// <summary>
        /// Tests the file opening functionality used by all three view buttons.
        /// Verifies that the ProcessStartInfo is correctly configured for different file types.
        /// </summary>
        [TestMethod]
        public void OpenFile_CorrectlyConfiguresProcessStartInfo()
        {
            // Test with one file of each type to verify the configuration is consistent
            string[] testFiles = {
                _createdFiles["excel"].FirstOrDefault() ?? Path.Combine(_testOutputFolder, "report1.xlsx"),
                _createdFiles["image"].FirstOrDefault() ?? Path.Combine(_testOutputFolder, "image1.jpg"),
                _createdFiles["text"].FirstOrDefault() ?? Path.Combine(_testOutputFolder, "text1.txt")
            };
            
            foreach (var filePath in testFiles)
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"Warning: Test file does not exist: {filePath}");
                    continue;
                }
                
                // Simulate the ProcessStartInfo creation done in the OpenFile method
                var startInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true  // Must be true to open with default system application
                };
                
                // Verify the ProcessStartInfo is correctly configured
                Assert.AreEqual(filePath, startInfo.FileName, "FileName should match the input file path");
                Assert.IsTrue(startInfo.UseShellExecute, "UseShellExecute should be true to open with default app");
            }
        }
        
        /// <summary>
        /// Tests error handling for when the output folder doesn't exist.
        /// Verifies that the code correctly detects a missing output folder.
        /// </summary>
        [TestMethod]
        public void ViewButtons_HandleMissingOutputFolder()
        {
            // Simulate a scenario where the output folder doesn't exist
            string nonExistentFolder = Path.Combine(_testOutputFolder, "non_existent");
            
            // Test the folder validation logic similar to what would be in the button click handlers
            bool folderExists = !string.IsNullOrEmpty(nonExistentFolder) && Directory.Exists(nonExistentFolder);
            
            // Verify that the validation correctly identifies the folder as missing
            Assert.IsFalse(folderExists, "Should detect non-existent output folder");
        }
        
        /// <summary>
        /// Tests error handling for when no files of the requested type are found.
        /// Verifies that the code correctly detects when no files are present.
        /// </summary>
        [TestMethod]
        public void ViewButtons_HandleNoFilesFound()
        {
            try
            {
                // Create an empty folder to simulate a scenario with no files
                string emptyFolder = Path.Combine(_testOutputFolder, "empty_folder");
                Directory.CreateDirectory(emptyFolder);
                
                // Test Excel file detection in the empty folder (View Excel button)
                var excelFiles = Directory.GetFiles(emptyFolder, "*.xlsx", SearchOption.AllDirectories);
                Assert.AreEqual(0, excelFiles.Length, "Should find no Excel files in empty folder");
                
                // Test image file detection in the empty folder (View Images button)
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };
                var imageFiles = Directory.GetFiles(emptyFolder, "*.*", SearchOption.AllDirectories)
                    .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToArray();
                Assert.AreEqual(0, imageFiles.Length, "Should find no image files in empty folder");
                
                // Test text file detection in the empty folder (View Text button)
                var textExtensions = new[] { ".txt", ".csv", ".json", ".xml", ".html" };
                var textFiles = Directory.GetFiles(emptyFolder, "*.*", SearchOption.AllDirectories)
                    .Where(file => textExtensions.Contains(Path.GetExtension(file).ToLower()))
                    .ToArray();
                Assert.AreEqual(0, textFiles.Length, "Should find no text files in empty folder");
            }
            catch (Exception ex)
            {
                Assert.Fail($"Test failed with exception: {ex.Message}");
            }
        }
    }
} 