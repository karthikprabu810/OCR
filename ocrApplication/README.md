# ML 24/25-10 Creating Text from images with OCR API

This OCR application suite provides advanced text extraction from images, integrating preprocessing techniques, multi-engine OCR, similarity analysis, and visualization tools. It supports Tesseract, Google Cloud Vision, and IronOCR for flexible text recognition.

This suite is designed for researchers, data scientists, and professionals who require high-accuracy text extraction, especially in challenging scenarios such as low-quality images, diverse fonts, or mixed languages. The current version provides a comprehensive and reliable solution for extracting and analyzing text from images, aligning with modern OCR technologies to deliver superior results in complex use cases.

<a name="top"></a>

## Table of Contents
1. [Goal of the Experiment](#goal-of-the-experiment)
2. [Features](#features)
3. [System Requirements](#system-requirements)
4. [Installation](#installation)
   - [Prerequisites](#prerequisites)
   - [Setup Steps](#setup-steps)
   - [Configuration](#configuration)
   - [Language Data Files](#language-data-files)
5. [Dependencies](#dependencies)
6. [Usage and Testing](#usage-and-testing)
   - [Command Line Interface](#command-line-interface)
   - [Graphical User Interface](#graphical-user-interface)
   - [Testing](#testing)
7. [Core Modules](#core-modules)
   - [Extending the System](#extending-the-system)
8. [Result and Visualization](#result-and-visualization)

## Goal of the Experiment
The goal of this experiment is to evaluate and optimize the effectiveness of various image preprocessing techniques on the accuracy of Optical Character Recognition (OCR) using the Terrasect SDK. Specifically, this experiment seeks to achieve the following objectives:

1. **Investigate the Impact of Preprocessing**  
   Assess how different image preprocessing transformations such as shifting, rotating, scaling, contrast adjustments, and noise reduction affect the quality of the extracted text from diverse images, including those with varying quality, lighting conditions, and angles.

2. **Optimize OCR Accuracy**  
   Utilize the Terrasect API to extract text from preprocessed images and determine which preprocessing methods yield the most accurate and reliable text extraction.

3. **Evaluate Preprocessing Strategies**  
   Compare the effectiveness of various preprocessing techniques using metrics like cosine similarity, Levenshtein distance, and clustering analysis to identify the preprocessing approach that best improves the OCR output.

4. **Develop a Robust OCR Solution**  
   Design a console application that loads images, applies preprocessing techniques, extracts text using Terrasect, and outputs both the extracted text and a comparative analysis of the OCR results from different preprocessing approaches.

Ultimately, the experiment aims to enhance OCR capabilities and provide a comprehensive understanding of the role of preprocessing in improving OCR outcomes.

## Features

- **Advanced Image Preprocessing**: 24 different preprocessing techniques to optimize images for OCR
- **Multiple OCR Engines**: Integration with Tesseract, Google Cloud Vision, and IronOCR
- **Synthetic Ground Truth**: Combines results from different preprocessing methods and OCR engines through ensemble approach
- **Text Similarity Analysis**: Compares OCR outputs using multiple similarity metrics
- **Performance Tracking**: Detailed execution time logging and performance benchmarking
- **Comprehensive Reporting**: Generates detailed Excel reports with visualizations
- **User-Friendly GUI**: Intuitive interface for easy interaction and result visualization
- **Cross-Platform Compatibility**: Works on Windows and macOS

## System Requirements

- **Operating System**: Windows 10+ or macOS 10.15+
- **Processor**: Multi-core processor recommended
- **Memory**: 8GB RAM minimum, 16GB recommended
- **Storage**: 2GB for installation, additional space for processing
- **Dependencies**: .NET 8.0 SDK and runtime

## Installation

### Prerequisites

1. **Install .NET 8.0 SDK and Runtime**:
   - Download from [Microsoft .NET Website](https://dotnet.microsoft.com/download/dotnet/8.0)

2. **Install Tesseract OCR**:
   - **macOS**:
     ```bash
     brew install tesseract
     ```
   - **Windows**:
     Nuget package provided along with the solution

3. **Optional**:
   - **IronOCR License Key**: Required only if using IronOCR features
   - **Google Vision API Key**: Required only if using Google Vision API features
     - Counter and Quota for vision API usage
   - **LLM Model Access**: Required only if using advanced text analysis features
     - API URL for your preferred LLM model service
     - Authentication credentials if required by the LLM service
     - Sufficient API quota/credits for processing text
     - Python, Flask, and Ollma are required to set up and run the LLM model locally
   - These can be configured in the config.json file as described in the Configuration section

To learn how to set up and configure the LLM model on your local machine, please refer to the [setup guide](assets/README_API.md)

### Setup Steps

1. **Clone the repository**:
   ```bash
   git clone https://github.com/karthikprabu810/OCR.git
   cd ocrApplication
   ```
2. **Set up TessdataPath**

    Create or modify the `config.json` file in the root directory to initialise the tessdata path for tesseract.
See [Configurations](#configuration) for more details.

3. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

4. **Build the solution**:
   ```bash
   dotnet build
   ```

A setup video has been uploaded for easy reference. Use this [youtube link](https://youtu.be/NpfjyuhVfWA)

### Configuration

Create or modify the `config.json` file in the root directory with the following structure:

```json
{
   "TesseractPath": "YOUR_TESSERACT_PATH",
   "TesseractTessDataPath": "YOUR_TESSDATA_PATH",
   "IronOcrLicenseKey": "YOUR_API_KEY",
   "GoogleVisionApiKey": "YOUR_API_KEY",
   "ApiUrl": "YOUR_LLM_API_URL",
   "Counter": 0,
   "Limit": "YOUR_VISION_API_LIMIT"
}
```

The configuration parameters are:
- **TesseractPath**: Path to Tesseract executable (required)
- **TesseractTessDataPath**: Path to Tesseract language data files (required)
- **IronOcrLicenseKey**: License key for IronOCR (optional)
- **GoogleVisionApiKey**: API key for Google Cloud Vision (optional)
- **Counter**: Counter for your google vision API requests (required if GoogleVisionApiKey is entered)
- **Limit**: Vision API Limit to avoid excess charge (required if GoogleVisionApiKey is entered)
- **ApiUrl**: URL endpoint for LLM API service (optional, used for advanced text analysis) (optional)

### Language Data Files:
Extract the `tessdata.zip` file from the assets folder to the appropriate location as specified in your config.json.

## Dependencies
See the [requirements.json](assets/requirements.json) file for a detailed list of dependencies.

## Usage and Testing

### Command Line Interface

Run the OCR application in command-line mode:

```bash
cd ocrApplication
dotnet run -- --input "/path/to/images" --output "/path/to/results" --methods "1,3,5,7"
```
#### Parameters:

- `--input`: Directory containing images to process
- `--output`: Directory to save results
- `--methods`: Comma-separated list of preprocessing method IDs (optional)
- `--help`: Display help information

Example Usage
```bash
cd ocrApplication
dotnet run "/path/to/images" "/path/to/results" "1,3,5,7,9,11,13,15"
```

### Graphical User Interface

Run the OCR application with GUI:

```bash
cd ocrGui
dotnet run
```

1. Select input folder containing images
2. Select output folder for results
3. Choose preprocessing methods
4. Click "Extraxt Text" to start OCR
5. View results and visualizations
6. Export data as needed

### Testing

The application includes a comprehensive test suite designed to ensure both reliability and accuracy:

```bash
cd unitTestProject
dotnet test
```

## Core Modules

The application consists of the following core Modules:

- **ocrApplication**: The core OCR processing engine that handles preprocessing, text extraction, and result export.
- **ocrGui**: A graphical user interface built on top of ocr Application with Avalonia UI, offering intuitive file selection and interactive result visualization.
- **unitTestProject**: A comprehensive test suite ensuring code reliability, accuracy, and performance.

For detailed information on the architecture of the system, please see [`architecture.md`](Documentation/readme/architecture.md).

### Extending the System

This OCR application is designed with a modular architecture, making it easy for developers to extend its functionality.  

If you want to:  
- Add new OCR engines (e.g., integrate another API or library).  
- Implement new preprocessing techniques (e.g., noise reduction, binarization, deskewing).  
- Introduce new text similarity metrics (e.g., alternative distance measures).  

Refer to [`extending.md`](Documentation/readme/extending.md) for detailed instructions on modifying and expanding the application. 

## Result and Visualization

For detailed information on the results and analysis, please see [`results.md`](Documentation/readme/results.md).

For UI and CLI output screenshots, please see, [`visualisation.md`](Documentation/readme/visualisation.md).

[⬆️ Back to Top](#top)