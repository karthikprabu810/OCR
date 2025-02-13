namespace ocrApplication;

using System;
using System.Collections.Generic;
using System.IO;

public class OcrFileReader
{
    // Method to read OCR results from files and return them as a list of strings
    public static List<string> ReadOcrResultsFromFiles(List<string> filePaths)
    {
        var ocrResults = new List<string>();

        foreach (var filePath in filePaths)
        {
            try
            {
                string ocrText = File.ReadAllText(filePath);
                ocrResults.Add(ocrText);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file {filePath}: {ex.Message}");
            }
        }

        return ocrResults;
    }
}
