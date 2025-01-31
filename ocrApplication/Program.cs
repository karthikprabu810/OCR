using System;
using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        // Define the paths
        string imagePath = @"/Users/karthikprabu/Downloads/seLab/seProj/trainData/multipleColumns.png";  // Full path to your image file
        string outputPath = @"/Users/karthikprabu/Downloads/AA/output_202";    // Output text file (no extension)

        // Construct the Tesseract command
        string tesseractCommand = $"tesseract \"{imagePath}\" \"{outputPath}\" -l eng";

        // Initialize the process to run the command
        ProcessStartInfo processStartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",  // Use bash shell to run the command
            Arguments = $"-c \"{tesseractCommand}\"",  // Pass the full command to bash
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(processStartInfo))
        {
            // Wait for the process to finish
            process.WaitForExit();
        }

        // Output the result
        Console.WriteLine("OCR complete! Check the output text file.");
    }
}