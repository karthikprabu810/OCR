# OCR Application Solution

## Project Overview

This solution implements an advanced Optical Character Recognition (OCR) system that uses multiple preprocessing techniques and OCR engines to improve text extraction accuracy from images. The solution leverages the Tesseract SDK along with other OCR tools to extract text from images after applying various preprocessing transformations.

## Solution Structure

The solution consists of two main projects:

```
OCR/
├── ocrApplication/           # Main OCR application
├── TestProject/              # Test suite for the application
├── Input/                    # Directory for input images
└── Output/                   # Directory for processed outputs
```

## Key Features

- **Image Preprocessing**: Implements 22 different preprocessing techniques including grayscale conversion, thresholding, noise reduction, edge detection, and document deskewing to enhance image quality before OCR.

- **OCR Engine Integration**: Integrates with multiple OCR engines including Tesseract, IronOCR, and Google Cloud Vision to extract text from images.

- **Ensemble Approach**: Combines results from different OCR engines and preprocessing techniques using a majority voting system to improve overall accuracy.

- **User-Defined Preprocessing**: Allows users to select which preprocessing techniques to apply, rather than using all techniques for every image.

- **Performance Metrics**: Tracks execution time and memory usage for both preprocessing and OCR operations to identify bottlenecks.

- **Comprehensive Reporting**: Generates detailed reports with similarity matrices, embeddings visualization, and effectiveness rankings for different preprocessing methods.

## Prerequisites

- .NET 8.0 SDK
- Required dependencies (see individual project README files for details)
- APIs and SDKs:
  - Tesseract OCR
  - IronOCR
  - Google Cloud Vision API

## Getting Started

1. Clone this repository
2. Configure the OCR engines in `ocrApplication/ocr_config.json`
3. Build the solution:
   ```
   dotnet build
   ```
4. Run the application:
   ```
   cd ocrApplication
   dotnet run
   ```
5. Run the tests:
   ```
   cd TestProject
   dotnet test
   ```

## Usage

1. Place images to be processed in the `Input/` directory
2. Run the application and follow the prompts:
   - Specify input and output directory paths when prompted
   - Select which preprocessing techniques to apply
3. The application will process all images and generate the results in the specified output directory
4. Review the results in the `Output/` directory, including:
   - Preprocessed images
   - OCR results text files
   - Comparative analysis Excel spreadsheets

## Implementation Details

### Image Preprocessing

The application offers a wide range of preprocessing techniques to enhance image quality before OCR:

- Basic conversions (grayscale, HSV)
- Noise reduction (Gaussian filter, median filter, bilateral filter)
- Binarization (Otsu, adaptive thresholding)
- Edge detection (Canny, Sobel, Laplacian)
- Morphological operations (dilation, erosion, opening, closing)
- Enhancement techniques (gamma correction, histogram equalization, log transform)
- Document optimization (deskewing, normalization)

### OCR Extraction

Multiple OCR engines are used to extract text from preprocessed images:

- Tesseract OCR: Open-source OCR engine
- IronOCR: Commercial OCR library
- Google Cloud Vision: Cloud-based OCR service

### Ensemble OCR

The application combines results from multiple OCR engines and preprocessing methods using:

- Majority voting
- Similarity-based weighting
- Confidence scores

### Comparative Analysis

The application generates detailed reports to evaluate the effectiveness of different preprocessing techniques:

- Similarity matrices using cosine similarity and Levenshtein distance
- Text embeddings visualization
- Preprocessing effectiveness rankings
- Performance metrics (execution time, memory usage)

## Further Documentation

- See the [ocrApplication README](./ocrApplication/README.md) for details on the main application
- See the [TestProject README](./TestProject/README.md) for information on the test suite

## Contributing

Contributions are welcome! Please feel free to submit pull requests to improve the OCR application.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 