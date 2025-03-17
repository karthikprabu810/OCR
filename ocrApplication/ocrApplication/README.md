# OCR Application Core

## Overview
The OCR Application Core is the main engine of the OCR (Optical Character Recognition) solution. It provides a comprehensive set of tools and utilities for extracting, processing, and comparing text from images with high accuracy.

## Features
- Multiple OCR engine support (Google Cloud Vision, Tesseract, IronOCR)
- Ensemble OCR processing with confidence scoring
- Advanced image preprocessing techniques
- Text similarity and comparison algorithms
- Excel and PDF export capabilities
- Performance logging and benchmarking

## Key Components

### OCR Extraction
- `EnsembleOcr.cs` - Combines results from multiple OCR engines for improved accuracy
- `EnsembleOcrWithConfidence.cs` - Adds confidence scoring to OCR results
- `OcrExtractionTools.cs` - Core OCR processing utilities
- `OCRExtractionHelper.cs` - Helper methods for OCR extraction
- `OcrFileReader.cs` - Handles reading files for OCR processing

### Image Processing
- `ImagePreprocessing.cs` - Image preprocessing techniques to improve OCR accuracy

### Text Processing
- `TextSimilarity.cs` - Algorithms for measuring text similarity
- `OcrComparision.cs` - Tools for comparing OCR results
- `ClusterAnalysis.cs` - Tools for processing Image properties

### Utilities
- `ExportUtilities.cs` - Tools for exporting results to Excel and PDF
- `ExecutionTimeLogger.cs` - Performance monitoring and logging

## Configuration
The application is configured via the `ocr_config.json` file, which allows customization of:
- OCR engines to use

## Requirements
- .NET 8.0
- Various NuGet packages (see requirements.json)
- MacOS dependencies for some features

## Usage
The application can be run from the command line or integrated into other applications:

```csharp
// Example usage
var ocrEngine = new EnsembleOcr();
var results = await ocrEngine.ProcessAsync("path/to/image.jpg");
```

## Getting Started
1. Ensure all dependencies are installed
2. Configure `ocr_config.json` for your needs
3. Build the solution
4. Run the application with appropriate parameters 