using ocrApplication;
using Emgu.CV;

namespace unitTestProject
{
    /// <summary>
    /// Test suite for the ImagePreprocessing class, which provides various image processing techniques
    /// to enhance OCR accuracy. Tests verify the functionality of grayscale conversion, binarization,
    /// noise reduction, and other image enhancement methods.
    /// </summary>
    [TestClass]
    public class ImagePreprocessingTests
    {
        private string _testImagePath;
        
        /// <summary>
        /// Sets up the test environment before each test.
        /// Creates a test image with sample text that will be used by the image processing tests.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // Create a test image for preprocessing
            _testImagePath = TestHelpers.CreateTestImage("Sample OCR Test Text");
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
        /// Tests the grayscale conversion functionality.
        /// Verifies that the output image is properly converted to grayscale format
        /// by checking its number of channels and ensuring it's not empty.
        /// </summary>
        [TestMethod]
        public void ConvertToGrayscale_ValidImage_ReturnsGrayscaleImage()
        {
            // Act
            var grayscaleImage = ImagePreprocessing.ConvertToGrayscale(_testImagePath);
            
            // Assert
            Assert.IsNotNull(grayscaleImage, "Grayscale image should not be null");
            Assert.IsFalse(grayscaleImage.IsEmpty, "Grayscale image should not be empty");
            
            // Verify it's actually grayscale (1 channel)
            Assert.AreEqual(1, grayscaleImage.NumberOfChannels, "Grayscale image should have 1 channel");
        }
        
        /// <summary>
        /// Tests Otsu's binarization method.
        /// Verifies that the method successfully converts an image to binary format
        /// using Otsu's thresholding algorithm.
        /// </summary>
        [TestMethod]
        public void OtsuBinarization_ValidImage_ReturnsBinaryImage()
        {
            // Act
            var binaryImage = ImagePreprocessing.OtsuBinarization(_testImagePath);
            
            // Assert
            Assert.IsNotNull(binaryImage, "Binary image should not be null");
            Assert.IsFalse(binaryImage.IsEmpty, "Binary image should not be empty");
        }
        
        /// <summary>
        /// Tests histogram equalization enhancement.
        /// Verifies that the method successfully enhances image contrast
        /// through histogram equalization.
        /// </summary>
        [TestMethod]
        public void HistogramEqualization_ValidImage_ReturnsEnhancedImage()
        {
            // Act
            var enhancedImage = ImagePreprocessing.HistogramEqualization(_testImagePath);
            
            // Assert
            Assert.IsNotNull(enhancedImage, "Enhanced image should not be null");
            Assert.IsFalse(enhancedImage.IsEmpty, "Enhanced image should not be empty");
        }
        
        /// <summary>
        /// Tests Gaussian noise reduction.
        /// Verifies that the method successfully reduces image noise
        /// using Gaussian blur while preserving important features.
        /// </summary>
        [TestMethod]
        public void RemoveNoiseUsingGaussian_ValidImage_ReturnsDenoisedImage()
        {
            // Act
            var denoisedImage = ImagePreprocessing.RemoveNoiseUsingGaussian(_testImagePath);
            
            // Assert
            Assert.IsNotNull(denoisedImage, "Denoised image should not be null");
            Assert.IsFalse(denoisedImage.IsEmpty, "Denoised image should not be empty");
        }
        
        /// <summary>
        /// Tests error handling for non-existent image files.
        /// Verifies that the preprocessing methods handle missing files gracefully
        /// by either returning null/empty images or throwing appropriate exceptions.
        /// </summary>
        [TestMethod]
        public void ProcessImage_NonexistentFile_HandlesErrorGracefully()
        {
            // Arrange
            string nonExistentFile = "non_existent_file.png";
            
            try
            {
                // Act - This should either throw or return null/empty image
                var image = ImagePreprocessing.ConvertToGrayscale(nonExistentFile);
                
                // If it doesn't throw, it should return an empty image
                Assert.IsTrue(image == null || image.IsEmpty, "Should return null or empty image for nonexistent file");
            }
            catch (Exception ex)
            {
                // If it throws, it should be a FileNotFoundException or similar
                Assert.IsTrue(
                    ex is FileNotFoundException || 
                    ex.Message.Contains("exist") || 
                    ex.Message.Contains("file"),
                    $"Expected file-related exception, got: {ex.GetType().Name}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Tests the application of multiple preprocessing techniques in sequence.
        /// Verifies that multiple image processing steps can be applied successfully
        /// in a pipeline, with each step producing valid output that can be used
        /// as input for the next step.
        /// </summary>
        [TestMethod]
        public void ProcessImageWithMultipleTechniques_ValidImage_SuccessfullyAppliesAll()
        {
            // Arrange
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            
            try
            {
                // Act - Apply a sequence of preprocessing techniques
                var grayscaleImage = ImagePreprocessing.ConvertToGrayscale(_testImagePath);
                string grayscalePath = Path.Combine(tempDir, "grayscale.png");
                CvInvoke.Imwrite(grayscalePath, grayscaleImage);
                
                var binaryImage = ImagePreprocessing.OtsuBinarization(grayscalePath);
                string binaryPath = Path.Combine(tempDir, "binary.png");
                CvInvoke.Imwrite(binaryPath, binaryImage);
                
                var enhancedImage = ImagePreprocessing.HistogramEqualization(binaryPath);
                string enhancedPath = Path.Combine(tempDir, "enhanced.png");
                CvInvoke.Imwrite(enhancedPath, enhancedImage);
                
                // Assert
                Assert.IsNotNull(grayscaleImage, "Grayscale image should not be null");
                Assert.IsNotNull(binaryImage, "Binary image should not be null");
                Assert.IsNotNull(enhancedImage, "Enhanced image should not be null");
                
                Assert.IsFalse(grayscaleImage.IsEmpty, "Grayscale image should not be empty");
                Assert.IsFalse(binaryImage.IsEmpty, "Binary image should not be empty");
                Assert.IsFalse(enhancedImage.IsEmpty, "Enhanced image should not be empty");
                
                Assert.IsTrue(File.Exists(grayscalePath), "Grayscale image file should exist");
                Assert.IsTrue(File.Exists(binaryPath), "Binary image file should exist");
                Assert.IsTrue(File.Exists(enhancedPath), "Enhanced image file should exist");
            }
            finally
            {
                // Clean up
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
        }

        /// <summary>
        /// Tests Sobel edge detection functionality.
        /// Verifies that the method successfully detects edges in the image
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void SobelEdgeDetection_ValidImage_ReturnsEdgeImage()
        {
            // Act
            var edgeImage = ImagePreprocessing.SobelEdgeDetection(_testImagePath);
            
            // Assert
            Assert.IsNotNull(edgeImage, "Edge image should not be null");
            Assert.IsFalse(edgeImage.IsEmpty, "Edge image should not be empty");
            Assert.AreEqual(1, edgeImage.NumberOfChannels, "Edge image should be single channel");
        }

        /// <summary>
        /// Tests Laplacian edge detection functionality.
        /// Verifies that the method successfully detects edges using
        /// the Laplacian operator and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void LaplacianEdgeDetection_ValidImage_ReturnsEdgeImage()
        {
            // Act
            var edgeImage = ImagePreprocessing.LaplacianEdgeDetection(_testImagePath);
            
            // Assert
            Assert.IsNotNull(edgeImage, "Edge image should not be null");
            Assert.IsFalse(edgeImage.IsEmpty, "Edge image should not be empty");
            Assert.AreEqual(1, edgeImage.NumberOfChannels, "Edge image should be single channel");
        }

        /// <summary>
        /// Tests bilateral filtering functionality.
        /// Verifies that the method successfully applies edge-preserving smoothing
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void BilateralFilter_ValidImage_ReturnsDenoisedImage()
        {
            // Act
            var filteredImage = ImagePreprocessing.BilateralFilter(_testImagePath);
            
            // Assert
            Assert.IsNotNull(filteredImage, "Filtered image should not be null");
            Assert.IsFalse(filteredImage.IsEmpty, "Filtered image should not be empty");
        }

        /// <summary>
        /// Tests morphological dilation functionality.
        /// Verifies that the method successfully expands white regions
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void Dilation_ValidImage_ReturnsDilatedImage()
        {
            // Act
            var dilatedImage = ImagePreprocessing.Dilation(_testImagePath);
            
            // Assert
            Assert.IsNotNull(dilatedImage, "Dilated image should not be null");
            Assert.IsFalse(dilatedImage.IsEmpty, "Dilated image should not be empty");
        }

        /// <summary>
        /// Tests morphological erosion functionality.
        /// Verifies that the method successfully shrinks white regions
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void Erosion_ValidImage_ReturnsErodedImage()
        {
            // Act
            var erodedImage = ImagePreprocessing.Erosion(_testImagePath);
            
            // Assert
            Assert.IsNotNull(erodedImage, "Eroded image should not be null");
            Assert.IsFalse(erodedImage.IsEmpty, "Eroded image should not be empty");
        }

        /// <summary>
        /// Tests morphological opening functionality.
        /// Verifies that the method successfully performs erosion followed by dilation
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void Opening_ValidImage_ReturnsOpenedImage()
        {
            // Act
            var openedImage = ImagePreprocessing.Opening(_testImagePath);
            
            // Assert
            Assert.IsNotNull(openedImage, "Opened image should not be null");
            Assert.IsFalse(openedImage.IsEmpty, "Opened image should not be empty");
        }

        /// <summary>
        /// Tests morphological closing functionality.
        /// Verifies that the method successfully performs dilation followed by erosion
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void Closing_ValidImage_ReturnsClosedImage()
        {
            // Act
            var closedImage = ImagePreprocessing.Closing(_testImagePath);
            
            // Assert
            Assert.IsNotNull(closedImage, "Closed image should not be null");
            Assert.IsFalse(closedImage.IsEmpty, "Closed image should not be empty");
        }

        /// <summary>
        /// Tests morphological gradient functionality.
        /// Verifies that the method successfully calculates the difference
        /// between dilation and erosion, producing a valid output image.
        /// </summary>
        [TestMethod]
        public void MorphologicalGradient_ValidImage_ReturnsGradientImage()
        {
            // Act
            var gradientImage = ImagePreprocessing.MorphologicalGradient(_testImagePath);
            
            // Assert
            Assert.IsNotNull(gradientImage, "Gradient image should not be null");
            Assert.IsFalse(gradientImage.IsEmpty, "Gradient image should not be empty");
        }

        /// <summary>
        /// Tests top hat transformation functionality.
        /// Verifies that the method successfully calculates the difference
        /// between the original image and its opening, producing a valid output image.
        /// </summary>
        [TestMethod]
        public void TopHat_ValidImage_ReturnsTopHatImage()
        {
            // Act
            var topHatImage = ImagePreprocessing.TopHat(_testImagePath);
            
            // Assert
            Assert.IsNotNull(topHatImage, "Top hat image should not be null");
            Assert.IsFalse(topHatImage.IsEmpty, "Top hat image should not be empty");
        }

        /// <summary>
        /// Tests black hat transformation functionality.
        /// Verifies that the method successfully calculates the difference
        /// between the closing and the original image, producing a valid output image.
        /// </summary>
        [TestMethod]
        public void BlackHat_ValidImage_ReturnsBlackHatImage()
        {
            // Act
            var blackHatImage = ImagePreprocessing.BlackHat(_testImagePath);
            
            // Assert
            Assert.IsNotNull(blackHatImage, "Black hat image should not be null");
            Assert.IsFalse(blackHatImage.IsEmpty, "Black hat image should not be empty");
        }

        /// <summary>
        /// Tests gamma correction functionality.
        /// Verifies that the method successfully adjusts image brightness
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void GammaCorrection_ValidImage_ReturnsGammaCorrectedImage()
        {
            // Act
            var gammaCorrectedImage = ImagePreprocessing.GammaCorrection(_testImagePath);
            
            // Assert
            Assert.IsNotNull(gammaCorrectedImage, "Gamma corrected image should not be null");
            Assert.IsFalse(gammaCorrectedImage.IsEmpty, "Gamma corrected image should not be empty");
        }

        /// <summary>
        /// Tests HSV color space conversion functionality.
        /// Verifies that the method successfully converts the image to HSV
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void ConvertToHsv_ValidImage_ReturnsHsvImage()
        {
            // Act
            var hsvImage = ImagePreprocessing.ConvertToHsv(_testImagePath);
            
            // Assert
            Assert.IsNotNull(hsvImage, "HSV image should not be null");
            Assert.IsFalse(hsvImage.IsEmpty, "HSV image should not be empty");
            Assert.AreEqual(3, hsvImage.NumberOfChannels, "HSV image should have 3 channels");
        }

        /// <summary>
        /// Tests log transformation functionality.
        /// Verifies that the method successfully enhances dark regions
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void LogTransform_ValidImage_ReturnsLogTransformedImage()
        {
            // Act
            var logTransformedImage = ImagePreprocessing.LogTransform(_testImagePath);
            
            // Assert
            Assert.IsNotNull(logTransformedImage, "Log transformed image should not be null");
            Assert.IsFalse(logTransformedImage.IsEmpty, "Log transformed image should not be empty");
        }

        /// <summary>
        /// Tests image normalization functionality.
        /// Verifies that the method successfully normalizes pixel values
        /// and produces a valid output image.
        /// </summary>
        [TestMethod]
        public void NormalizeImage_ValidImage_ReturnsNormalizedImage()
        {
            // Act
            var normalizedImage = ImagePreprocessing.NormalizeImage(_testImagePath);
            
            // Assert
            Assert.IsNotNull(normalizedImage, "Normalized image should not be null");
            Assert.IsFalse(normalizedImage.IsEmpty, "Normalized image should not be empty");
        }
    }
} 