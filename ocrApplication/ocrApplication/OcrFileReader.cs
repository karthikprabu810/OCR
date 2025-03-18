namespace ocrApplication;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Reads and processes OCR result files.
/// This class provides methods to load OCR results from disk and prepare them for further processing.
/// </summary>
public abstract class OcrFileReader
{
    /// <summary>
    /// Extracts OCR text from multiple files.
    /// Collects content from each file path, skipping inaccessible files.
    /// Errors during file reading are logged but don't interrupt the overall process.
    /// </summary>
    /// <param name="filePaths">List of OCR result file locations to read from</param>
    /// <returns>Collection of extracted text contents from all readable files</returns>
    /// <remarks>
    /// This method is resilient to file access errors and will continue processing
    /// other files even if some are inaccessible or corrupted.
    /// </remarks>
    public static List<string> ReadOcrResultsFromFiles(List<string> filePaths)
    {
        // Create a new list to store the OCR results from all files
        var ocrResults = new List<string>();

        // Process each file path in the input list
        foreach (var filePath in filePaths)
        {
            try
            {
                // Read the entire text content of the file
                string ocrText = File.ReadAllText(filePath);
                // Add the file content to the results list
                ocrResults.Add(ocrText);
            }
            catch (Exception ex)
            {
                // Handle and log any errors that occur when reading files
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
                // Continue processing other files even if one fails
            }
        }

        // Return the list of OCR results collected from all successfully read files
        return ocrResults;
    }
}