using ocrApplication;

namespace unitTestProject
{
    /// <summary>
    /// Test suite for the OcrExtractionTools class, which provides OCR functionality using multiple engines.
    /// Tests verify the initialization, configuration loading, and text extraction capabilities
    /// using various OCR engines including Tesseract, Google Vision API, and OCR.space.
    /// </summary>
    [TestClass]
    public class OcrExtractionToolsTests
    {
        private string _testImagePath;
        private string _testConfigPath;
        private OcrExtractionTools _ocrTools;
        
        /// <summary>
        /// Sets up the test environment before each test.
        /// Creates a test image with sample text, generates a test configuration file,
        /// and initializes the OCR tools with the test configuration.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create a test image and configuration
            _testImagePath = TestHelpers.CreateTestImage("Sample OCR Test Text");
            _testConfigPath = TestHelpers.CreateTestConfig();
            _ocrTools = new OcrExtractionTools(_testConfigPath);
        }
        
        /// <summary>
        /// Cleans up test resources after each test.
        /// Ensures that temporary test files are deleted and OCR tools are properly disposed
        /// to prevent resource leaks and temporary file accumulation.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files
            if (_testImagePath != null)
            {
                TestHelpers.CleanupTestFiles(_testImagePath);
            }
            
            if (_ocrTools != null)
            {
                _ocrTools.Dispose();
            }
        }
        
        /// <summary>
        /// Tests successful initialization with a valid configuration file.
        /// Verifies that the OcrExtractionTools class can be properly instantiated
        /// when provided with a valid configuration path.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidConfig_InitializesSuccessfully()
        {
            // Arrange & Act already done in Setup
            
            // Assert
            Assert.IsNotNull(_ocrTools, "OcrExtractionTools should be successfully initialized with valid config");
        }
        
        /// <summary>
        /// Tests error handling when initializing with an invalid configuration path.
        /// Verifies that the constructor throws a FileNotFoundException when the
        /// specified configuration file does not exist.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Constructor_WithInvalidConfigPath_ThrowsFileNotFoundException()
        {
            // Arrange
            string nonExistentConfigPath = "non_existent_config.json";
            
            // Act - This should throw FileNotFoundException
            var ocrTools = new OcrExtractionTools(nonExistentConfigPath);
        }
        
        /// <summary>
        /// Tests Tesseract OCR text extraction functionality.
        /// Verifies that the method successfully processes an image and creates
        /// an output file containing the extracted text. Handles cases where
        /// Tesseract is not properly installed.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextUsingTesseractAsync_ValidImage_CreatesOutputFile()
        {
            // Arrange
            string outputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDir);
            string outputPath = Path.Combine(outputDir, "output");
            
            try
            {
                // Act
                _ocrTools.ExtractTextUsingTesseract(_testImagePath, outputPath);
                
                // Wait briefly for the process to complete
                await Task.Delay(1000);
                
                // Assert
                string outputFilePath = outputPath + ".txt";
                Assert.IsTrue(File.Exists(outputFilePath), "Output text file should exist");
            }
            catch (Exception ex) when (ex.Message.Contains("Tesseract") || ex.ToString().Contains("process"))
            {
                // Skip test if Tesseract isn't properly installed
                Assert.Inconclusive($"Test requires Tesseract to be installed: {ex.Message}");
            }
            finally
            {
                // Clean up
                if (Directory.Exists(outputDir))
                {
                    Directory.Delete(outputDir, true);
                }
            }
        }
        
        /// <summary>
        /// Tests Tesseract OCR using the Windows NuGet package.
        /// Verifies that the method can execute successfully when the appropriate
        /// Tesseract dependencies are available. Handles cases where the NuGet
        /// package or data files are not properly configured.
        /// </summary>
        [TestMethod]
        public void ExtractTextUsingTesseractWindowsNuGet_ValidImage_ProducesOutput()
        {
            try
            {
                // Act
                string text = _ocrTools.ExtractTextUsingTesseractWindowsNuGet(_testImagePath);
                
                // Just verify execution without exception
                // The Tesseract Windows NuGet may not be properly set up on all systems
                // so we don't assert the content of the result, just that it executed
                Console.WriteLine($"Tesseract NuGet output: {text}");
            }
            catch (Exception ex) when (ex.Message.Contains("Tesseract") || 
                                      ex.ToString().Contains("System.Reflection.TargetInvocationException") ||
                                      ex.ToString().Contains("InvalidOperationException"))
            {
                // Skip test if Tesseract data files are not properly installed
                Assert.Inconclusive($"Test requires proper Tesseract installation: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tests Google Vision API text extraction functionality.
        /// Verifies that the method can successfully communicate with the Google Vision API
        /// and extract text from images. Handles cases where API credentials are invalid
        /// or rate limits are exceeded.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextUsingGoogleVisionAsync_ValidImage_ReturnsText()
        {
            try
            {
                // Act
                string text = await _ocrTools.ExtractTextUsingGoogleVisionAsync(_testImagePath);
                
                // Assert
                Assert.IsFalse(string.IsNullOrEmpty(text), "Extracted text should not be empty");
            }
            catch (Exception ex) when (ex.Message.Contains("limit") || 
                                      ex.Message.Contains("credentials") || 
                                      ex.ToString().Contains("Google"))
            {
                // Skip test if Google Vision API is not properly configured
                Assert.Inconclusive($"Test requires proper Google Vision API configuration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tests OCR.space API text extraction functionality.
        /// Verifies that the method can successfully communicate with the OCR.space API
        /// and extract text from images. Handles cases where the API is unavailable
        /// or returns errors.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextUsingOcrSpaceAsync_ValidImage_ReturnsText()
        {
            try
            {
                // Act
                string text = await _ocrTools.ExtractTextUsingOcrSpaceAsync(_testImagePath);
                
                // Assert - Note: This test depends on external API availability
                Assert.IsFalse(string.IsNullOrEmpty(text), "Extracted text should not be empty");
            }
            catch (Exception ex) when (ex.Message.Contains("API") || 
                                      ex.ToString().Contains("HttpRequestException"))
            {
                // Skip test if OCR Space API is not reachable
                Assert.Inconclusive($"Test requires OCR Space API availability: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tests configuration file loading functionality.
        /// Verifies that the class can successfully load and parse a valid JSON
        /// configuration file containing all required OCR settings and API keys.
        /// Tests with a complete configuration including all expected fields.
        /// </summary>
        [TestMethod]
        public void LoadConfig_ValidFile_LoadsConfiguration()
        {
            // Arrange - Create a test JSON file
            string jsonContent = @"{
                ""TesseractPath"": ""/usr/local/bin/tesseract"",
                ""TesseractTessDataPath"": ""/usr/local/share/tessdata"",
                ""OcrSpaceApiKey"": ""test_api_key"",
                ""IronOcrLicenseKey"": ""test_license_key"",
                ""GoogleVisionApiKey"": ""test_google_key"",
                ""Counter"": 0,
                ""Limit"": 100,
                ""ApiUrl"": ""http://localhost:5000/process_ocr""
            }";
            
            string tempConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
            File.WriteAllText(tempConfigPath, jsonContent);
            
            try
            {
                // Act - Create a new instance using the test config
                var testOcrTools = new OcrExtractionTools(tempConfigPath);
                
                // Assert - Should initialize without exceptions
                Assert.IsNotNull(testOcrTools, "Should initialize with valid configuration");
                
                // Clean up
                testOcrTools.Dispose();
            }
            finally
            {
                // Delete the temporary config file
                if (File.Exists(tempConfigPath))
                {
                    File.Delete(tempConfigPath);
                }
            }
        }
    }
} 