# OCR Application Suite

## Overview
This repository contains a comprehensive OCR (Optical Character Recognition) solution designed to extract text from images with high accuracy. The suite includes three main components:

1. **ocrApplication**: A powerful command-line OCR processing tool with advanced image preprocessing techniques and ensemble OCR methods.
2. **ocrGui**: A user-friendly GUI interface for the OCR functionality, built with Avalonia UI framework.
3. **unitTestProject**: A comprehensive test suite to ensure reliability and accuracy of OCR components.

## Key Features
- Cross-platform support (Windows and macOS)
- Multiple OCR engines integration (Tesseract, Google Cloud Vision, IronOCR)
- Advanced image preprocessing techniques
- Ensemble OCR approach for improved accuracy
- Text similarity and comparison analysis
- Performance tracking and benchmarking
- Comprehensive result exports (Excel, PDF, CSV)
- GUI interface for easier interaction

## System Requirements
- **OS**: Windows 10+ or macOS 10.15+
- **Memory**: 8GB minimum, 16GB recommended
- **Storage**: 2GB minimum for application, additional space for OCR processing
- **.NET**: .NET 8.0 SDK and runtime
- **Python**: Python 3.9+ (for certain API integrations)

## Installation and Setup

### 1. Clone the repository:
```bash
git clone https://github.com/yourusername/ocrApplication.git
cd ocrApplication
```

### 2. Install .NET dependencies:
```bash
dotnet restore
```

### 3. Install Tesseract OCR:
- **macOS**:
  ```bash
  brew install tesseract
  ```
  The tesseract directory can be found using 'brew info tesseract' command
  
- **Windows**:
  - Use the NuGet package already included in the project, or
  - Download installer from [UB-Mannheim Tesseract](https://github.com/UB-Mannheim/tesseract/wiki)
  - Make sure to add the tesseract-OCR binaries directory to the PATH environment variable

### 4. Configuration:
Create a `config.json` file in the root directory with the following structure:
```json
{
  "Settings": {
    "TesseractPath": "YOUR_TESSERACT_PATH",
    "TessDataPath": "YOUR_TESSDATA_PATH"
  }
}
```

### 5. Language Data Files:
Extract the `tessdata.zip` file from the assets folder to the appropriate location as specified in your config.json.

## Running the Projects

### OCR Application (Command Line):
```bash
cd ocrApplication
dotnet run
```
Follow the prompts to specify input and output folders.

### OCR GUI:
```bash
cd ocrGui
dotnet run
```

### Unit Tests:
```bash
cd unitTestProject
dotnet test
```

## Project Structure

### ocrApplication
Core OCR processing engine with the following key components:
- `ConfigLocator`: Automatic config file detection
- `ImagePreprocessing`: Image optimization for OCR
- `OcrExtractionTools`: Text extraction from images
- `EnsembleOcr`: Multi-engine OCR combination
- `TextSimilarity`: Text comparison and analysis
- `ExecutionTimeLogger`: Performance tracking
- `ExportUtilities`: Results export to various formats

### ocrGui
User interface for OCR functionalities:
- Built with Avalonia UI framework
- Provides visual feedback for OCR processing
- Supports all core OCR features in a user-friendly interface

### unitTestProject
Comprehensive test suite:
- Unit tests for individual components
- Integration tests for workflow validation
- Mock objects for isolated testing

## Dependencies
See the [requirements.json](assets/requirements.json) file for a detailed list of dependencies.

## Contributors
Karthik Prabu Natarajan