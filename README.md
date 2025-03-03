# OCR Preprocessing Comparison and Analysis Tool

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-%3E%3D6.0-blue)](https://dotnet.microsoft.com/download)

A comprehensive OCR analysis tool that evaluates and compares the effectiveness of different image preprocessing techniques on OCR accuracy. This tool helps identify the most effective preprocessing approaches for various document types by systematically analyzing how different preprocessing methods affect OCR results.

## 📋 Table of Contents
- [Features](#-features)
- [Requirements](#-requirements)
- [Installation](#-installation)
- [Usage](#-usage)
- [Technical Details](#-technical-details)
- [Output Analysis](#-output-analysis)
- [Contribution](#-contribution)
- [License](#-license)

## ✨ Features

- Multiple image preprocessing techniques
- Automated OCR analysis with Tesseract
- Comprehensive result comparison
- Performance metrics and visualization
- Parallel processing support
- Detailed analysis reports

## 📋 Requirements

### Windows
- Windows 10/11
- .NET 6.0 or higher
- Visual Studio 2019/2022 or Visual Studio Code

### macOS
- macOS 10.15 or higher
- .NET 6.0 or higher
- Homebrew Package Manager
- Visual Studio Code or Visual Studio for Mac

### Common Requirements
- Minimum 4GB RAM (8GB recommended)

## 🚀 Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/karthikprabu810/OCR
   cd OCR
   ```

2. Install dependencies based on your operating system:

   ### Windows (Using NuGet)
   ```bash
   dotnet add package Tesseract            # OCR engine
   dotnet add package Emgu.CV              # Image processing
   dotnet add package Emgu.CV.runtime.windows  # Windows runtime
   dotnet add package System.Drawing.Common    # Image handling
   ```

   Or using Visual Studio:
   - Right-click on the project in Solution Explorer
   - Select "Manage NuGet Packages"
   - Install the following packages:
     - Tesseract
     - Emgu.CV
     - Emgu.CV.runtime.windows
     - System.Drawing.Common

   ### macOS
   First, ensure you have Homebrew installed. If not, install it:
   ```bash
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   ```

   Then install Tesseract:
   ```bash
   brew install tesseract
   ```

   Install .NET packages:
   ```bash
   dotnet add package Emgu.CV
   dotnet add package Emgu.CV.runtime.macos
   dotnet add package System.Drawing.Common
   ```

3. Configure the application:
   - Copy `config.example.json` to `config.json`
   - Update the configuration based on your OS:

   ### Windows Configuration
   ```json
   {
     "TesseractPath": "/usr/local/bin/tesseract",
     "TesseractTessDataPath": "/usr/local/share/tessdata"
   }
   ```

   ### macOS Configuration
   ```json
   {
     "TesseractPath": "/usr/local/bin/tesseract",
     "TesseractTessDataPath": "/usr/local/share/tessdata"
   }
   ```

## 💻 Usage

1. Prepare your images:
   - Place input images in the `input` directory
   - Supported formats: PNG, JPEG, TIFF, BMP

2. Run the application:
   ### Windows
   - Using Visual Studio:
     - Open the solution
     - Press F5 to run
   - Using Command Line:
     ```bash
     dotnet run
     ```

   ### macOS
   ```bash
   dotnet run
   ```

3. Find results in the `output` directory:
   - Preprocessed images
   - OCR results
   - Analysis reports
   - Performance metrics

## 🔧 Technical Details

### Preprocessing Pipeline

1. **Basic Processing**
   - `ConvertToGrayscale`: Converts color images to grayscale using EmguCV
   - `RemoveNoiseUsingGaussian`: Applies Gaussian blur (kernel size: 5x5)
   - `RemoveNoiseUsingMedian`: Applies median filter for noise reduction

2. **Advanced Processing**
   - `AdaptiveThresholding`: Local thresholding for varying lighting
   - `OtsuBinarization`: Automatic global thresholding
   - `GammaCorrection`: Dynamic brightness adjustment
   - `CannyEdgeDetection`: Edge detection for text boundaries
   - `Dilation`/`Erosion`: Morphological operations

### Analysis Methods

1. **Text Similarity**
   - Levenshtein Distance
   - Cosine Similarity
   - N-gram Comparison
   - Word Error Rate (WER)

2. **Performance Metrics**
   - Processing time
   - Memory usage
   - OCR confidence scores

## 📊 Output Analysis

The tool generates comprehensive reports including:

1. **Preprocessing Results**
   - Side-by-side comparisons
   - Visual quality metrics
   - Processing time analysis

2. **OCR Analysis**
   - Text extraction accuracy
   - Confidence scores
   - Error patterns
   - Recognition rates

3. **Visualizations**
   - Similarity matrices
   - Performance charts
   - Quality metrics

## 🤝 Contribution

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [Tesseract OCR](https://github.com/tesseract-ocr/tesseract)
- [EmguCV](https://www.emgu.com/)
- [.NET Core](https://dotnet.microsoft.com/)

## 📧 Contact

Your Name - [Karthik Prabu](https://github.com/karthikprabu810)

Project Link: [OCR Comparison and Analysis Tool](https://github.com/karthikprabu810/OCR) 
