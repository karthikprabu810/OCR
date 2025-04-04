using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Drawing;
using Emgu.CV.Structure;


namespace ocrApplication;

/// <summary>
/// Provides image preprocessing techniques to enhance OCR quality.
/// Implements various algorithms using EmguCV to improve text clarity and recognition accuracy.
/// </summary>
public static class ImagePreprocessing
{
    // Kernel size for morphological operations and filters (must be odd)
    private const int KernelSize = 3;

    /// <summary>
    /// Cache for loaded images to prevent redundant disk reads.
    /// </summary>
    private static readonly Dictionary<string, Mat> ImageCache = new Dictionary<string, Mat>();
    
    /// <summary>
    /// Lock object for thread-safe cache access.
    /// </summary>
    private static readonly object CacheLock = new object();

    /// <summary>
    /// Loads an image from disk or retrieves it from cache.
    /// </summary>
    /// <param name="imagePath">Path to the image file</param>
    /// <param name="mode">Image reading mode (color, grayscale, etc.)</param>
    /// <returns>A clone of the loaded or cached image</returns>
    private static Mat LoadImage(string imagePath, ImreadModes mode = ImreadModes.Color)
    {
        lock (CacheLock)
        {
            // Create unique key based on path and reading mode
            string cacheKey = $"{imagePath}_{mode}";
            if (!ImageCache.TryGetValue(cacheKey, out Mat cachedImage))
            {
                // Image not in cache, load from disk
                cachedImage = CvInvoke.Imread(imagePath, mode);
                if (!cachedImage.IsEmpty)
                {
                    // Store in cache for future use
                    ImageCache[cacheKey] = cachedImage;
                }
            }
            return cachedImage.Clone(); // Return a clone to prevent modifying the cached image
        }
    }

    // Clears the image cache when it exceeds a threshold size.
    private static void ClearCacheIfNeeded()
    {
        lock (CacheLock)
        {
            if (ImageCache.Count > 100) // Arbitrary limit to prevent excessive memory usage
            {
                // Dispose all cached images to free unmanaged resources
                foreach (var image in ImageCache.Values)
                {
                    image.Dispose();
                }
                ImageCache.Clear();
            }
        }
    }

    // Converts a color image to grayscale.
    // <returns>Grayscale version of the image</returns>
    public static Mat ConvertToGrayscale(string imagePath)
    {
        try
        {
            // Load directly in grayscale mode for efficiency
            Mat grayImage = LoadImage(imagePath, ImreadModes.Grayscale);
            
            if (grayImage.IsEmpty)
            {
                Console.WriteLine("Error loading image: " + imagePath);
                return new Mat();
            }
            
            ClearCacheIfNeeded();
            return grayImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in grayscale conversion: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies Gaussian blur to reduce noise while preserving image structure.
    // <returns>Denoised image</returns>
    public static Mat RemoveNoiseUsingGaussian(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for Gaussian blur: " + imagePath);
                return new Mat();
            }

            Mat denoisedImage = new Mat();
            // Apply 5x5 Gaussian kernel for noise reduction
            CvInvoke.GaussianBlur(image, denoisedImage, new Size(5, 5), 0);
            
            ClearCacheIfNeeded();
            return denoisedImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Gaussian blur: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies Median blur to remove salt-and-pepper noise while preserving edges.
    // <returns>Denoised image</returns>
    public static Mat RemoveNoiseUsingMedian(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for median blur: " + imagePath);
                return new Mat();
            }

            Mat denoisedImage = new Mat();
            // Apply median blur with specified kernel size
            CvInvoke.MedianBlur(image, denoisedImage, KernelSize);
            
            ClearCacheIfNeeded();
            return denoisedImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in median blur: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies adaptive thresholding to handle varying lighting conditions.
    // <returns>Threshold binary image</returns>
    public static Mat AdaptiveThresholding(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for adaptive threshold: " + imagePath);
                return new Mat();
            }

            Mat thresholdImage = new Mat();
            // Apply adaptive thresholding with Gaussian-weighted neighborhood
            CvInvoke.AdaptiveThreshold(image, thresholdImage, 255,
                AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 11, 2);
            
            ClearCacheIfNeeded();
            return thresholdImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in adaptive thresholding: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies gamma correction to adjust image brightness and contrast.
    // <returns>Gamma-corrected image</returns>
    public static Mat GammaCorrection(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for gamma correction: " + imagePath);
                return new Mat();
            }

            // Estimate optimal gamma value based on image properties
            double gamma = EstimateGamma(image);
            
            // Convert to 32-bit float for processing
            Mat normalizedImage = new Mat();
            image.ConvertTo(normalizedImage, DepthType.Cv32F, 1.0 / 255.0);
            
            // Apply gamma correction: pixel = pixel^(1/gamma)
            Mat correctedImage = new Mat();
            CvInvoke.Pow(normalizedImage, 1.0 / gamma, correctedImage);
            
            // Convert back to 8-bit format
            Mat resultImage = new Mat();
            correctedImage.ConvertTo(resultImage, DepthType.Cv8U, 255.0);
            
            normalizedImage.Dispose();
            correctedImage.Dispose();
            
            ClearCacheIfNeeded();
            return resultImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in gamma correction: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies gamma correction to reduce brightness by 20%.
    public static Mat GammaCorrectionRed20(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for gamma correction: " + imagePath);
                return new Mat();
            }

            // Estimate optimal gamma value based on image properties
            double gamma = EstimateGamma(image);
            
            // Convert to 32-bit float for processing
            Mat normalizedImage = new Mat();
            image.ConvertTo(normalizedImage, DepthType.Cv32F, 1.0 / 255.0);
            
            // Apply gamma correction: pixel = pixel^(1/gamma)
            Mat correctedImage = new Mat();
            CvInvoke.Pow(normalizedImage, 1.0 / gamma, correctedImage);
            correctedImage *= 0.8;
            // Convert back to 8-bit format
            Mat resultImage = new Mat();
            correctedImage.ConvertTo(resultImage, DepthType.Cv8U, 255.0);
            
            normalizedImage.Dispose();
            correctedImage.Dispose();
            
            ClearCacheIfNeeded();
            return resultImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in gamma correction: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies gamma correction to reduce brightness by 40%.
    public static Mat GammaCorrectionRed40(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for gamma correction: " + imagePath);
                return new Mat();
            }

            // Estimate optimal gamma value based on image properties
            double gamma = EstimateGamma(image);
            
            // Convert to 32-bit float for processing
            Mat normalizedImage = new Mat();
            image.ConvertTo(normalizedImage, DepthType.Cv32F, 1.0 / 255.0);
            
            // Apply gamma correction: pixel = pixel^(1/gamma)
            Mat correctedImage = new Mat();
            CvInvoke.Pow(normalizedImage, 1.0 / gamma, correctedImage);
            correctedImage *= 0.6;
            // Convert back to 8-bit format
            Mat resultImage = new Mat();
            correctedImage.ConvertTo(resultImage, DepthType.Cv8U, 255.0);
            
            normalizedImage.Dispose();
            correctedImage.Dispose();
            
            ClearCacheIfNeeded();
            return resultImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in gamma correction: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies gamma correction to reduce brightness by 60%.
    public static Mat GammaCorrectionRed60(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for gamma correction: " + imagePath);
                return new Mat();
            }

            // Estimate optimal gamma value based on image properties
            double gamma = EstimateGamma(image);
            
            // Convert to 32-bit float for processing
            Mat normalizedImage = new Mat();
            image.ConvertTo(normalizedImage, DepthType.Cv32F, 1.0 / 255.0);
            
            // Apply gamma correction: pixel = pixel^(1/gamma)
            Mat correctedImage = new Mat();
            CvInvoke.Pow(normalizedImage, 1.0 / gamma, correctedImage);
            correctedImage *= 0.4;
            // Convert back to 8-bit format
            Mat resultImage = new Mat();
            correctedImage.ConvertTo(resultImage, DepthType.Cv8U, 255.0);
            
            normalizedImage.Dispose();
            correctedImage.Dispose();
            
            ClearCacheIfNeeded();
            return resultImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in gamma correction: {ex.Message}");
            return new Mat();
        }
    }
    
    // Applies gamma correction to reduce brightness by 80%.
    public static Mat GammaCorrectionRed80(string imagePath)
    {
        try
        {
            // Load grayscale image
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for gamma correction: " + imagePath);
                return new Mat();
            }

            // Estimate optimal gamma value based on image properties
            double gamma = EstimateGamma(image);
            
            // Convert to 32-bit float for processing
            Mat normalizedImage = new Mat();
            image.ConvertTo(normalizedImage, DepthType.Cv32F, 1.0 / 255.0);
            
            // Apply gamma correction: pixel = pixel^(1/gamma)
            Mat correctedImage = new Mat();
            CvInvoke.Pow(normalizedImage, 1.0 / gamma, correctedImage);
            correctedImage *= 0.2;
            // Convert back to 8-bit format
            Mat resultImage = new Mat();
            correctedImage.ConvertTo(resultImage, DepthType.Cv8U, 255.0);
            
            normalizedImage.Dispose();
            correctedImage.Dispose();
            
            ClearCacheIfNeeded();
            return resultImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in gamma correction: {ex.Message}");
            return new Mat();
        }
    }
    
    // Estimates optimal gamma value based on image histogram.
    // <returns>Estimated gamma value</returns>
    private static double EstimateGamma(Mat image)
    {
        try
        {
            // Calculate average brightness directly instead of using histogram
            double avgBrightness;
            double totalPixels = image.Rows * image.Cols;
            
            // Convert image to byte array for direct access
            byte[] imageData = new byte[(int)totalPixels];
            if (image.Depth == DepthType.Cv8U && image.NumberOfChannels == 1)
            {
                // Copy data from image to byte array
                Marshal.Copy(
                    image.DataPointer, 
                    imageData, 
                    0, 
                    (int)totalPixels);
                
                // Calculate average brightness directly
                long sum = 0;
                foreach (var t in imageData)
                {
                    sum += t;
                }
                // for (int i = 0; i < imageData.Length; i++)
                // {
                //     sum += imageData[i];
                // }
                avgBrightness = sum / totalPixels;
            }
            else
            {
                // Default value if image format isn't as expected
                avgBrightness = 128;
            }
            
            // Target middle brightness (128)
            double targetBrightness = 128.0;
            
            // Calculate gamma (avoid division by zero)
            double gamma = Math.Log(targetBrightness / 255.0) / Math.Log(Math.Max(avgBrightness, 1) / 255.0);
            
            // Limit gamma to reasonable range
            gamma = Math.Min(Math.Max(gamma, 0.5), 2.5);
            
            return gamma;
        }
        catch
        {
            // Return default gamma value if estimation fails
            return 1.0;
        }
    }
    
    /// <summary>
    /// Applies Canny edge detection to identify text boundaries.
    /// Edge detection helps isolate text regions from backgrounds by finding
    /// sharp transitions in pixel intensity.
    /// </summary>
    /// <returns>Edge-detected binary image as a Mat object</returns>
    public static Mat CannyEdgeDetection(string imagePath)
    {
        // First convert to grayscale, as edge detection works on intensity values
        Mat image = ConvertToGrayscale(imagePath);
        Mat edgeDetectedImage = new Mat();
        
        // Apply Canny edge detection with:
        // - Lower threshold = 100 (pixels with gradient magnitude below this are non-edges)
        // - Upper threshold = 200 (pixels with gradient magnitude above this are definite edges)
        // Pixels between thresholds are considered edges if connected to definite edges
        CvInvoke.Canny(image, edgeDetectedImage, 100, 200);
        
        if (edgeDetectedImage.IsEmpty)
        {
            Console.WriteLine("Canny edge detection failed for " + imagePath);
        }
        return edgeDetectedImage;
    }

    /// <summary>
    /// Applies dilation morphological operation to thicken text in the image.
    /// Dilation expands the boundaries of foreground (white) regions, which can
    /// help connect broken characters or enhance thin text strokes.
    /// </summary>
    /// <returns>Dilated image as a Mat object</returns>
    public static Mat Dilation(string imagePath)
    {
        // Convert image to grayscale
        Mat image = ConvertToGrayscale(imagePath);
        if (image.IsEmpty)
        {
            Console.WriteLine("Error: Unable to load or convert image " + imagePath);
        }

        Mat dilatedImage = new Mat();

        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));

        // Perform dilation
        try
        {
            CvInvoke.Dilate(image, dilatedImage, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

            // Check if dilation was successful
            if (dilatedImage.IsEmpty)
            {
                Console.WriteLine("Dilation failed for " + imagePath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during dilation: " + ex.Message);
        }

        return dilatedImage;
    }

    /// <summary>
    /// Applies erosion morphological operation to thin text in the image.
    /// Erosion shrinks the boundaries of foreground (white) regions, which can
    /// help separate touching characters and remove small noise.
    /// </summary>
    /// <returns>Eroded image as a Mat object</returns>
    public static Mat Erosion(string imagePath)
    {
        Mat image = ConvertToGrayscale(imagePath);
        Mat erodedImage = new Mat();
        
        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));
        
        // Apply erosion operation
        // Parameters:
        // - Border type: Reflect (mirrors pixels at the borders)
        // - Border value: Black (0)
        CvInvoke.Erode(image, erodedImage, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
        
        if (erodedImage.IsEmpty)
        {
            Console.WriteLine("Erosion failed for " + imagePath);
        }
        
        return erodedImage;
    }
    
    /// <summary>
    /// Applies Otsu's method for automatic thresholding to binarize images.
    /// Otsu's algorithm determines the optimal threshold value automatically
    /// by minimizing the intra-class variance between foreground and background.
    /// </summary>
    /// <returns>Binary image as a Mat object</returns>
    public static Mat OtsuBinarization(string imagePath)
    {
        // Step 1: Read the image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        
        // Step 2: Apply Otsu's method to perform automatic thresholding
        // Otsu automatically calculates the optimal threshold to separate
        // foreground and background based on the image histogram
        Mat binaryImage = new Mat();
        try
        {
            // Threshold parameter 0 is ignored when using Otsu's method
            // Maximum value 255 is assigned to pixels that pass the threshold
            CvInvoke.Threshold(image, binaryImage, 0, 255, ThresholdType.Otsu);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Otsu binarization failed for image at path: {imagePath}. Error code: 1502. Exception: {ex.Message}");
            return new Mat(); // Return empty Mat if thresholding fails
        }

        return binaryImage;
    }
    
    /// <summary>
    /// Detects and corrects skew (rotation) in document images.
    /// Uses Hough line detection to find the dominant orientation of lines
    /// and rotates the image to straighten the text, improving OCR accuracy.
    /// </summary>
    /// <returns>Deskewed (straightened) image as a Mat object</returns>
    public static Mat Deskew(string imagePath)
    {
        // Load the image in grayscale
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty)
        {
            throw new Exception("Failed to load image: " + imagePath);
        }

        // Detect edges to identify lines of text
        Mat edges = new Mat();
        CvInvoke.Canny(image, edges, 50, 150);  // Detect edges with Canny algorithm

        // Detect lines using Hough Transform
        // Parameters:
        // - Accumulator resolution: 1 pixel
        // - Angle resolution: 1 degree
        // - Threshold: 100 votes
        // - Min line length: 100 pixels
        // - Max line gap: 10 pixels
        LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 100, 100, 10);
        
        if (lines.Length == 0)
        {
            Console.WriteLine("No lines detected, returning original image.");
            return image;
        }

        // Compute average angle of detected lines to determine skew
        double totalAngle = 0;
        int count = 0;

        foreach (var line in lines)
        {
            // Calculate angle of the line in degrees
            double angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180 / Math.PI;
            
            // Ignore nearly vertical lines (likely not text lines)
            if (Math.Abs(angle) < 45)
            {
                totalAngle += angle;
                count++;
            }
        }

        // If no valid angles found, return original image
        if (count == 0) return image;

        // Calculate average skew angle
        double skewAngle = totalAngle / count;

        // Rotate image to correct skew
        // Negative angle to counter-rotate the detected skew
        return RotateImage(image, -skewAngle);
    }
    
    //  Rotates the image by 45 degree
    public static Mat Rotate_45(string imagePath)
    {
        // Load the image in grayscale
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty)
        {
            throw new Exception("Failed to load image: " + imagePath);
        }
        
        // Calculate average skew angle
        double skewAngle = 45;

        // Rotate image to correct skew
        // Negative angle to counter-rotate the detected skew
        return RotateImage(image, -skewAngle);
    }
    
    //  Rotates the image by 90 degree
    public static Mat Rotate_90(string imagePath)
    {
        // Load the image in grayscale
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty)
        {
            throw new Exception("Failed to load image: " + imagePath);
        }

        // Calculate average skew angle
        double skewAngle = 90;

        // Rotate image to correct skew
        // Negative angle to counter-rotate the detected skew
        return RotateImage(image, -skewAngle);
    }
    
    //  Rotates the image by 135 degree
    public static Mat Rotate_135(string imagePath)
    {
        // Load the image in grayscale
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty)
        {
            throw new Exception("Failed to load image: " + imagePath);
        }
        
        // Calculate average skew angle
        double skewAngle = 135;

        // Rotate image to correct skew
        // Negative angle to counter-rotate the detected skew
        return RotateImage(image, -skewAngle);
    }
    
    //  Rotates the image by 180 degree
    public static Mat Rotate_180(string imagePath)
    {
        // Load the image in grayscale
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty)
        {
            throw new Exception("Failed to load image: " + imagePath);
        }
        // Calculate average skew angle
        double skewAngle = 180;
        // Rotate image to correct skew
        // Negative angle to counter-rotate the detected skew
        return RotateImage(image, -skewAngle);
    }
    
    /// <summary>
    /// Rotates an image by the specified angle around its center.
    /// Used by the deskew function to correct image orientation.
    /// </summary>
    /// <param name="src">Source image to rotate</param>
    /// <param name="angle">Angle in degrees to rotate the image</param>
    /// <returns>Rotated image as a Mat object</returns>
    private static Mat RotateImage(Mat src, double angle)
    {
        // Calculate the center of the image
        PointF center = new PointF(src.Width / 2f, src.Height / 2f);
        
        // Get the rotation transformation matrix
        Mat rotationMatrix = new Mat();
        CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);
        
        // Apply the rotation to create a new image
        // Parameters:
        // - Linear interpolation for smooth rotation
        // - White border filling (255,255,255) to avoid black edges
        Mat rotated = new Mat();
        CvInvoke.WarpAffine(src, rotated, rotationMatrix, src.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255, 255, 255));

        return rotated;
    }
    
    /// <summary>
    /// Applies histogram equalization to enhance image contrast.
    /// Stretches the intensity distribution to cover the full dynamic range,
    /// improving visibility of text in low-contrast images.
    /// </summary>
    /// <returns>Contrast-enhanced image as a Mat object</returns>
    public static Mat HistogramEqualization(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        // Apply histogram equalization
        // This redistributes intensity values to enhance contrast
        Mat equalizedImage = new Mat();
        CvInvoke.EqualizeHist(image, equalizedImage);
        
        return equalizedImage;
    }
    
    /// <summary>
    /// Applies bilateral filtering to reduce noise while preserving edges.
    /// Unlike Gaussian blur, bilateral filter considers both spatial proximity
    /// and intensity similarity, maintaining sharp edges important for OCR.
    /// </summary>
    /// <returns>Filtered image as a Mat object</returns>
    public static Mat BilateralFilter(string imagePath)
    {
        // Load original color image
        Mat image = CvInvoke.Imread(imagePath);
        if (image.IsEmpty) return image;
        
        // Apply bilateral filter
        // Parameters:
        // - Diameter of each pixel neighborhood: 9
        // - Filter sigma in color space: 75
        // - Filter sigma in coordinate space: 75
        // Higher sigmas mean more aggressive smoothing while preserving edges
        Mat filteredImage = new Mat();
        CvInvoke.BilateralFilter(image, filteredImage, 9, 75, 75);
        
        return filteredImage;
    }
    
    /// <summary>
    /// Applies Sobel edge detection to highlight horizontal edges.
    /// Useful for detecting text lines and character boundaries
    /// based on horizontal gradient changes.
    /// </summary>
    /// <returns>Edge map as a Mat object with highlighted horizontal edges</returns>
    public static Mat SobelEdgeDetection(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        // Apply Sobel operator
        // Parameters:
        // - Output depth: 8-bit unsigned
        // - X derivative order: 1 (detect horizontal edges)
        // - Y derivative order: 0 (ignore vertical edges)
        // - Kernel size: 3x3
        // - Scale factor: 1
        // - Delta value: 0
        // - Border type: Reflect
        Mat edges = new Mat();
        CvInvoke.Sobel(image, edges, DepthType.Cv8U, 1, 0, KernelSize, 1, 0, BorderType.Reflect);
        
        return edges;
    }

    /// <summary>
    /// Applies Laplacian edge detection to highlight all edges.
    /// Detects edges in all directions simultaneously by measuring
    /// the second derivative (rate of change of gradient) in an image.
    /// </summary>
    /// <returns>Edge map as a Mat object with all edges highlighted</returns>
    public static Mat LaplacianEdgeDetection(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        // Apply Laplacian operator
        // Parameters:
        // - Output depth: 8-bit unsigned
        // - Kernel size: 3x3
        // - Scale factor: 1
        // - Delta value: 0
        // - Border type: Reflect
        Mat edges = new Mat();
        CvInvoke.Laplacian(image, edges, DepthType.Cv8U, KernelSize, 1, 0, BorderType.Reflect);
        
        return edges;
    }

    /// <summary>
    /// Normalizes pixel intensity values to a specified range.
    /// Ensures consistent brightness and contrast across images
    /// by scaling all pixel values to a standard range.
    /// </summary>
    /// <returns>Normalized image as a Mat object</returns>
    public static Mat NormalizeImage(string imagePath)
    {
        // Define normalization target range (0-1)
        double newMin = 0; 
        double newMax = 1;
        
        // Load the original image
        Mat image = CvInvoke.Imread(imagePath);
        if (image.IsEmpty) return image;
    
        // Apply min-max normalization
        // Scales all pixel values to range between newMin and newMax
        Mat normalizedImage = new Mat();
        CvInvoke.Normalize(image, normalizedImage, newMin, newMax, NormType.MinMax, DepthType.Cv8U);
        
        return normalizedImage;
    }
    
    /// <summary>
    /// Applies morphological opening operation (erosion followed by dilation).
    /// Removes small objects and noise from the foreground while preserving
    /// the shape and size of larger objects like text characters.
    /// </summary>
    /// <returns>Opened image as a Mat object</returns>
    public static Mat Opening(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        Mat openedImage = new Mat();
        
        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));
   
        // Apply morphological opening
        // This is equivalent to erosion followed by dilation
        // Removes small objects and noise while preserving shape and size
        CvInvoke.MorphologyEx(image, openedImage, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
        
        return openedImage;
    }
    
    /// <summary>
    /// Applies morphological closing operation (dilation followed by erosion).
    /// Closes small holes in the foreground and joins nearby objects,
    /// useful for connecting broken character strokes in text.
    /// </summary>
    /// <returns>Closed image as a Mat object</returns>
    public static Mat Closing(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        Mat closedImage = new Mat();
        
        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));
   
        // Apply morphological closing
        // This is equivalent to dilation followed by erosion
        // Closes small holes and joins nearby objects
        CvInvoke.MorphologyEx(image, closedImage, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

        return closedImage;
    }

    /// <summary>
    /// Applies morphological gradient operation (dilation minus erosion).
    /// Produces an image containing the boundaries of objects,
    /// highlighting the transitions between text and background.
    /// </summary>
    /// <returns>Gradient image as a Mat object</returns>
    public static Mat MorphologicalGradient(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;

        Mat gradientImage = new Mat();
    
        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));

        // Apply morphological gradient
        // This is equivalent to dilation minus erosion
        // Produces an outline of objects in the image
        CvInvoke.MorphologyEx(image, gradientImage, MorphOp.Gradient, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
    
        return gradientImage;
    }
    
    /// <summary>
    /// Applies logarithmic transformation to enhance details in dark regions.
    /// Maps the input intensity values to the logarithmic curve,
    /// compressing bright values and expanding dark ones.
    /// </summary>
    /// <returns>Log-transformed image as a Mat object</returns>
    public static Mat LogTransform(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
    
        // Log transformation requires floating point calculation
        Mat logImage = new Mat();
        image.ConvertTo(logImage, DepthType.Cv32F);
        
        // Apply log transformation: s = c * log(1 + r)
        // Adding 1 to avoid log(0) which is undefined
        CvInvoke.Pow(logImage + 1, 1.0, logImage);  // Apply log transformation
        
        // Convert back to 8-bit unsigned format
        logImage.ConvertTo(logImage, DepthType.Cv8U);
        
        return logImage;
    }
    
    /// <summary>
    /// Converts BGR color image to HSV (Hue, Saturation, Value) color space.
    /// HSV representation can be useful for color-based segmentation
    /// or enhancing specific color properties.
    /// </summary>
    /// <returns>HSV color space image as a Mat object</returns>
    public static Mat ConvertToHsv(string imagePath)
    {
        // Load original color image
        Mat image = CvInvoke.Imread(imagePath);
        if (image.IsEmpty) return image;
        
        // Convert from BGR (default in OpenCV) to HSV color space
        Mat hsvImage = new Mat();
        CvInvoke.CvtColor(image, hsvImage, ColorConversion.Bgr2Hsv);
        
        return hsvImage;
    }
    
    /// <summary>
    /// Applies Top-Hat morphological operation (original minus opening).
    /// Extracts small, bright details and reduces background variations,
    /// useful for finding bright text on varying backgrounds.
    /// </summary>
    /// <returns>Top-Hat filtered image as a Mat object</returns>
    public static Mat TopHat(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));
        
        // First apply opening (erosion followed by dilation)
        Mat openedImage = new Mat();
        CvInvoke.MorphologyEx(image, openedImage, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

        // Subtract the opened image from the original to get bright details
        // Top Hat = Original - Opening
        Mat topHatImage = new Mat();
        CvInvoke.Subtract(image, openedImage, topHatImage);
        
        return topHatImage;
    }
    
    /// <summary>
    /// Applies Black-Hat morphological operation (closing minus original).
    /// Extracts small, dark details and enhances dark text against lighter backgrounds.
    /// </summary>
    /// <returns>Black-Hat filtered image as a Mat object</returns>
    public static Mat BlackHat(string imagePath)
    {
        // Load image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        
        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(KernelSize, KernelSize), new Point(-1, -1));
        
        // First apply closing (dilation followed by erosion)
        Mat closedImage = new Mat();
        CvInvoke.MorphologyEx(image, closedImage, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
        
        // Subtract the original image from the closed image to get dark details
        // Black Hat = Closing - Original
        Mat blackHatImage = new Mat();
        CvInvoke.Subtract(closedImage, image, blackHatImage);
        
        return blackHatImage;
    }
    
    /// <summary>
    /// Applies grayscale to the image.
    /// Applies Gaussian blur to reduce noise while preserving image structure,
    /// Applies gamma correction to adjust image brightness and contrast.
    /// </summary>
    /// <returns>Processed image as a Mat object</returns>
    public static Mat Combo1(string imagePath)
    {
        try
        {
            // Load the image in grayscale mode for efficiency
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
        
            // Check if image was loaded successfully
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image: " + imagePath);
                return new Mat();  // Return an empty matrix in case of failure
            }

            // Apply Gaussian noise reduction (Gaussian Blur)
            Mat denoisedImage = new Mat();
            CvInvoke.GaussianBlur(image, denoisedImage, new Size(5, 5), 0);

            // Estimate optimal gamma value based on image properties
            double gamma = EstimateGamma(denoisedImage);
        
            // Convert the image to 32-bit float for processing
            Mat normalizedImage = new Mat();
            denoisedImage.ConvertTo(normalizedImage, DepthType.Cv32F, 1.0 / 255.0);
        
            // Apply gamma correction: pixel = pixel^(1/gamma)
            Mat correctedImage = new Mat();
            CvInvoke.Pow(normalizedImage, 1.0 / gamma, correctedImage);
        
            // Convert the result back to 8-bit format
            Mat resultImage = new Mat();
            correctedImage.ConvertTo(resultImage, DepthType.Cv8U, 255.0);

            // Release unnecessary resources
            normalizedImage.Dispose();
            correctedImage.Dispose();

            // Clear cache if needed (this can be part of optimization or cleanup)
            ClearCacheIfNeeded();
        
            return resultImage;
        }
        catch (Exception ex)
        {
            // Handle exceptions and provide error feedback
            Console.WriteLine($"Error in image processing: {ex.Message}");
            return new Mat();  // Return an empty matrix in case of error
        }
    }
    
}