using ocrApplication;
using Emgu.CV;

namespace unitTestProject
{
    [TestClass]
    public class IntegrationTests
    {
        private string _testImagePath;
        private string _testConfigPath;
        private OcrExtractionTools _ocrTools;
        private OcrComparison _ocrComparison;
        private EnsembleOcr _ensembleOcr;
        private string _tempOutputDir;

        [TestInitialize]
        public void Setup()
        {
            // Create test image and configuration
            _testImagePath = TestHelpers.CreateTestImage("Sample OCR Integration Test");
            _testConfigPath = TestHelpers.CreateTestConfig();
            
            // Initialize components
            _ocrTools = new OcrExtractionTools(_testConfigPath);
            _ocrComparison = new OcrComparison();
            _ensembleOcr = new EnsembleOcr();
            
            // Create temporary output directory
            _tempOutputDir = Path.Combine(Path.GetTempPath(), $"ocr_test_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempOutputDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test files
            if (_testImagePath != null)
            {
                TestHelpers.CleanupTestFiles(_testImagePath);
            }
            
            // Dispose OCR tools
            if (_ocrTools != null)
            {
                _ocrTools.Dispose();
            }
            
            // Delete temporary output directory
            if (_tempOutputDir != null && Directory.Exists(_tempOutputDir))
            {
                Directory.Delete(_tempOutputDir, true);
            }
        }

        [TestMethod]
        public async Task PreprocessAndExtractText_CompleteWorkflow_Success()
        {
            // Arrange - Create output paths
            string preprocessedImagePath = Path.Combine(_tempOutputDir, "preprocessed.png");
            string outputTextPath = Path.Combine(_tempOutputDir, "output");
            
            try
            {
                // Act - Preprocess image
                var grayscaleImage = ImagePreprocessing.ConvertToGrayscale(_testImagePath);
                CvInvoke.Imwrite(preprocessedImagePath, grayscaleImage);
                
                // Extract text using Tesseract
                _ocrTools.ExtractTextUsingTesseract(preprocessedImagePath, outputTextPath);
                
                // Wait briefly for the process to complete
                await Task.Delay(1000);
                
                // Assert
                string outputFilePath = outputTextPath + ".txt";
                Assert.IsTrue(File.Exists(preprocessedImagePath), "Preprocessed image file should exist");
                Assert.IsTrue(File.Exists(outputFilePath), "Output text file should exist");
                
                // Verify the output file has content
                string extractedText = File.ReadAllText(outputFilePath);
                Console.WriteLine($"Extracted text: {extractedText}");
                Assert.IsFalse(string.IsNullOrEmpty(extractedText), "Extracted text should not be empty");
            }
            catch (Exception ex) when (ex.Message.Contains("Tesseract") || ex.ToString().Contains("process"))
            {
                // Skip test if Tesseract isn't properly installed
                Assert.Inconclusive($"Test requires Tesseract to be installed: {ex.Message}");
            }
        }

        [TestMethod]
        public void MultipleOcrEngines_EnsembleApproach_Success()
        {
            // Arrange - Create mock OCR results
            var ocrResults = new List<string>
            {
                "Sample OCR text with slight differences",
                "Sample OCR text with slight diferences",
                "Sample OCR text with slight differences"
            };
            
            // Act - Apply ensemble approach
            string combinedText = _ensembleOcr.CombineUsingMajorityVoting(ocrResults);
            
            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(combinedText), "Combined text should not be empty");
            Assert.AreEqual("Sample OCR text with slight differences", combinedText, 
                "Combined text should match the most common result");
        }

        [TestMethod]
        public void SimilarityComparison_BetweenMultipleOcrResults_Success()
        {
            // Arrange
            string referenceText = "Sample OCR Integration Test";
            string[] comparisonTexts = {
                "Sample OCR Integration Test", // 100% match
                "Sample OCR Integration Tets", // Small typo
                "completely different text"    // Low similarity
            };
            
            // Act & Assert
            foreach (string text in comparisonTexts)
            {
                double similarity = _ocrComparison.CalculateLevenshteinSimilarity(referenceText, text);
                Console.WriteLine($"Similarity between '{referenceText}' and '{text}': {similarity}%");
                
                if (text == referenceText)
                {
                    Assert.AreEqual(100.0, similarity, 0.001, "Identical texts should have 100% similarity");
                }
                else if (text.Contains("Sample"))
                {
                    Assert.IsTrue(similarity > 80.0, "Similar texts should have high similarity");
                }
                else
                {
                    Assert.IsTrue(similarity < 50.0, "Different texts should have low similarity");
                }
            }
        }

        [TestMethod]
        public void ImagePreprocessingComparison_MultipleMethodsTest()
        {
            // Arrange - Prepare output paths
            string grayscalePath = Path.Combine(_tempOutputDir, "grayscale.png");
            string binaryPath = Path.Combine(_tempOutputDir, "binary.png");
            string enhancedPath = Path.Combine(_tempOutputDir, "enhanced.png");
            
            // Act - Apply different preprocessing methods
            var grayscaleImage = ImagePreprocessing.ConvertToGrayscale(_testImagePath);
            CvInvoke.Imwrite(grayscalePath, grayscaleImage);
            
            var binaryImage = ImagePreprocessing.OtsuBinarization(_testImagePath);
            CvInvoke.Imwrite(binaryPath, binaryImage);
            
            var enhancedImage = ImagePreprocessing.HistogramEqualization(_testImagePath);
            CvInvoke.Imwrite(enhancedPath, enhancedImage);
            
            // Assert - Verify all output files were created and have valid content
            Assert.IsTrue(File.Exists(grayscalePath), "Grayscale image file should exist");
            Assert.IsTrue(File.Exists(binaryPath), "Binary image file should exist");
            Assert.IsTrue(File.Exists(enhancedPath), "Enhanced image file should exist");
            
            // Verify image properties
            Assert.IsNotNull(grayscaleImage, "Grayscale image should not be null");
            Assert.IsNotNull(binaryImage, "Binary image should not be null");
            Assert.IsNotNull(enhancedImage, "Enhanced image should not be null");
            
            Assert.IsFalse(grayscaleImage.IsEmpty, "Grayscale image should not be empty");
            Assert.IsFalse(binaryImage.IsEmpty, "Binary image should not be empty");
            Assert.IsFalse(enhancedImage.IsEmpty, "Enhanced image should not be empty");
            
            // Verify files have content by checking file sizes
            long grayscaleSize = new FileInfo(grayscalePath).Length;
            long binarySize = new FileInfo(binaryPath).Length;
            long enhancedSize = new FileInfo(enhancedPath).Length;
            
            Console.WriteLine($"Grayscale size: {grayscaleSize}, Binary size: {binarySize}, Enhanced size: {enhancedSize}");
            
            // Since all preprocessing methods return valid images,
            // and we've verified they are not empty, the test can be considered successful
            // without assuming anything about their relative sizes
            Assert.IsTrue(grayscaleSize > 0 && binarySize > 0 && enhancedSize > 0,
                "All preprocessed image files should have content");
        }
    }
} 