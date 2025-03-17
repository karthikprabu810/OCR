using Accord.MachineLearning;
using Accord.Math.Distances;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace ocrApplication
{
    public class ClusterAnalysis
    {
        /// <summary>
        /// Performs clustering on the feature vectors extracted from preprocessed images.
        /// </summary>
        /// <param name="featureVectors">List of feature vectors for each preprocessed image.</param>
        /// <param name="numClusters">Number of clusters to form.</param>
        /// <returns>Clustering results including cluster labels and evaluation metrics.</returns>
        public (int[] clusterLabels, double silhouetteScore) PerformClustering(List<double[]> featureVectors, int numClusters)
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
            var clusters = kmeans.Learn(featureVectors.ToArray());

            // Assign each feature vector to a cluster
            int[] labels = clusters.Decide(featureVectors.ToArray());

            // Calculate silhouette score for evaluation
            double silhouetteScore = CalculateSilhouetteScore(featureVectors, labels, numClusters);

            // If we duplicated vectors, return only the labels for original vectors
            if (featureVectors.Count > numClusters)
            {
                labels = labels.Take(Math.Min(numClusters, featureVectors.Count)).ToArray();
            }

            return (labels, silhouetteScore);
        }

        /// <summary>
        /// Adds small random noise to a feature vector to create variation.
        /// </summary>
        /// <param name="vector">Original feature vector.</param>
        /// <returns>Feature vector with added noise.</returns>
        private double[] AddNoiseToVector(double[] vector)
        {
            Random rand = new Random();
            double[] noisyVector = new double[vector.Length];
            
            for (int i = 0; i < vector.Length; i++)
            {
                // Add small random noise (±0.05)
                noisyVector[i] = vector[i] + (rand.NextDouble() * 0.1 - 0.05);
            }
            
            return noisyVector;
        }

        /// <summary>
        /// Calculates the silhouette score for the clustering results.
        /// </summary>
        /// <param name="featureVectors">List of feature vectors.</param>
        /// <param name="labels">Cluster labels for each feature vector.</param>
        /// <param name="numClusters">Number of clusters.</param>
        /// <returns>Silhouette score indicating clustering quality.</returns>
        private double CalculateSilhouetteScore(List<double[]> featureVectors, int[] labels, int numClusters)
        {
            if (featureVectors.Count <= 1 || numClusters <= 1 || numClusters >= featureVectors.Count)
                return 0.0; // Return 0 for edge cases
                
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
            
            // Return the average silhouette score
            return silhouetteValues.Where(s => !double.IsNaN(s)).Average();
        }

        /// <summary>
        /// Extracts feature vectors from an image.
        /// </summary>
        /// <param name="image">The image to extract features from.</param>
        /// <returns>A feature vector representing the image.</returns>
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
