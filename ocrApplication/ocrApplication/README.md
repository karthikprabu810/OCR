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
- Google Cloud Vision API key (optional)
- 8GB+ RAM recommended

### Dependencies
The application relies on the following key packages:
- Tesseract (5.2.0) - Open-source OCR engine
- EmguCV (4.9.0.5494) - OpenCV wrapper for .NET
- OpenCvSharp4 (4.10.0) - Alternative OpenCV wrapper
- EPPlus (7.6.0) - Excel export functionality
- Google.Cloud.Vision.V1 (3.7.0) - Google Cloud Vision API
- IronOcr.MacOs (2025.1.2) - Commercial OCR engine
- TensorFlow.NET (0.150.0) - TensorFlow wrapper for .NET
- ShellProgressBar (5.2.0) - Progress visualization
- Accord.MachineLearning (3.8.0) - For clustering and analysis
- Newtonsoft.Json (13.0.3) - JSON processing

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

## How to Run

This application processes images using Optical Character Recognition (OCR) techniques. It supports both interactive and batch processing modes.

### Features
- Load images from a specified input folder
- Apply various preprocessing techniques
- Perform OCR on images
- Generate reports and visualizations
- Supports command-line interface for batch processing

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

- Process images in `~/Documents/Images` and save results to `~/Documents/Results` using methods "Grayscale", "Binarization", and "Noise Removal":

  ```bash
  $ dotnet ocrApplication.dll ~/Documents/Images ~/Documents/Results "Grayscale,Binarization,Noise Removal"
  ```

#### Help
To display help information, use the `-h`, `--help`, or `/?` flags:

```bash
$ dotnet ocrApplication.dll -h
```

### Dependencies
- .NET 8.0 or higher
- EPPlus for Excel operations
- ShellProgressBar for progress display

### Building the Application
To build the application, navigate to the project directory and run:

```bash
$ dotnet build
```

### Running Tests
To run the unit tests, navigate to the `unitTestProject` directory and execute:

```bash
$ dotnet test
```

### License
This project is licensed under the MIT License.

## Workflow
1. Images are loaded from the input folder
2. Selected preprocessing methods are applied to each image
3. Each preprocessed image is processed by multiple OCR engines
4. Results are compared and analyzed
5. Best results are selected based on various metrics
6. Comprehensive reports are generated
7. All results are exported to the specified output folder

## Example Usage

```bash
# Run the application and provide paths interactively
dotnet run

# Sample interaction:
# Enter the input folder path: /path/to/images
# Enter the output folder path: /path/to/results
# Select preprocessing methods (comma-separated numbers):
# 1. Grayscale
# 2. Binarization
# 3. Noise Removal
# 4. Deskew
# 5. Contrast Enhancement
# Enter your choice: 1,2,3,4,5
```

## Output Structure
The application creates a timestamped output folder containing:
- `processed_images/` - Images after preprocessing
- `ocr_results/` - Raw OCR output files
- `OCR_Results_Summary.xlsx` - Detailed analysis and comparison
- `OCR_Results_BestMethods.xlsx` - Summary of best methods per image
- `OCR_Results_[timestamp].pdf` - PDF report with visualizations

## Performance Considerations
- Processing large images may require significant memory
- Google Cloud Vision API requires internet connectivity
- Ensemble processing increases accuracy but requires more time

## Troubleshooting
- Ensure Tesseract is properly installed and paths are correctly configured
- Check for valid Google Cloud Vision API key if using that engine
- Verify sufficient disk space for output files
- For memory issues, try processing fewer images at once

## Future Enhancements
- Additional OCR engines
- More advanced preprocessing techniques
- Improved ensemble algorithms
- Language-specific optimizations 