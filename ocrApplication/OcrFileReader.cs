namespace ocrApplication;

using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Utility class for reading OCR result files.
/// Provides methods to read OCR text from files and convert them to a collection.
/// Made abstract to prevent instantiation as it contains only static methods.
/// </summary>
public abstract class OcrFileReader
{
    /// <summary>
    /// Reads OCR results from a list of file paths and returns them as a list of strings.
    /// Each file's content is read as a separate entry in the returned list.
    /// Files that cannot be read (missing, permission issues, etc.) are skipped with an error message.
    /// </summary>
    /// <param name="filePaths">List of file paths containing OCR results</param>
    /// <returns>List of strings where each string is the content of a file</returns>
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