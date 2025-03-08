using System.Diagnostics;
using Google.Cloud.Vision.V1;
using IronOcr;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tesseract;

namespace ocrApplication
{
    /// <summary>
    /// Configuration class for OCR settings and API keys.
    /// Used to store configuration loaded from JSON files.
    /// </summary>
    public class OcrConfig
    {
        public string TesseractPath { get; set; }           // Path to the Tesseract OCR executable
        public string TesseractTessDataPath { get; set; }   // Path to the Tesseract data files (language models, etc.)
        public string OcrSpaceApiKey { get; set; }          // API key for OCR.Space service
        public string IronOcrLicenseKey { get; set; }       // License key for IronOCR library
        public string GoogleVisionApiKey { get; set; }      // API key for Google Cloud Vision API
        
        // Used to limit API usage for google vision to avoid exceeding quotas
        public int Counter { get; set; }                    // Counter to track the number of API calls made
        public int Limit { get; set; }                      // Used to limit API usage to avoid exceeding quotas or costs
    }

    /// <summary>
    /// Main class for OCR operations, providing methods to extract text from images
    /// using various OCR engines and APIs. Implements IDisposable to manage resources.
    /// </summary>
    public class OcrExtractionTools : IDisposable
    {
        // Private fields to store configuration settings
        private string _tesseractPath;           // Path to Tesseract OCR executable
        private string _tessDataPath;            // Path to Tesseract language data files
        private string _ocrSpaceApiKey;          // API key for OCR.Space
        private string _ironOcrLicenseKey;       // License key for IronOCR
        private string _googleVisionApiKey;      // API key/credential path for Google Vision
        private bool _disposed = false;          // Flag to track if object has been disposed
        private string _configFilePath;          // Path to the configuration file
        
        
        /// <summary>
        /// Constructor that initializes the OCR tools with settings from a configuration file
        /// </summary>
        /// <param name="configFilePath">Path to the JSON configuration file</param>
        /// <exception cref="InvalidOperationException">Thrown when required API keys are missing</exception>
        public OcrExtractionTools(string configFilePath)
        {
            _configFilePath = configFilePath;
            // Load configuration settings from the JSON file
            var config = LoadConfig(configFilePath);

            // Initialize fields with values from config, with fallbacks where appropriate
            _tesseractPath = config.TesseractPath ?? "tesseract";  // Default to 'tesseract' if not specified
            _tessDataPath = config.TesseractTessDataPath;
            _ocrSpaceApiKey = config.OcrSpaceApiKey;
            _ironOcrLicenseKey = config.IronOcrLicenseKey;
            _googleVisionApiKey = config.GoogleVisionApiKey;

            // Validate that critical API keys are present
            // Without these keys, the OCR services can't be used
            if (string.IsNullOrEmpty(_ocrSpaceApiKey) || string.IsNullOrEmpty(_ironOcrLicenseKey))
            {
                throw new InvalidOperationException("One or more required API keys are missing from the config file.");
            }
        }
        
        /// Loads OCR configuration from a JSON file
        /// <param name="filePath">Path to the JSON configuration file</param>
        /// <returns>Populated OcrConfig object with settings from the file</returns>
        /// <exception cref="FileNotFoundException">Thrown when the config file doesn't exist</exception>
        /// <exception cref="JsonException">Thrown when the JSON is invalid</exception>
        private OcrConfig LoadConfig(string filePath)
        {
            // Read all text from the config file
            var json = File.ReadAllText(filePath);
            // Deserialize the JSON into an OcrConfig object
            return JsonConvert.DeserializeObject<OcrConfig>(json);
        }
        
        /// <summary>
        /// Writes updated configuration back to the JSON file
        /// Used for updating counters and limits after API usage
        /// </summary>
        /// <param name="config">The configuration object to serialize and save</param>
        private void WriteConfigToFile(OcrConfig config)
        {
            // Serialize the config object to JSON with indentation for readability
            string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
            // Write the JSON back to the configuration file
            File.WriteAllText(_configFilePath, jsonContent);
        }

        /// <summary>
        /// Extracts text from an image using Tesseract OCR command line
        /// This method uses the system's installed Tesseract OCR via command line
        /// </summary>
        /// <param name="imagePath">Path to the input image</param>
        /// <param name="outputPath">Path to save the output text (without extension)</param>
        /// <param name="language">Language code for OCR (default: eng for English)</param>
        public void ExtractTextUsingTesseract(string imagePath, string outputPath, string language = "eng")
        {
            // Build the Tesseract command with proper quoting for paths
            string tesseractCommand = $"\"{_tesseractPath}\" \"{imagePath}\" \"{outputPath}\" ";

            // Configure the process to run the command
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",                 // Use bash shell on macOS/Linux
                Arguments = $"-c \"{tesseractCommand}\"", // Pass the command to bash
                RedirectStandardOutput = true,          // Capture output (not used here)
                UseShellExecute = false,                // Don't use the OS shell
                CreateNoWindow = true                   // Don't show a command window
            };

            // Start the process and wait for it to complete
            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();  // Block until the process finishes
            }

            // Log completion message
            Console.WriteLine("Tesseract OCR complete! Check the output text file.");
        }
        
        /// <summary>
        /// Extracts text from an image using Tesseract OCR via .NET library
        /// Uses the Tesseract NuGet package instead of command line
        /// </summary>
        /// <param name="imagePath">Path to the input image</param>
        /// <param name="language">Language code for OCR (default: eng for English)</param>
        /// <returns>Extracted text as a string, or empty string on error</returns>
        public string ExtractTextUsingTesseractWindowsNuGet(string imagePath, string language = "eng")
        {
            try
            {
                // Get path to the Tesseract language data files from configuration
                string tesseractDataPath = _tessDataPath;
        
                // Initialize the Tesseract OCR engine with specified language and mode
                using (var engine = new TesseractEngine(tesseractDataPath, language, EngineMode.Default))
                {
                    // Load the image into Tesseract's internal format (Pix)
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        // Process the image to extract text
                        var result = engine.Process(img);

                        // Return the extracted text
                        return result.GetText();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur during OCR
                Console.WriteLine($"Error during OCR process: {ex.Message}");
                // Return empty string on error
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts text from an image using IronOCR library
        /// IronOCR is a commercial OCR library with its own Tesseract wrapper
        /// </summary>
        /// <param name="imagePath">Path to the input image</param>
        /// <returns>Extracted text as a string</returns>
        public string ExtractTextUsingIronOcr(string imagePath)
        {
            // Set the IronOCR license key from configuration
            License.LicenseKey = _ironOcrLicenseKey;
            // Create a new instance of the IronTesseract OCR engine
            var ocr = new IronTesseract();
            
            // Process the image using IronOCR
            using (var ocrInput = new OcrInput())
            {
                // Load the image into the OCR engine
                ocrInput.LoadImage(imagePath);
                // Perform OCR to extract text
                var ocrResult = ocr.Read(ocrInput);
                // Return the extracted text
                return ocrResult.Text;
            }
        }

        /// <summary>
        /// Extracts text from an image using Google Cloud Vision API
        /// Uses cloud-based OCR for potentially higher accuracy
        /// </summary>
        /// <param name="imagePath">Path to the input image</param>
        /// <returns>Extracted text as a string, or empty string on error</returns>
        public async Task<string> ExtractTextUsingGoogleVisionAsync(string imagePath)
        {
            try
            {
                // Read the current configuration including usage counter and limit
                var config = LoadConfig(_configFilePath);

                // Check if the usage limit has been reached
                if (config.Counter >= config.Limit)
                {
                    throw new InvalidOperationException("OCR process limit has been reached.");
                }

                // Set the environment variable for Google API authentication
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _googleVisionApiKey);

                // Create the Google Vision API client
                var client = ImageAnnotatorClient.Create();

                // Load the image file into memory
                var image = Image.FromFile(imagePath);

                // Send the image to Google Vision API for text detection
                var response = await client.DetectTextAsync(image);

                // Combine all detected text blocks into a single string
                string extractedText = string.Empty;
                foreach (var annotation in response)
                {
                    extractedText += annotation.Description + "\n";
                }
                
                // Increment the API usage counter
                config.Counter++;
                // Log the current usage status
                Console.WriteLine($"Google Vision OCR process: {config.Counter} of {config.Limit}");

                // Save the updated counter back to the config file
                WriteConfigToFile(config);
                
                // Return the extracted text
                return extractedText;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during API communication
                Console.WriteLine($"Error extracting text using Google Vision API: {ex.Message}");
                // Return empty string on error
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts text from an image using OCR.Space API
        /// OCR.Space is a cloud-based OCR service
        /// </summary>
        /// <param name="imagePath">Path to the input image</param>
        /// <returns>Extracted text as a string, or error message on failure</returns>
        public async Task<string> ExtractTextUsingOcrSpaceAsync(string imagePath)
        {
            // Create HttpClient for API communication
            using (var client = new HttpClient())
            {
                // Create a multipart form for sending the image and API key
                var form = new MultipartFormDataContent();

                // Add the image file to the request
                // Open the file as a stream and add it to the form
                form.Add(new StreamContent(File.OpenRead(imagePath)), "file", Path.GetFileName(imagePath));

                // Add the API key to authenticate the request
                form.Add(new StringContent(_ocrSpaceApiKey), "apikey");

                try
                {
                    // Send the HTTP POST request to the OCR.Space API
                    var response = await client.PostAsync("https://api.ocr.space/parse/image", form);
                    // Read the response body as a string
                    var content = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    var json = JObject.Parse(content);

                    // Check if OCR was successful (exit code 1 means success)
                    var ocrExitCode = json["OCRExitCode"].ToString();
                    if (ocrExitCode == "1")
                    {
                        // Extract and return the parsed text from the response
                        return json["ParsedResults"][0]["ParsedText"].ToString();
                    }
                    else
                    {
                        // Return the error message if OCR failed
                        return $"Error: {json["ErrorMessage"]}";
                    }
                }
                catch (Exception ex)
                {
                    // Return error message for any exceptions during API call
                    return $"Error calling OCR.Space API: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Public implementation of Dispose pattern
        /// Releases resources used by the class
        /// </summary>
        public void Dispose()
        {
            // Call the protected Dispose method with disposing=true
            Dispose(true);
            // Tell the garbage collector not to finalize this object
            // since we've already cleaned up resources
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected implementation of Dispose pattern
        /// Allows derived classes to release resources
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            // Only dispose once
            if (!_disposed)
            {
                if (disposing)
                {
                    // Release any managed resources here
                    // Currently no managed resources to release
                }

                // Mark as disposed
                _disposed = true;
            }
        }
    }
}
