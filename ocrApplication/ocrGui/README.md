# OCR GUI Application

## Table of Contents
1. [Overview](#overview)
2. [Features](#features)
3. [Installation and Setup](#installation-and-setup)
   - [Prerequisites](#prerequisites)
   - [Configuration](#configuration)
4. [How to Run](#how-to-run)
5. [User Interface](#user-interface)
   - [Main Window](#main-window)
6. [Workflow](#workflow)
7. [Key Components](#key-components)
   - [MainWindow](#2-mainwindow)
   - [ExitButtonHandler](#3-exitbuttonhandler)
   - [Image Viewing](#4-image-viewing)
   - [OCR Processing](#5-ocr-processing)
   - [Results Display](#6-results-display)
   - [Export Options](#7-export-options)

## Overview
OCR GUI is a user-friendly graphical interface for the OCR functionality provided by the core OCR Application. Built with the Avalonia UI framework, it offers cross-platform compatibility and a modern user experience for OCR processing.

## Features
- **Intuitive User Interface**: Easy-to-use interface for loading and processing images
- **Image Visualization**: View original and preprocessed images
- **Real-time Processing**: See OCR results as they are processed
- **Multiple OCR Engines**: Choose between different OCR engines
- **Preprocessing Options**: Apply various preprocessing techniques through GUI
- **Result Export**: Export results to multiple formats (Excel, PDF, CSV)
- **Cross-platform**: Works on Windows and macOS

## Installation and Setup

### Prerequisites
- .NET 8.0 SDK and runtime
- Tesseract OCR engine
- 8GB+ RAM recommended

It also leverages all the OCR functionality from the core ocrApplication project.

### Configuration
The GUI uses the same configuration as the core OCR application. Ensure you have a `config.json` file in the root directory or parent directory with:
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

## How to Run

```bash
cd ocrGui
dotnet run
```

## User Interface

### Main Window
The main window provides the following functionality:
- Image loading and selection
- Preprocessing method selection
- OCR engine selection
- Processing controls
- Result visualization
- Export options

### Workflow
1. Click "Select Images" to choose images for processing
2. Select desired preprocessing methods from the checkboxes
3. Choose OCR engines to use
4. Click "Process" to start OCR
5. View results in the results panel
6. Export results using the export buttons

## Key Components

### 1. MainWindow
The primary UI component that orchestrates the GUI experience and integrates with the core OCR functionality.

### 2. ExitButtonHandler
Manages application exit and cleanup operations.

### 3. Image Viewing
The application provides functionality to view:
- Original images
- Preprocessed images with different techniques
- Side-by-side comparisons

### 4. OCR Processing
The GUI leverages the same powerful OCR processing capabilities as the command-line application:
- Multiple preprocessing techniques
- Multiple OCR engines
- Ensemble approaches
- Text similarity analysis

### 5. Results Display
Results are displayed in an organized manner:
- Text output by preprocessing method
- Confidence scores
- Performance metrics

### 6. Export Options
The GUI provides multiple export options:
- Excel exports for detailed analysis
- PDF exports with visualizations
- CSV exports for raw data
