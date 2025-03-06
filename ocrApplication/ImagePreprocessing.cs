using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Drawing;
using Emgu.CV.Structure;


namespace ocrApplication;

public static class ImagePreprocessing
{
    // Cache for loaded images to prevent multiple disk reads
    private static readonly Dictionary<string, Mat> ImageCache = new Dictionary<string, Mat>();
    private static readonly object CacheLock = new object();

    // Helper method to load or get cached image
    private static Mat LoadImage(string imagePath, ImreadModes mode = ImreadModes.Color)
    {
        lock (CacheLock)
        {
            string cacheKey = $"{imagePath}_{mode}";
            if (!ImageCache.TryGetValue(cacheKey, out Mat cachedImage))
            {
                cachedImage = CvInvoke.Imread(imagePath, mode);
                if (!cachedImage.IsEmpty)
                {
                    ImageCache[cacheKey] = cachedImage;
                }
            }
            return cachedImage.Clone(); // Return a clone to prevent modification of cached image
        }
    }

    // Clear cache if memory usage is high
    private static void ClearCacheIfNeeded()
    {
        lock (CacheLock)
        {
            if (ImageCache.Count > 100) // Arbitrary limit, adjust based on your needs
            {
                foreach (var image in ImageCache.Values)
                {
                    image.Dispose();
                }
                ImageCache.Clear();
            }
        }
    }

    // 1. Optimized Grayscale Conversion Function
    public static Mat ConvertToGrayscale(string imagePath)
    {
        try
        {
            // Load image directly in grayscale mode to avoid color conversion
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
    
    // 2. Gaussian Blur
    public static Mat RemoveNoiseUsingGaussian(string imagePath)
    {
        try
        {
            // Load directly in grayscale
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for Gaussian blur: " + imagePath);
                return new Mat();
            }

            Mat denoisedImage = new Mat();
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
    
    // 3.  Median Blur
    public static Mat RemoveNoiseUsingMedian(string imagePath)
    {
        try
        {
            // Load directly in grayscale
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for median blur: " + imagePath);
                return new Mat();
            }

            Mat denoisedImage = new Mat();
            CvInvoke.MedianBlur(image, denoisedImage, 3);
            
            ClearCacheIfNeeded();
            return denoisedImage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in median blur: {ex.Message}");
            return new Mat();
        }
    }
    
    // 4. Optimized Adaptive Thresholding
    public static Mat AdaptiveThresholding(string imagePath)
    {
        try
        {
            // Load directly in grayscale
            Mat image = LoadImage(imagePath, ImreadModes.Grayscale);
            if (image.IsEmpty)
            {
                Console.WriteLine("Error loading image for adaptive threshold: " + imagePath);
                return new Mat();
            }

            Mat thresholdImage = new Mat();
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
    
    // 5. Gamma Correction (Used for correcting the image brightness)
    
    public static Mat GammaCorrection(string imagePath)
    {
        Mat image = ConvertToGrayscale(imagePath);  // Convert to grayscale (if not already)
        double gamma = EstimateGamma(image);  // Estimate gamma based on image lighting
        Mat gammaCorrectedImage = new Mat();

        // Create lookup table for gamma correction
        byte[] lut = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            lut[i] = (byte)(Math.Pow(i / 255.0, gamma) * 255.0);
        }

        // Create the lookup table Mat using the lut array
        Mat lookUpTable = new Mat(1, 256, DepthType.Cv8U, 1);
        Marshal.Copy(lut, 0, lookUpTable.DataPointer, 256);

        // Apply the LUT (gamma correction)
        CvInvoke.LUT(image, lookUpTable, gammaCorrectedImage);

        if (gammaCorrectedImage.IsEmpty)
        {
            Console.WriteLine("Gamma correction failed for " + imagePath);
        }

        return gammaCorrectedImage;
    }

    public static double EstimateGamma(Mat image)
    {
        // Compute the histogram of the image
        int[] hist = new int[256];
    
        // Convert image to a byte array (this avoids unsafe code)
        byte[] imageData = image.ToImage<Gray, byte>().Bytes;
    
        // Loop through the byte array to access pixel values
        for (int i = 0; i < imageData.Length; i++)
        {
            byte pixelValue = imageData[i];
            hist[pixelValue]++;
        }

        // Compute the mean intensity
        double totalIntensity = 0;
        int totalPixels = imageData.Length;
        for (int i = 0; i < 256; i++)
        {
            totalIntensity += hist[i] * i;
        }
        double meanIntensity = totalIntensity / totalPixels;

        // Set gamma based on the mean intensity
        double gamma = 1.5;  // Default value
        if (meanIntensity < 85)
        {
            gamma = 2.0;  // Dark image, brighten it
        }
        else if (meanIntensity > 170)
        {
            gamma = 0.8;  // Bright image, darken it
        }

        return gamma;
    }
    
    // 6. Canny Edge Detection (Used to detect edges in an image)
    public static Mat CannyEdgeDetection(string imagePath)
    {
        Mat image = ConvertToGrayscale(imagePath);
        Mat edgeDetectedImage = new Mat();
        CvInvoke.Canny(image, edgeDetectedImage, 100, 200);
        if (edgeDetectedImage.IsEmpty)
        {
            Console.WriteLine("Canny edge detection failed for " + imagePath);
        }
        return edgeDetectedImage;
    }

    // 7. Morphological Operations (Dilation)
    public static Mat Dilation(string imagePath)
    {
        int kernelSize = 3; // You can change this value based on your needs
        if (kernelSize % 2 == 0) 
        {
            kernelSize += 1; // Ensure kernel size is odd
        }

        // Convert image to grayscale
        Mat image = ConvertToGrayscale(imagePath);
        if (image == null || image.IsEmpty)
        {
            Console.WriteLine("Error: Unable to load or convert image " + imagePath);
            return null;
        }

        Mat dilatedImage = new Mat();

        // Create a rectangular structuring element (kernel)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));

        // Perform dilation
        try
        {
            CvInvoke.Dilate(image, dilatedImage, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

            // Check if dilation was successful
            if (dilatedImage.IsEmpty)
            {
                Console.WriteLine("Dilation failed for " + imagePath);
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during dilation: " + ex.Message);
            return null;
        }

        return dilatedImage;
    }

    // 8. Morphological Operations (Erosion)
    public static Mat Erosion(string imagePath)
    {
        int kernelSize = 3;
        Mat image = ConvertToGrayscale(imagePath);
        Mat erodedImage = new Mat();
        
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
        CvInvoke.Erode(image, erodedImage, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
        
        if (erodedImage.IsEmpty)
        {
            Console.WriteLine("Erosion failed for " + imagePath);
        }
        
        return erodedImage;
    }
    
    // 9. Binarization Improvements (Otsu's Method)
    public static Mat OtsuBinarization(string imagePath)
    {
        // Step 1: Read the image in grayscale mode
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        
        // Step 2: Apply Otsu's method to perform automatic thresholding
        Mat binaryImage = new Mat();
        try
        {
            CvInvoke.Threshold(image, binaryImage, 0, 255, ThresholdType.Otsu);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: Otsu binarization failed for image at path: {imagePath}. Error code: 1502. Exception: {ex.Message}");
            return new Mat(); // Return empty Mat if thresholding fails
        }

        return binaryImage;
    }
    
    // 10. Deskew the image (rotate to correct skew) (Used to fix skewed images)
    public static Mat Deskew(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty)
        {
            throw new Exception("Failed to load image: " + imagePath);
        }

        Mat edges = new Mat();
        CvInvoke.Canny(image, edges, 50, 150);  // Detect edges

        // Detect lines using Hough Transform
        LineSegment2D[] lines = CvInvoke.HoughLinesP(edges, 1, Math.PI / 180, 100, 100, 10);
        
        if (lines.Length == 0)
        {
            Console.WriteLine("No lines detected, returning original image.");
            return image;
        }

        // Compute average angle of detected lines
        double totalAngle = 0;
        int count = 0;

        foreach (var line in lines)
        {
            double angle = Math.Atan2(line.P2.Y - line.P1.Y, line.P2.X - line.P1.X) * 180 / Math.PI;
            if (Math.Abs(angle) < 45)  // Ignore vertical lines
            {
                totalAngle += angle;
                count++;
            }
        }

        if (count == 0) return image; // If no valid angle found, return original

        double skewAngle = totalAngle / count;
        //Console.WriteLine($"Calculated Skew Angle: {skewAngle} degrees");

        // Rotate image to correct skew
        return RotateImage(image, -skewAngle);
    }

    // Rotate image without distortion
    private static Mat RotateImage(Mat src, double angle)
    {
        PointF center = new PointF(src.Width / 2f, src.Height / 2f);
        Mat rotationMatrix = new Mat();
        CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);
        
        Mat rotated = new Mat();
        CvInvoke.WarpAffine(src, rotated, rotationMatrix, src.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(255, 255, 255));

        return rotated;
    }
    
    // 12. Histogram Equilisation
    public static Mat HistogramEqualization(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        Mat equalizedImage = new Mat();
        CvInvoke.EqualizeHist(image, equalizedImage);
        return equalizedImage;
    }
    
    

    // 14 Bilateral filtering
    
    public static Mat BilateralFilter(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath);
        if (image.IsEmpty) return image;
        Mat filteredImage = new Mat();
        CvInvoke.BilateralFilter(image, filteredImage, 9, 75, 75);
        return filteredImage;
    }
    
    // 16. Edge Detection (Sobel)
    
    public static Mat SobelEdgeDetection(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        Mat edges = new Mat();
        CvInvoke.Sobel(image, edges, DepthType.Cv8U, 1, 0, 3, 1, 0,BorderType.Reflect);
        return edges;
    }

    
    // 17. Edge Detection (Laplacian)
    
    public static Mat LaplacianEdgeDetection(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        Mat edges = new Mat();
        CvInvoke.Laplacian(image, edges, DepthType.Cv8U, 3, 1, 0, BorderType.Reflect);
        return edges;
    }

    // 18. Image Normalization
    
    public static Mat NormalizeImage(string imagePath)
    {
        double newMin = 0; 
        double newMax = 1;
        Mat image = CvInvoke.Imread(imagePath);
        if (image.IsEmpty) return image;
    
        Mat normalizedImage = new Mat();
        CvInvoke.Normalize(image, normalizedImage, newMin, newMax, NormType.MinMax, DepthType.Cv8U, null);
        return normalizedImage;
    }
    
    // 19. Morphological Operations (Opening)
    
    public static Mat Opening(string imagePath)
    {
        int kernelSize = 5;
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        Mat openedImage = new Mat();
        
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
   
        if (image.IsEmpty) return image;
        CvInvoke.MorphologyEx(image, openedImage, MorphOp.Open, kernel, new Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(0));
        return openedImage;
    }
    
    // 20. Morphological Operations (Closing)
    
    public static Mat Closing(string imagePath)
    {
        int kernelSize = 5;
        Mat image = CvInvoke.Imread(imagePath, ImreadModes.Grayscale);
        Mat closedImage = new Mat();
        
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
   
        if (image.IsEmpty) return image;
        CvInvoke.MorphologyEx(image, closedImage, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

        return closedImage;
    }

    // 21. Morphological Gradient
    public static Mat MorphologicalGradient(string imagePath)
    {
        int kernelSize = 5;
        // Load image in grayscale
        Mat image = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
        if (image.IsEmpty) return image;

        Mat gradientImage = new Mat();
    
        // Create structuring element (5x5 rectangle)
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));

        // Corrected MorphologyEx with 8 parameters
        CvInvoke.MorphologyEx(image, gradientImage, Emgu.CV.CvEnum.MorphOp.Gradient, kernel, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(0));
    
        return gradientImage;
    }
    
    // 22. Log Transform
    public static Mat LogTransform(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
    
        Mat logImage = new Mat();
        image.ConvertTo(logImage, DepthType.Cv32F);
        CvInvoke.Pow(logImage + 1, 1.0, logImage);  // Apply log transformation
        logImage.ConvertTo(logImage, DepthType.Cv8U); // Convert back to original depth
        return logImage;
    }
    
    // 23. Color Space Transformation (BGR to HSV)
    public static Mat ConvertToHSV(string imagePath)
    {
        Mat image = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Color);
        if (image.IsEmpty) return image;
        Mat hsvImage = new Mat();
        CvInvoke.CvtColor(image, hsvImage, Emgu.CV.CvEnum.ColorConversion.Bgr2Hsv);
        return hsvImage;
    }
    
    // 24. Top-Hat Morphological Operations
    public static Mat TopHat(string imagePath)
    {
        int kernelSize = 5;
        
        Mat image = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
        
        Mat openedImage = new Mat();
        CvInvoke.MorphologyEx(image, openedImage, Emgu.CV.CvEnum.MorphOp.Open, kernel, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(0));

        // Subtract the opened image from the original image to get the TopHat result
        Mat topHatImage = new Mat();
        CvInvoke.Subtract(image, openedImage, topHatImage);
        
        return topHatImage;
    }
    // 25. Black-Hat Morphological Operations
    public static Mat BlackHat(string imagePath)
    {
        int kernelSize = 5;
        Mat image = CvInvoke.Imread(imagePath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
        if (image.IsEmpty) return image;
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
        Mat closedImage = new Mat();
        CvInvoke.MorphologyEx(image, closedImage, Emgu.CV.CvEnum.MorphOp.Close, kernel, new System.Drawing.Point(-1, -1), 1, Emgu.CV.CvEnum.BorderType.Reflect, new MCvScalar(0));
        // Subtract the original image from the closed image to get the BlackHat result
        Mat blackHatImage = new Mat();
        CvInvoke.Subtract(closedImage, image, blackHatImage);
        return blackHatImage;
    }

    

    
    
}