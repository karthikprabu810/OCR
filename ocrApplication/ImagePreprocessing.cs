using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Drawing;
using Emgu.CV.Structure;

namespace ocrApplication;

public static class ImagePreprocessing
{
    // 1. Grayscale Conversion Function
    public static Mat ConvertToGrayscale(string imagePath)
    {
        // Load the image
        Mat image = CvInvoke.Imread(imagePath); // Read image in color mode
        // Mat image = CvInvoke.Imread(imagePath, ImreadModes.Color); // Read image in color mode
        
        // Convert to grayscale
        Mat grayImage = new Mat();
        CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);
        
        if (grayImage.IsEmpty)
        {
            Console.WriteLine("Error loading image: " + imagePath);
        }
        
        return grayImage;
    }
    
    // 2. Noise Removal: Gaussian Blur
    public static Mat RemoveNoiseUsingGaussian(string imagePath)
    {

        int kernelSize = 5;
        Mat image = ConvertToGrayscale(imagePath); // First convert to grayscale if not done
        Mat denoisedImage = new Mat();

        // Apply Gaussian Blur to reduce noise
        CvInvoke.GaussianBlur(image, denoisedImage, new Size(kernelSize, kernelSize), 0);
        
        if (denoisedImage.IsEmpty)
        {
            Console.WriteLine("Gaussian blur failed for " + imagePath);
        }
        
        return denoisedImage;
    }
    
    // 3. Noise Removal: Median Filtering
    public static Mat RemoveNoiseUsingMedian(string imagePath)
    {
        int kernelSize = 3;
        Mat image = ConvertToGrayscale(imagePath); // First convert to grayscale if not done
        Mat denoisedImage = new Mat();

        // Apply Median Filtering to reduce noise
        CvInvoke.MedianBlur(image, denoisedImage, kernelSize);
        
        if (denoisedImage.IsEmpty)
        {
            Console.WriteLine("Median filtering failed for " + imagePath);
        }
        
        return denoisedImage;
    }
    
    // 4. Adaptive Thresholding (Used when lighting varies across the image)
    public static Mat AdaptiveThresholding(string imagePath)
    {
        Mat image = ConvertToGrayscale(imagePath);
        Mat thresholdImage = new Mat();
        
        CvInvoke.AdaptiveThreshold(image, thresholdImage, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 11, 2);
        
        if (thresholdImage.IsEmpty)
        {
            Console.WriteLine("Adaptive thresholding failed for " + imagePath);
        }
        
        return thresholdImage;
    }
    
    // 5. Gamma Correction (Used for correcting the image brightness)
    public static Mat GammaCorrection(string imagePath)
    {
        double gamma = 1.5;
        Mat image = ConvertToGrayscale(imagePath);
        Mat gammaCorrectedImage = new Mat();

        // Create lookup table for gamma correction
        Mat lookUpTable = new Mat(1, 256, DepthType.Cv8U, 1);
        for (int i = 0; i < 256; i++)
        {
            byte[] values = new byte[] { (byte)(Math.Pow(i / 255.0, gamma) * 255.0) };
            lookUpTable.SetTo(values);
        }

        CvInvoke.LUT(image, lookUpTable, gammaCorrectedImage);

        if (gammaCorrectedImage.IsEmpty)
        {
            Console.WriteLine("Gamma correction failed for " + imagePath);
        }

        return gammaCorrectedImage;
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
        int kernelSize = 3;
        Mat image = ConvertToGrayscale(imagePath);
        Mat dilatedImage = new Mat();
        
        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(kernelSize, kernelSize), new Point(-1, -1));
        CvInvoke.Dilate(image, dilatedImage, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
        
        if (dilatedImage.IsEmpty)
        {
            Console.WriteLine("Dilation failed for " + imagePath);
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
        Console.WriteLine($"Calculated Skew Angle: {skewAngle} degrees");

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
}