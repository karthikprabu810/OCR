# OCR GUI Application

## Overview
The OCR GUI Application provides a user-friendly graphical interface for the OCR processing capabilities of the core OCR engine. It allows users to select input and output folders, process images, and view results in real-time.

## Features
- Modern, intuitive user interface built with Avalonia UI
- Folder selection for batch processing
- Real-time progress monitoring
- Interactive console output
- Input handling for interactive OCR processes
- Cross-platform support (Windows, macOS, Linux)

## User Interface Components

### Input Controls
- Input folder selection
- Output folder selection
- Process button to start OCR operations
- Exit button with confirmation dialog

### Progress Monitoring
- Progress bar with percentage display
- Dynamic window title updating with progress information

### Output Display
- Scrollable text output area
- Interactive input section for processes that require user feedback

## Architecture
The GUI application follows the MVVM (Model-View-ViewModel) pattern:
- `MainWindow.axaml` - The main view definition
- `MainWindow.axaml.cs` - Code-behind for the main window
- `ExitButtonHandler.cs` - Handler for application exit functionality
- `App.axaml` and `App.axaml.cs` - Application entry point and configuration

## Dependencies
- Avalonia UI framework 11.1.3
- MessageBox.Avalonia 3.2.0
- .NET 8.0

## Building and Running
1. Ensure you have .NET 8.0 SDK installed
2. Build the solution: `dotnet build ocrGui.csproj`
3. Run the application: `dotnet run --project ocrGui.csproj`

## Usage Guide
1. Launch the application
2. Select an input folder containing images to process
3. Select an output folder where results will be saved
4. Click "Process Images" to start the OCR process
5. Monitor progress in the output area
6. Provide input when prompted
7. Use "Exit Application" to safely close the application

## Notes
- When an OCR process is running, the application will prompt for confirmation before exiting
- The application interface will adapt to show relevant controls based on the current processing state 