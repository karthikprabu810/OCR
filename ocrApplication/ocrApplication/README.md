# OCR Application

## Overview
The OCR Application is a powerful command-line tool designed to extract text from images using multiple OCR engines and advanced preprocessing techniques. It implements an ensemble approach to improve OCR accuracy by combining results from different engines and preprocessing methods.

## Problem Statement
Traditional OCR solutions often struggle with:
- Poor quality images
- Various fonts and text styles
- Different languages and character sets
- Inconsistent results across different OCR engines

This application addresses these challenges by:
1. Applying multiple preprocessing techniques to optimize images for OCR
2. Utilizing multiple OCR engines (Tesseract, Google Cloud Vision, IronOCR)
3. Combining results using ensemble techniques
4. Analyzing text similarity to improve final outputs
5. Providing comprehensive performance metrics and comparisons

## Installation and Setup

### Prerequisites
- .NET 8.0 SDK and runtime
- Tesseract OCR engine
- 8GB+ RAM recommended

### Configuration
Create a `config.json` file in the root directory or parent directory with:
```json
{
  "ApiKeys": {
    "GoogleCloudVision": "YOUR_API_KEY"
  },
  "Settings": {
    "TesseractPath": "YOUR_TESSERACT_PATH",
    "TessDataPath": "YOUR_TESSDATA_PATH"
  }
}
```

## Key Components

### 1. ConfigLocator
Automatically detects the configuration file location across different environments, allowing for seamless execution on different machines.

### 2. ImagePreprocessing
Implements various image preprocessing techniques to improve OCR accuracy:
- Binarization (Otsu, Adaptive, etc.)
- Noise reduction
- Contrast enhancement
- Deskewing
- Scaling
- Morphological operations

### 3. OcrExtractionTools
Manages the OCR extraction process using multiple engines:
- Tesseract OCR
- Google Cloud Vision API
- IronOCR

### 4. EnsembleOcr
Combines results from multiple OCR engines and preprocessing techniques using:
- Majority voting
- Confidence-based selection
- Weighted combinations

### 5. TextSimilarity
Provides text comparison and analysis:
- Levenshtein distance
- Cosine similarity
- Jaccard similarity
- Cluster analysis of OCR results

### 6. ExecutionTimeLogger
Tracks and reports on execution time and performance metrics.

### 7. ExportUtilities
Exports OCR results and analysis to various formats:
- Excel (detailed reports)
- PDF (with visualization)
- CSV (raw data)

### Usage

#### Interactive Mode
Run the application without any command-line arguments to enter interactive mode. You will be prompted to provide the input folder, output folder, and select preprocessing methods.

```bash
$ dotnet ocrApplication.dll
```

#### Batch Mode
You can also run the application with command-line arguments for automated processing. The arguments are:

- **inputFolder**: Path to the folder containing images to process
- **outputFolder**: Path where results will be saved
- **preprocessingMethods**: Comma-separated list of preprocessing method numbers or names

```bash
$ dotnet ocrApplication.dll [inputFolder] [outputFolder] [preprocessingMethods]
```

##### Examples

- Process images in `C:\Images` and save results to `C:\Results` using preprocessing methods 1, 3, and 5:

  ```bash
  $ dotnet ocrApplication.dll C:\Images C:\Results 1,3,5
  ```


#### Help
To display help information, use the `-h`, `--help`, or `/?` flags:

```bash
$ dotnet ocrApplication.dll -h
```

## Workflow
1. Images are loaded from the input folder
2. Selected preprocessing methods are applied to each image
3. Results are compared and analyzed
4. Best results are selected based on various metrics
5. Comprehensive reports are generated
6. All results are exported to the specified output folder
