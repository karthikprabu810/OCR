using System.Diagnostics;
using Google.Cloud.Vision.V1;
using IronOcr;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tesseract;

namespace ocrApplication
{
    public class OcrConfig
    {
        public string TesseractPath { get; set; }
        
        public string TesseractTessDataPath { get; set; }
        public string OcrSpaceApiKey { get; set; }
        public string IronOcrLicenseKey { get; set; }
        public string GoogleVisionApiKey { get; set; }
        
        public int Counter { get; set; }
        
        public int Limit { get; set; }
    }

    public class OcrExtractionTools : IDisposable
    {
        private string _tesseractPath;
        private string _tessDataPath;
        private string _ocrSpaceApiKey;
        private string _ironOcrLicenseKey;
        private string _googleVisionApiKey;
        private bool _disposed = false;
        private string _configFilePath;

        public OcrExtractionTools(string configFilePath)
        {
            _configFilePath = configFilePath;
            // Load configuration settings from the JSON file
            var config = LoadConfig(configFilePath);

            _tesseractPath = config.TesseractPath ?? "tesseract";
            _tessDataPath = config.TesseractTessDataPath;
            _ocrSpaceApiKey = config.OcrSpaceApiKey;
            _ironOcrLicenseKey = config.IronOcrLicenseKey;
            _googleVisionApiKey = config.GoogleVisionApiKey;

            // Optionally handle missing values (e.g., throw exception if critical keys are missing)
            if (string.IsNullOrEmpty(_ocrSpaceApiKey) || string.IsNullOrEmpty(_ironOcrLicenseKey))
            {
                throw new InvalidOperationException("One or more required API keys are missing from the config file.");
            }
        }
        
        // Method to load configuration from JSON file
        private OcrConfig LoadConfig(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<OcrConfig>(json);
        }
        
        // Method to write the updated configuration back to the JSON file
        private void WriteConfigToFile(OcrConfig config)
        {
            string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(_configFilePath, jsonContent);
        }

        // Method to extract text using Tesseract OCR
        public void ExtractTextUsingTesseract(string imagePath, string outputPath, string language = "eng")
        {
            string tesseractCommand = $"\"{_tesseractPath}\" \"{imagePath}\" \"{outputPath}\" -l {language}";

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash", // Use bash shell to run the command
                Arguments = $"-c \"{tesseractCommand}\"", // Pass the full command to bash
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                process.WaitForExit();
            }

            Console.WriteLine("Tesseract OCR complete! Check the output text file.");
        }
        
        // Method to extract text using Tesseract OCR via Nuget Package
        public string ExtractTextUsingTesseractWindowsNuGet(string imagePath, string language = "eng")
        {
            try
            {
                // Specify the path to the Tesseract language data files
                string tesseractDataPath = _tessDataPath;  // Make sure this path is correct
        
                // Initialize the Tesseract engine
                using (var engine = new TesseractEngine(tesseractDataPath, language, EngineMode.Default))
                {
                    // Load the image
                    using (var img = Pix.LoadFromFile(imagePath))
                    {
                        // Perform OCR on the image
                        var result = engine.Process(img);

                        // Return the extracted text
                        return result.GetText();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during OCR process: {ex.Message}");
                return string.Empty;
            }
        }


        // Method to extract text using IronOCR API
        public string ExtractTextUsingIronOcr(string imagePath)
        {
            License.LicenseKey = _ironOcrLicenseKey;
            var ocr = new IronTesseract();
            
            using (var ocrInput = new OcrInput())
            {
                ocrInput.LoadImage(imagePath);
                var ocrResult = ocr.Read(ocrInput);
                return ocrResult.Text;
            }
        }

        // Method to extract text using Google Vision API
        public async Task<string> ExtractTextUsingGoogleVisionAsync(string imagePath)
        {
            try
            {
                // Read the current configuration from the file (including the counter and limit)
                var config = LoadConfig(_configFilePath);

                // Check if the counter has reached or exceeded the limit
                if (config.Counter >= config.Limit)
                {
                    throw new InvalidOperationException("OCR process limit has been reached.");
                }

                // Set Google Vision API credentials
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _googleVisionApiKey);

                // Instantiate the Vision API client
                var client = ImageAnnotatorClient.Create();

                // Load the image into memory
                var image = Image.FromFile(imagePath);

                // Perform text detection on the image
                var response = await client.DetectTextAsync(image);

                // Combine all detected text in the response
                string extractedText = string.Empty;
                foreach (var annotation in response)
                {
                    extractedText += annotation.Description + "\n";
                }
                // Increment the counter after the OCR process
                config.Counter++;
                Console.WriteLine($"Google Vision OCR process: {config.Counter} of {config.Limit}");

                // Write the updated configuration back to the file
                WriteConfigToFile(config);
                
                return extractedText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text using Google Vision API: {ex.Message}");
                return string.Empty;
            }
        }

        // Method to extract text using OCR.Space API
        public async Task<string> ExtractTextUsingOcrSpaceAsync(string imagePath)
        {
            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent();

                // Add the image file to the request
                form.Add(new StreamContent(File.OpenRead(imagePath)), "file", Path.GetFileName(imagePath));

                // Add the API key to the request
                form.Add(new StringContent(_ocrSpaceApiKey), "apikey");

                try
                {
                    // Send the request to OCR.Space
                    var response = await client.PostAsync("https://api.ocr.space/parse/image", form);
                    var content = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response
                    var json = JObject.Parse(content);

                    // Check if OCR was successful
                    var ocrExitCode = json["OCRExitCode"].ToString();
                    if (ocrExitCode == "1")
                    {
                        // Return the extracted text
                        return json["ParsedResults"][0]["ParsedText"].ToString();
                    }
                    else
                    {
                        // Handle OCR failure
                        return $"Error: {json["ErrorMessage"]}";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error calling OCR.Space API: {ex.Message}";
                }
            }
        }

        // Dispose method to release resources
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Release any managed resources
                }

                _disposed = true;
            }
        }
    }
}
