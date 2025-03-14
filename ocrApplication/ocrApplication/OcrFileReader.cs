namespace ocrApplication;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Reads and processes OCR result files.
/// Contains static utilities for text extraction from result files.
/// </summary>
public abstract class OcrFileReader
{
    /// <summary>
    /// Extracts OCR text from multiple files.
    /// Collects content from each file path, skipping inaccessible files.
    /// </summary>
    /// <param name="filePaths">OCR result file locations</param>
    /// <returns>Collection of extracted text contents</returns>
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