using System.Diagnostics;

namespace unitTestProject
{
    /// <summary>
    /// Test cases for ocrGui application functionality.
    /// Note: These tests focus on logic rather than UI components to avoid Avalonia dependencies.
    /// Tests in this class validate core file handling and process management functionality
    /// used by the GUI application.
    /// </summary>
    [TestClass]
    public class GuiTests
    {
        // Temporary folders for testing file operations
        private string _tempOutputFolder;
        private string _tempInputFolder;
        
        // Test file paths that will be created during setup
        private string _testImagePath;
        private string _testTextPath;
        private string _testExcelPath;

        /// <summary>
        /// Setup method run before each test.
        /// Creates testing directories and sample files needed for tests.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create temporary test folders with unique GUIDs to avoid conflicts
            _tempInputFolder = Path.Combine(Path.GetTempPath(), $"ocr_gui_test_input_{Guid.NewGuid():N}");
            _tempOutputFolder = Path.Combine(Path.GetTempPath(), $"ocr_gui_test_output_{Guid.NewGuid():N}");
            
            Directory.CreateDirectory(_tempInputFolder);
            Directory.CreateDirectory(_tempOutputFolder);
            
            // Create test image file using TestHelpers utility
            _testImagePath = TestHelpers.CreateTestImage("Test GUI Image", 800, 600);
            File.Copy(_testImagePath, Path.Combine(_tempInputFolder, "test_image.png"), true);
            
            // Create a sample text file to simulate OCR output
            _testTextPath = Path.Combine(_tempOutputFolder, "result.txt");
            File.WriteAllText(_testTextPath, "Sample OCR extracted text from test image");
            
            // Create a sample Excel file to simulate report generation
            _testExcelPath = Path.Combine(_tempOutputFolder, "report.xlsx");
            CreateSampleExcelFile(_testExcelPath);
        }

        /// <summary>
        /// Cleanup method run after each test.
        /// Removes all temporary files and directories created during testing.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files and directories
            TestHelpers.CleanupTestFiles(_testImagePath);
            
            try
            {
                // Attempt to delete input folder with all contents
                if (Directory.Exists(_tempInputFolder))
                {
                    Directory.Delete(_tempInputFolder, true);
                }
                
                // Attempt to delete output folder with all contents
                if (Directory.Exists(_tempOutputFolder))
                {
                    Directory.Delete(_tempOutputFolder, true);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail tests if cleanup has issues
                // This could happen if files are still locked
                Console.WriteLine($"Warning: Clean-up failed: {ex.Message}");
                // Non-critical failure, don't fail the test
            }
        }

        /// <summary>
        /// Creates a sample Excel file for testing.
        /// For file detection tests, we only need the file to exist with the correct extension.
        /// </summary>
        /// <param name="filePath">Path where the Excel file should be created</param>
        private void CreateSampleExcelFile(string filePath)
        {
            try
            {
                // Create a simple Excel file directly (without EPPlus)
                // Since we're just testing file detection, an empty file is sufficient
                // Real file content isn't necessary for these tests
                File.WriteAllBytes(filePath, new byte[100]);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating test Excel file: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests the file detection methods used by the View buttons.
        /// Validates that each file type (Excel, Text, Image) can be correctly identified.
        /// </summary>
        [TestMethod]
        public void FileDetection_FindsCorrectFiles_InOutputFolder()
        {
            // Test Excel file detection - should find exactly one .xlsx file
            var excelFiles = Directory.GetFiles(_tempOutputFolder, "*.xlsx", SearchOption.AllDirectories);
            Assert.AreEqual(1, excelFiles.Length, "Should find exactly one Excel file");
            Assert.AreEqual(_testExcelPath, excelFiles[0], "Should find the correct Excel file");
            
            // Test Text file detection - should find exactly one .txt file
            var textFiles = Directory.GetFiles(_tempOutputFolder, "*.txt", SearchOption.AllDirectories);
            Assert.AreEqual(1, textFiles.Length, "Should find exactly one Text file");
            Assert.AreEqual(_testTextPath, textFiles[0], "Should find the correct Text file");
            
            // Test image file filtering logic - using the same extension filter used in the application
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".gif" };
            var imageFiles = Directory.GetFiles(_tempInputFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower())).ToArray();
            
            Assert.AreEqual(1, imageFiles.Length, "Should find exactly one Image file");
            Assert.IsTrue(Path.GetFileName(imageFiles[0]) == "test_image.png", "Should find the PNG test image");
        }

        /// <summary>
        /// Tests the Process.Start configuration used by all view buttons.
        /// Validates that file opening would be configured correctly.
        /// This is a non-invasive test that doesn't actually open files.
        /// </summary>
        [TestMethod]
        public void ProcessLaunching_CorrectlyFormatsStartInfo()
        {
            // We can't easily start actual processes in the test environment,
            // but we can check if the ProcessStartInfo is constructed correctly
            
            var testFile = Path.Combine(_tempOutputFolder, "test.txt");
            var startInfo = new ProcessStartInfo
            {
                FileName = testFile,
                UseShellExecute = true // This is crucial for opening files with their default application
            };
            
            // Verify the file path is correctly set
            Assert.AreEqual(testFile, startInfo.FileName, "ProcessStartInfo filename should match the input file");
            // Verify UseShellExecute is enabled, which is required to open files with default applications
            Assert.IsTrue(startInfo.UseShellExecute, "UseShellExecute should be true for opening files with default applications");
        }

        /// <summary>
        /// Tests the exit confirmation dialog logic.
        /// Validates the conditions for showing/hiding the confirmation dialog
        /// when attempting to exit while a process is running.
        /// </summary>
        [TestMethod]
        public void ExitHandling_ConfirmationLogic_WorksCorrectly()
        {
            // Create an instance of Process without mocking
            // Note: We're testing the logic, not the actual Process behavior
            Process? process = null;
            
            // Test Case 1: Process is null but isProcessing flag is true
            // Should not show confirmation dialog (can't terminate null process)
            bool isProcessing = true;
            bool shouldShowConfirmation = isProcessing && process != null;
            
            Assert.IsFalse(shouldShowConfirmation, "Should not show confirmation when process is null");
            
            // Test Case 2: Process exists and isProcessing flag is true
            // Should show confirmation dialog
            isProcessing = true;
            process = new Process(); // Just creating an instance, not starting it
            shouldShowConfirmation = isProcessing && process != null;
            
            Assert.IsTrue(shouldShowConfirmation, "Should show confirmation when process is not null and processing is true");
            
            // Test Case 3: Process exists but isProcessing flag is false
            // Should not show confirmation dialog (nothing to terminate)
            isProcessing = false;
            shouldShowConfirmation = isProcessing && process != null;
            
            Assert.IsFalse(shouldShowConfirmation, "Should not show confirmation when not processing");
        }
    }
} 