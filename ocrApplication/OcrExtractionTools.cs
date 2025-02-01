namespace ocrApplication;

using System;
using System.Diagnostics;

public class OcrExtractionTools
{
    private string _tesseractPath;

    // Constructor for Tesseract path configuration
    public OcrExtractionTools(string tesseractPath = "tesseract")
    {
        _tesseractPath = tesseractPath;
    }

    // Method for extracting text using Tesseract
    public void ExtractTextUsingTesseract(string imagePath, string outputPath, string language = "eng")
    {
        // Construct the Tesseract command
        string tesseractCommand = $"\"{_tesseractPath}\" \"{imagePath}\" \"{outputPath}\" -l {language}";

        // Initialize the process to run the command
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",  // Use bash shell to run the command
            Arguments = $"-c \"{tesseractCommand}\"",  // Pass the full command to bash
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Start the process and wait for it to complete
        using (Process process = Process.Start(processStartInfo))
        {
            process.WaitForExit();
        }

        Console.WriteLine("Tesseract OCR complete! Check the output text file.");
    }
}
