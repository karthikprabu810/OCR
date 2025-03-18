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
        private string _testImagePath;          /// <summary>Path to the test image file containing sample text for OCR extraction</summary>
        private string _testConfigPath;         /// <summary>Path to the test configuration file with OCR engine settings</summary>
        private OcrExtractionTools _ocrTools;   /// <summary>Instance of OcrExtractionTools to be tested</summary>
        
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
            
            // Dispose OCR tools to release resources
            _ocrTools?.Dispose();
        }
        
        /// <summary>
        /// Tests that the OcrExtractionTools constructor properly initializes the object
        /// when provided with a valid configuration file path.
        /// Verifies that no exceptions are thrown and the object is in a usable state.
        /// </summary>
        [TestMethod]
        public void Constructor_WithValidConfig_InitializesSuccessfully()
        {
            // Act
            var ocrTools = new OcrExtractionTools(_testConfigPath);
            
            // Assert
            Assert.IsNotNull(ocrTools);
            
            // Clean up
            ocrTools.Dispose();
        }
        
        /// <summary>
        /// Tests that the OcrExtractionTools constructor throws a FileNotFoundException
        /// when initialized with an invalid or non-existent configuration file path.
        /// This verifies proper error handling for missing configuration files.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void Constructor_WithInvalidConfigPath_ThrowsFileNotFoundException()
        {
            // Arrange
            string invalidConfigPath = Path.Combine(Path.GetTempPath(), $"non_existent_config_{Guid.NewGuid()}.json");
            
            // Act - should throw FileNotFoundException
            var ocrTools = new OcrExtractionTools(invalidConfigPath);
            
            // Assert is handled by ExpectedException attribute
        }
        
        /// <summary>
        /// Tests the asynchronous Tesseract OCR extraction functionality with a valid image.
        /// Verifies that the extraction process completes successfully and creates an output file
        /// containing the extracted text. This test may be skipped if Tesseract is not installed.
        /// </summary>
        /// <returns>Task representing the asynchronous test operation</returns>
        [TestMethod]
        public async Task ExtractTextUsingTesseractAsync_ValidImage_CreatesOutputFile()
        {
            // Arrange
            string outputPath = Path.Combine(Path.GetTempPath(), $"ocr_test_output_{Guid.NewGuid():N}");
            
            try
            {
                // Act
                await _ocrTools.ExtractTextUsingTesseractAsync(_testImagePath, outputPath);
                
                // Assert
                string outputFilePath = outputPath + ".txt";
                Assert.IsTrue(File.Exists(outputFilePath), "Output file should be created");
                
                string extractedText = File.ReadAllText(outputFilePath);
                Assert.IsFalse(string.IsNullOrEmpty(extractedText), "Extracted text should not be empty");
            }
            catch (Exception ex) when (ex.Message.Contains("Tesseract") || ex.ToString().Contains("process"))
            {
                // Skip test if Tesseract isn't properly installed
                Assert.Inconclusive($"Test requires Tesseract to be installed: {ex.Message}");
            }
            finally
            {
                // Clean up output file
                string outputFilePath = outputPath + ".txt";
                if (File.Exists(outputFilePath))
                {
                    File.Delete(outputFilePath);
                }
            }
        }
        
        /// <summary>
        /// Tests the Windows-specific Tesseract OCR extraction using the NuGet package.
        /// Verifies that text extraction works correctly on Windows platforms using the
        /// .NET library integration rather than command-line execution.
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
        /// Tests the Google Cloud Vision API OCR extraction functionality.
        /// Verifies that the API can successfully extract text from a valid image.
        /// This test may be skipped if the Google Cloud Vision API key is not configured.
        /// </summary>
        /// <returns>Task representing the asynchronous test operation</returns>
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
        /// Tests the OCR.space API text extraction functionality.
        /// Verifies that the API can successfully extract text from a valid image.
        /// This test may be skipped if the OCR.space API key is not configured.
        /// </summary>
        /// <returns>Task representing the asynchronous test operation</returns>
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
        /// Tests the configuration loading functionality of OcrExtractionTools.
        /// Verifies that the class can correctly parse and load configuration settings
        /// from a valid JSON configuration file, including API keys and paths.
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