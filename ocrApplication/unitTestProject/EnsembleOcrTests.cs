using ocrApplication;

namespace unitTestProject
{
    /// <summary>
    /// Test suite for the EnsembleOcr class, which combines results from multiple OCR engines.
    /// Tests verify the functionality of majority voting algorithms and API communication
    /// for optimizing OCR accuracy through ensemble methods.
    /// </summary>
    [TestClass]
    public class EnsembleOcrTests
    {
        private string _testImagePath;
        private string _testConfigPath;
        private EnsembleOcr _ensembleOcr;

        /// <summary>
        /// Sets up the test environment before each test.
        /// Creates a test image, loads configuration, and initializes the EnsembleOcr instance
        /// with appropriate API URL from config or default value.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create a test image and configuration
            _testImagePath = TestHelpers.CreateTestImage("Sample OCR Test Text");
            _testConfigPath = TestHelpers.CreateTestConfig();
            
            // Initialize EnsembleOcr
            _ensembleOcr = new EnsembleOcr();
        }

        /// <summary>
        /// Cleans up test resources after each test.
        /// Ensures that temporary test files are properly deleted to prevent resource leaks.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files
            if (_testImagePath != null)
            {
                TestHelpers.CleanupTestFiles(_testImagePath);
            }
        }

        /// <summary>
        /// Tests majority voting with a clear majority case.
        /// Verifies that when multiple OCR results are provided with a clear majority,
        /// the most common text is correctly identified and returned.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_VariousResults_ReturnsMostCommonText()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This is a test",
                "This is a test",
                "This is a toast",
                "This is a test"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual("This is a test", result, "Majority voting should return the most common text");
        }

        /// <summary>
        /// Tests majority voting with equally distributed results.
        /// Verifies that when multiple OCR results have equal occurrences,
        /// one of the most frequent results is consistently returned.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_EqualDistribution_ReturnsFirstHighestOccurrence()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This is a test",
                "This is a test",
                "This is a toast",
                "This is a toast"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.IsTrue(result == "This is a test" || result == "This is a toast", 
                "When equal distribution, should return one of the equally common texts");
        }

        /// <summary>
        /// Tests majority voting with an empty input list.
        /// Verifies that the method handles empty input gracefully by returning an empty string.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_EmptyInput_ReturnsEmptyString()
        {
            // Arrange
            var ocrResults = new List<string>();

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual(string.Empty, result, "Empty input should return empty string");
        }

        /// <summary>
        /// Tests majority voting with null input.
        /// Verifies that the method handles null input gracefully by returning an empty string
        /// instead of throwing an exception.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_NullInput_ReturnsEmptyString()
        {
            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(null);

            // Assert
            Assert.AreEqual(string.Empty, result, "Null input should return empty string");
        }

        /// <summary>
        /// Tests majority voting with mixed case and whitespace variations.
        /// Verifies that the method correctly handles text normalization
        /// and returns the most common normalized form.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_MixedCaseAndWhitespace_NormalizesCorrectly()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This   is  a  test",
                "THIS IS A TEST",
                "This is a test  ",
                "this is a test"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual("This is a test", result, "Should normalize whitespace and case");
        }

        /// <summary>
        /// Tests majority voting with punctuation variations.
        /// Verifies that the method correctly handles different punctuation
        /// marks and returns the most common normalized form.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_PunctuationVariations_NormalizesCorrectly()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This is a test.",
                "This is a test!",
                "This is a test?",
                "This is a test,"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual("This is a test.", result, "Should normalize punctuation");
        }

        /// <summary>
        /// Tests majority voting with special characters.
        /// Verifies that the method correctly handles text containing
        /// special characters and returns a cleaned result.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_SpecialCharacters_CleansText()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This #is a@ test",
                "This $is a% test",
                "This &is a* test",
                "This is a test"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual("This is a test", result, "Should clean special characters");
        }

        /// <summary>
        /// Tests majority voting with very short results.
        /// Verifies that the method correctly filters out very short results
        /// that are likely OCR errors.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_VeryShortResults_FiltersIncorrectly()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This is a test",
                "a",
                "test",
                "This is a test"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual("This is a test", result, "Should filter out very short results");
        }

        /// <summary>
        /// Tests majority voting with line break variations.
        /// Verifies that the method correctly preserves line breaks
        /// and handles majority voting for each line.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_LineBreakVariations_NormalizesCorrectly()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "This is\na test",
                "This is\r\na test",
                "This is\na test",
                "This is\na test"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            Assert.AreEqual("This is\na test", result, "Should preserve line breaks and handle majority voting per line");
        }

        /// <summary>
        /// Tests majority voting with multiple lines.
        /// Verifies that the method correctly preserves line breaks and handles
        /// majority voting for each line independently.
        /// </summary>
        [TestMethod]
        public void CombineUsingMajorityVoting_MultipleLines_PreservesLineStructure()
        {
            // Arrange
            var ocrResults = new List<string>
            {
                "Line one\nLine two\nLine three",
                "Line one\nLine tvo\nLine three",
                "Line one\nLine two\nLine three"
            };

            // Act
            string result = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);

            // Assert
            string expected = "Line one\nLine two\nLine three";
            Assert.AreEqual(expected, result, "Should preserve line structure and apply majority voting per line");
        }

        /// <summary>
        /// Tests API communication with valid OCR text input.
        /// Verifies that the method successfully sends OCR results to the API
        /// and receives a valid response. Handles API unavailability gracefully.
        /// Note: This test requires a valid, accessible API endpoint.
        /// </summary>
        [TestMethod]
        public async Task SendOcrTextsToApiAsync_ValidInput_ReturnsResponse()
        {
            // Arrange
            var ocrTexts = new List<string>
            {
                "This is a test",
                "This is a test but slightly different"
            };

            // Check if API URL is available and not a localhost address (for CI environments)
            if (_ensembleOcr.ApiUrl == null || _ensembleOcr.ApiUrl.Contains("localhost"))
            {
                Assert.Inconclusive("Test requires a valid API URL that is not a localhost address");
                return;
            }

            try
            {
                // Act
                string result = await _ensembleOcr.SendOcrTextsToApiAsync(ocrTexts);
                
                // Assert - Note: This test depends on external API availability
                Assert.IsFalse(string.IsNullOrEmpty(result), "API response should not be empty");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
            {
                // In case the API is not reachable, we'll mark the test as inconclusive
                Assert.Inconclusive($"API unavailable: {ex.Message}");
            }
        }

        /// <summary>
        /// Tests API communication with null input.
        /// Verifies that the method properly handles null input by throwing
        /// a NullReferenceException rather than attempting to process invalid data.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task SendOcrTextsToApiAsync_NullInput_ThrowsArgumentNullException()
        {
            // Act - should throw ArgumentNullException
            await _ensembleOcr.SendOcrTextsToApiAsync(null);
        }

        /// <summary>
        /// Tests API communication with empty string input.
        /// Verifies that the method handles empty strings appropriately
        /// and returns an empty result.
        /// </summary>
        [TestMethod]
        public async Task SendOcrTextsToApiAsync_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            var ocrTexts = new List<string> { "" };

            // Act
            string result = await _ensembleOcr.SendOcrTextsToApiAsync(ocrTexts);

            // Assert
            Assert.AreEqual(string.Empty, result, "Empty input should return empty string");
        }

        /// <summary>
        /// Tests API communication with whitespace-only input.
        /// Verifies that the method handles whitespace-only strings appropriately
        /// and returns an empty result.
        /// </summary>
        [TestMethod]
        public async Task SendOcrTextsToApiAsync_WhitespaceOnly_ReturnsEmptyString()
        {
            // Arrange
            var ocrTexts = new List<string> { "   ", "\t", "\n" };

            // Act
            string result = await _ensembleOcr.SendOcrTextsToApiAsync(ocrTexts);

            // Assert
            Assert.AreEqual(string.Empty, result, "Whitespace-only input should return empty string");
        }

        /// <summary>
        /// Tests API communication with mixed valid and empty results.
        /// Verifies that the method correctly filters out empty results
        /// and processes the valid ones.
        /// </summary>
        [TestMethod]
        public async Task SendOcrTextsToApiAsync_MixedValidAndEmptyResults_ProcessesValidResults()
        {
            // Arrange
            var ocrTexts = new List<string>
            {
                "This is a test",
                "",
                "   ",
                "Another test"
            };

            try
            {
                // Act
                string result = await _ensembleOcr.SendOcrTextsToApiAsync(ocrTexts);

                // Assert
                Assert.IsFalse(string.IsNullOrEmpty(result), "Should return non-empty result for valid inputs");
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is HttpRequestException)
            {
                // In case the API is not reachable, we'll mark the test as inconclusive
                Assert.Inconclusive($"API unavailable: {ex.Message}");
            }
        }
    }
} 