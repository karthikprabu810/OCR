using Accord.MachineLearning;
using Accord.Math.Distances;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ocrApplication
{
    /// <summary>
    /// Provides methods for clustering analysis of OCR results and preprocessed images.
    /// Uses machine learning techniques to group similar images and OCR outputs
    /// to identify optimal preprocessing methods and improve OCR accuracy.
    /// </summary>
    public class ClusterAnalysis
    {
        /// <summary>
        /// Performs clustering on the feature vectors extracted from preprocessed images.
        /// Groups similar preprocessing results to identify the most effective techniques.
        /// </summary>
        /// <param name="featureVectors">List of feature vectors for each preprocessed image.</param>
        /// <param name="numClusters">Number of clusters to form.</param>
        /// <returns>A tuple containing cluster labels assigned to each vector, the overall silhouette score,
        /// and individual silhouette scores for each vector/method.</returns>
        /// <remarks>
        /// If there are fewer feature vectors than requested clusters, the method will
        /// duplicate vectors with small random variations to ensure sufficient samples.
        /// </remarks>
        public (int[]? clusterLabels, double silhouetteScore, double[] individualSilhouetteScores) PerformClustering(List<double[]> featureVectors, int numClusters)
        {
            // Ensure we have enough samples for clustering
            if (featureVectors.Count < numClusters)
            {
                // Duplicate feature vectors to ensure we have enough samples
                var originalCount = featureVectors.Count;
                for (int i = 0; i < numClusters - originalCount; i++)
                {
                    // Duplicate with small random variations to avoid identical vectors
                    var vectorToDuplicate = featureVectors[i % originalCount];
                    var duplicatedVector = AddNoiseToVector(vectorToDuplicate);
                    featureVectors.Add(duplicatedVector);
                }
            }

            // Initialize K-means clustering algorithm
            var kmeans = new KMeans(k: numClusters)
            {
                // Set maximum iterations and convergence threshold
                MaxIterations = 100,
                Tolerance = 1e-6
            };

            // Compute the clusters
            int[] labels = kmeans.Learn(featureVectors.ToArray()).Decide(featureVectors.ToArray());

            // Calculate silhouette scores to evaluate clustering quality
            var (overallSilhouetteScore, individualSilhouetteScores) = CalculateSilhouetteScores(featureVectors, labels, numClusters);

            return (labels, overallSilhouetteScore, individualSilhouetteScores);
        }

        /// <summary>
        /// Adds small random noise to a feature vector to create a slightly different version.
        /// Used when duplicating vectors is needed to meet the minimum cluster count.
        /// </summary>
        /// <param name="vector">Original feature vector</param>
        /// <returns>A new vector with small random variations</returns>
        private double[] AddNoiseToVector(double[] vector)
        {
            // Create a new vector to avoid modifying the original
            var result = new double[vector.Length];
            var random = new Random();

            // Add small random variations to each element
            for (int i = 0; i < vector.Length; i++)
            {
                // Add noise in the range of ±5% of the original value
                double noise = vector[i] * (random.NextDouble() * 0.1 - 0.05);
                result[i] = vector[i] + noise;
            }

            return result;
        }

        /// <summary>
        /// Calculates silhouette scores to evaluate clustering quality for all methods.
        /// The silhouette score measures how similar an object is to its own cluster
        /// compared to other clusters. Higher values indicate better clustering.
        /// </summary>
        /// <param name="featureVectors">List of feature vectors</param>
        /// <param name="labels">Cluster labels assigned to each vector</param>
        /// <param name="numClusters">Number of clusters</param>
        /// <returns>A tuple with the overall silhouette score and individual silhouette scores per method</returns>
        private (double overallScore, double[] individualScores) CalculateSilhouetteScores(List<double[]> featureVectors, int[] labels, int numClusters)
        {
            if (featureVectors.Count <= 1 || numClusters <= 1 || numClusters >= featureVectors.Count)
                return (0.0, new double[featureVectors.Count]); // Return zeros for edge cases
                
            double[] silhouetteValues = new double[featureVectors.Count];
            var distance = new Euclidean(); // Using Euclidean distance

            for (int i = 0; i < featureVectors.Count; i++)
            {
                int clusterI = labels[i];
                
                // Calculate the average distance to points in the same cluster (a)
                double a = 0.0;
                int sameClusterCount = 0;
                
                for (int j = 0; j < featureVectors.Count; j++)
                {
                    if (i != j && labels[j] == clusterI)
                    {
                        a += distance.Distance(featureVectors[i], featureVectors[j]);
                        sameClusterCount++;
                    }
                }
                
                // If this is the only point in its cluster
                if (sameClusterCount == 0)
                {
                    silhouetteValues[i] = 0.0;
                    continue;
                }
                
                a /= sameClusterCount;
                
                // Find the average distance to points in the nearest cluster (b)
                double b = double.MaxValue;
                
                for (int otherCluster = 0; otherCluster < numClusters; otherCluster++)
                {
                    if (otherCluster == clusterI)
                        continue;
                        
                    double avgDistance = 0.0;
                    int otherClusterCount = 0;
                    
                    for (int j = 0; j < featureVectors.Count; j++)
                    {
                        if (labels[j] == otherCluster)
                        {
                            avgDistance += distance.Distance(featureVectors[i], featureVectors[j]);
                            otherClusterCount++;
                        }
                    }
                    
                    if (otherClusterCount > 0)
                    {
                        avgDistance /= otherClusterCount;
                        b = Math.Min(b, avgDistance);
                    }
                }
                
                // Calculate silhouette value for this point
                silhouetteValues[i] = (b - a) / Math.Max(a, b);
            }
            
            // Filter out NaN values, which can occur in edge cases
            var validSilhouetteValues = silhouetteValues.Where(s => !double.IsNaN(s)).ToArray();
            
            // Return both the average silhouette score and all individual scores
            double overallScore = validSilhouetteValues.Length > 0 ? validSilhouetteValues.Average() : 0;
            return (overallScore, silhouetteValues);
        }

        /// <summary>
        /// Extracts feature vectors from an image for use in clustering.
        /// These features capture important visual characteristics of the image
        /// that can be used to compare different preprocessing methods.
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Feature vector representing the image</returns>
        public double[] ExtractFeatures(Mat image)
        {
            try
            {
                // Resize image to ensure consistent feature size
                var resizedImage = new Mat();
                CvInvoke.Resize(image, resizedImage, new System.Drawing.Size(256, 256));
                
                // Convert to grayscale if needed
                var grayImage = new Mat();
                if (resizedImage.NumberOfChannels == 3)
                {
                    CvInvoke.CvtColor(resizedImage, grayImage, ColorConversion.Bgr2Gray);
                }
                else
                {
                    grayImage = resizedImage.Clone();
                }

                // Extract features using multiple techniques
                var features = new List<double>();
                
                // 1. Extract basic image statistics
                Mat imgMean = new Mat();
                Mat imgStdDev = new Mat();
                CvInvoke.MeanStdDev(grayImage, imgMean, imgStdDev);
                
                // Get mean and std dev values - fix casting issue
                double mean = imgMean.GetData().Cast<double>().FirstOrDefault() / 255.0;
                double stdDev = imgStdDev.GetData().Cast<double>().FirstOrDefault() / 255.0;
                
                features.Add(mean);
                features.Add(stdDev);
                
                // 2. Extract edge information using Canny
                var cannyImage = new Mat();
                CvInvoke.Canny(grayImage, cannyImage, 100, 200);
                
                // Calculate edge density
                MCvScalar sum = CvInvoke.Sum(cannyImage);
                double edgeDensity = sum.V0 / (cannyImage.Rows * cannyImage.Cols * 255.0);
                features.Add(edgeDensity);
                
                // 3. Add size information
                features.Add(grayImage.Width / 1000.0);
                features.Add(grayImage.Height / 1000.0);
                features.Add((double)grayImage.Width / grayImage.Height);
                
                return features.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting features: {ex.Message}");
                // Return a default feature vector in case of error
                return new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }
    }
}
