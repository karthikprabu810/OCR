using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace ocrGui
{
    /// <summary>
    /// The OCR GUI application that provides a user-friendly interface for the OCR processing.
    /// It handles user input, displays progress information, and shows results.
    /// 
    /// For accurate progress reporting, the OCR application should output progress in the format:
    /// ##PROGRESS:XX.XX
    /// where XX.XX is a floating-point value representing the percentage of completion (0-100).
    /// These lines won't be displayed in the GUI output but will update the progress bar.
    /// </summary>
    public partial class MainWindow : Window
    {
        // UI Controls
        private TextBox? _inputFolderTextBox;
        private TextBox? _outputFolderTextBox;
        private Button? _browseInputButton;
        private Button? _browseOutputButton;
        private Button? _processButton;
        private Button? _exitButton;
        private TextBox? _outputTextBox;
        private TextBox? _userInputTextBox;
        private Button? _sendInputButton;
        private ProgressBar? _progressBar;
        private TextBlock? _progressNameTextBlock;
        private TextBlock? _durationTextBlock;
        private Button? _viewExcelButton;
        private Button? _viewImagesButton;
        private Button? _viewTextButton;
        private string _currentOutputFolder = string.Empty;
        
        // Process vars
        private Process? _currentProcess;
        private bool _waitingForUserInput = false;
        private string _lastPrompt = string.Empty;
        private bool _isProcessing = false;
        private int _totalImagesToProcess = 0;
        private int _imagesProcessed = 0;
        private int _stepsCompleted = 0;
        private int _totalSteps = 0;
        private DateTime _processStartTime;
        private bool _isProcessingCompleted = false;
        // Add a protection time to prevent re-enabling the button too quickly
        private DateTime _lastInputSentTime = DateTime.MinValue;
        private const int INPUT_PROTECTION_MS = 1000; // 1 second protection
        
        // Timer variables for processing duration
        private DateTime _processingStartTime = DateTime.MinValue;
        private TimeSpan _processingDuration = TimeSpan.Zero;
        private bool _processingTimerActive = false;
        private System.Timers.Timer? _uiUpdateTimer;

        // Common OCR prompts that require user input
        private static readonly string[] _inputPromptPatterns = new string[]
        {
            @"Select preprocessing method",
            @"Choose report type",
            @"Do you want to",
            @"Enter your choice",
            @"Enter option",
            @"(\[y/n\])",
            @"\([0-9]+\-[0-9]+\):",
            @"Press any key to continue",
            @"Enter the input folder path:",  // Add input folder prompt
            @"Enter the output folder path:"  // Add output folder prompt
        };

        // Add a progress bar lock flag
        private bool _progressBarLocked = false;

        public MainWindow()
        {
            InitializeComponent();

            _inputFolderTextBox = this.FindControl<TextBox>("InputFolderTextBox");
            _outputFolderTextBox = this.FindControl<TextBox>("OutputFolderTextBox");
            _browseInputButton = this.FindControl<Button>("BrowseInputButton");
            _browseOutputButton = this.FindControl<Button>("BrowseOutputButton");
            _processButton = this.FindControl<Button>("ProcessButton");
            _exitButton = this.FindControl<Button>("ExitButton");
            _outputTextBox = this.FindControl<TextBox>("OutputTextBox");
            _userInputTextBox = this.FindControl<TextBox>("UserInputTextBox");
            _sendInputButton = this.FindControl<Button>("SendInputButton");
            _progressBar = this.FindControl<ProgressBar>("ProgressBar");
            _progressNameTextBlock = this.FindControl<TextBlock>("ProgressNameTextBlock");
            _durationTextBlock = this.FindControl<TextBlock>("DurationTextBlock");
            _viewExcelButton = this.FindControl<Button>("ViewExcelButton");
            _viewImagesButton = this.FindControl<Button>("ViewImagesButton");
            _viewTextButton = this.FindControl<Button>("ViewTextButton");

            // Explicitly disable input controls at startup
            SetInputControlsEnabled(false);
            
            // Show exit button
            if (_exitButton != null)
            {
                _exitButton.IsVisible = true;
                // Add hover effect using Avalonia input events
                _exitButton.AddHandler(PointerEnteredEvent, (s, e) => {
                    if (_exitButton.Background != null)
                        _exitButton.Background = new SolidColorBrush(Color.Parse("#ffebee"));
                }, RoutingStrategies.Direct);
                
                _exitButton.AddHandler(PointerExitedEvent, (s, e) => {
                    if (_exitButton.Background != null)
                        _exitButton.Background = new SolidColorBrush(Colors.Transparent);
                }, RoutingStrategies.Direct);
            }

            if (_browseInputButton != null) _browseInputButton.Click += BrowseInputButton_Click;
            if (_browseOutputButton != null) _browseOutputButton.Click += BrowseOutputButton_Click;
            if (_processButton != null) _processButton.Click += ProcessButton_Click;
            if (_sendInputButton != null) _sendInputButton.Click += SendInputButton_Click;
            if (_exitButton != null) _exitButton.Click += ExitButton_Click;
            if (_viewExcelButton != null)
            {
                _viewExcelButton.Click += ViewExcelButton_Click;
                _viewExcelButton.IsVisible = false;
            }
            
            if (_viewImagesButton != null)
            {
                _viewImagesButton.Click += ViewImagesButton_Click;
                _viewImagesButton.IsVisible = false;
            }
            
            if (_viewTextButton != null) 
            {
                _viewTextButton.Click += ViewTextButton_Click;
                _viewTextButton.IsVisible = false;
            }
            
            // Handle Enter key in input box
            if (_userInputTextBox != null) 
            {
                _userInputTextBox.KeyDown += (sender, e) => 
                {
                    if (e.Key == Key.Enter && _sendInputButton != null && _sendInputButton.IsEnabled)
                    {
                        SendUserInput();
                    }
                };
            }
            
            // Initialize progress bar
            if (_progressBar != null)
            {
                _progressBar.IsVisible = false;
                _progressBar.Minimum = 0;
                _progressBar.Maximum = 100;
                _progressBar.Value = 0;
            }
            
            // Add change events for input/output folder textboxes
            if (_inputFolderTextBox != null) _inputFolderTextBox.PropertyChanged += InputOutputFolder_Changed;
            if (_outputFolderTextBox != null) _outputFolderTextBox.PropertyChanged += InputOutputFolder_Changed;
            
            // Update process button state
            UpdateProcessButtonState();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void InputOutputFolder_Changed(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Text")
            {
                UpdateProcessButtonState();
            }
        }
        
        private void UpdateProcessButtonState()
        {
            if (_processButton != null && _inputFolderTextBox != null && _outputFolderTextBox != null)
            {
                bool inputFolderValid = !string.IsNullOrWhiteSpace(_inputFolderTextBox.Text);
                bool outputFolderValid = !string.IsNullOrWhiteSpace(_outputFolderTextBox.Text);
                
                _processButton.IsEnabled = inputFolderValid && outputFolderValid && !_isProcessing && !_isProcessingCompleted;
            }
        }

        private async void BrowseInputButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var storageProvider = StorageProvider ?? throw new InvalidOperationException("Storage provider not available");
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Input Folder",
                    AllowMultiple = false
                });

                var folder = folders.FirstOrDefault();
                if (folder != null && _inputFolderTextBox != null)
                {
                    _inputFolderTextBox.Text = folder.Path.LocalPath;
                    
                    // Count image files to process
                    _totalImagesToProcess = CountImageFilesInDirectory(_inputFolderTextBox.Text);
                    
                    if (_outputTextBox != null && _totalImagesToProcess > 0)
                    {
                        _outputTextBox.Text = $"Found {_totalImagesToProcess} images to process.\n";
                    }
                    
                    UpdateProcessButtonState();
                }
            }
            catch (Exception ex)
            {
                if (_outputTextBox != null)
                {
                    _outputTextBox.Text += $"Error selecting input folder: {ex.Message}\n";
                }
            }
        }
        
        private int CountImageFilesInDirectory(string directory)
        {
            try
            {
                var imageFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories)
                    .Where(file => 
                        file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                        file.EndsWith(".tif", StringComparison.OrdinalIgnoreCase));
                
                return imageFiles.Count();
            }
            catch
            {
                return 0;
            }
        }

        private async void BrowseOutputButton_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var storageProvider = StorageProvider ?? throw new InvalidOperationException("Storage provider not available");
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Output Folder",
                    AllowMultiple = false
                });

                var folder = folders.FirstOrDefault();
                if (folder != null && _outputFolderTextBox != null)
                {
                    _outputFolderTextBox.Text = folder.Path.LocalPath;
                    UpdateProcessButtonState();
                }
            }
            catch (Exception ex)
            {
                if (_outputTextBox != null)
                {
                    _outputTextBox.Text += $"Error selecting output folder: {ex.Message}\n";
                }
            }
        }

        private async void ProcessButton_Click(object? sender, RoutedEventArgs e)
        {
            // Disable process button to prevent multiple simultaneous processes
            if (_processButton != null)
            {
                _processButton.IsEnabled = false;
            }
            
            // Reset processing state
            _isProcessing = true;
            _isProcessingCompleted = false;
            
            // Hide Excel button when starting a new process
            if (_viewExcelButton != null)
            {
                _viewExcelButton.IsVisible = false;
            }
            
            if (_viewImagesButton != null)
            {
                _viewImagesButton.IsVisible = false;
            }
            
            if (_viewTextButton != null)
            {
                _viewTextButton.IsVisible = false;
            }
            
            // Reset and show progress indicators
            if (_progressBar != null)
            {
                _progressBar.Value = 0;
                _progressBar.IsVisible = true;
                _progressBarLocked = false; // Reset lock flag
            }
            
            if (_progressNameTextBlock != null)
            {
                _progressNameTextBlock.Text = "Starting...";
                _progressNameTextBlock.IsVisible = true;
            }
            
            if (_durationTextBlock != null)
            {
                _durationTextBlock.Text = "00:00:00";
                _durationTextBlock.IsVisible = true;
            }
            
            if (_inputFolderTextBox == null || _outputFolderTextBox == null || _outputTextBox == null)
                return;

            if (string.IsNullOrWhiteSpace(_inputFolderTextBox.Text))
            {
                await ShowErrorDialog("Please select an input folder.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_outputFolderTextBox.Text))
            {
                await ShowErrorDialog("Please select an output folder.");
                return;
            }

            // Disable UI during processing
            SetControlsEnabled(false);
            
            // Explicitly disable input controls at process start
            SetInputControlsEnabled(false);
            
            _outputTextBox.Text = "Starting OCR process...\n";
            
            // Initialize and show progress bar only if it's not locked
            _imagesProcessed = 0;
            if (_progressBar != null && !_progressBarLocked)
            {
                _progressBar.IsVisible = true;
                _progressBar.Value = 0;
                
                // Show progress name text block
                if (_progressNameTextBlock != null)
                {
                    _progressNameTextBlock.IsVisible = true;
                    await UpdateProgressText($"Processing Images (0/{_totalImagesToProcess})");
                }
                
                // Make sure duration text block is initialized correctly
                if (_durationTextBlock != null)
                {
                    _durationTextBlock.Text = "Elapsed Duration: 00:00:00";
                    _durationTextBlock.IsVisible = true;
                }
                
                // If we already know how many images to process, show it
                if (_totalImagesToProcess > 0)
                {
                    _outputTextBox.Text += $"Preparing to process {_totalImagesToProcess} images...\n";
                }
                else
                {
                    // Try to count images again if not already done
                    _totalImagesToProcess = CountImageFilesInDirectory(_inputFolderTextBox.Text);
                    if (_totalImagesToProcess > 0)
                    {
                        _outputTextBox.Text += $"Found {_totalImagesToProcess} images to process.\n";
                    }
                }
            }
            
            try
            {
                await RunOcrApplication(_inputFolderTextBox.Text, _outputFolderTextBox.Text);
            }
            catch (Exception ex)
            {
                if (_outputTextBox != null)
                {
                    _outputTextBox.Text += $"Error: {ex.Message}\n";
                }
            }
        }
        
        private async void SendInputButton_Click(object? sender, RoutedEventArgs e)
        {
            await SendUserInput(); // Now properly awaited
        }
        
        private async Task<bool> SendUserInput()
        {
            if (_currentProcess == null || _currentProcess.HasExited || 
                _userInputTextBox == null || string.IsNullOrEmpty(_userInputTextBox.Text))
                return false;

            try
            {
                string input = _userInputTextBox.Text.Trim();
                await _currentProcess.StandardInput.WriteLineAsync(input);
                await _currentProcess.StandardInput.FlushAsync();

                // Clear input and disable controls
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _userInputTextBox.Text = string.Empty;
                    _waitingForUserInput = false;
                    SetInputControlsEnabled(false);
                });
                
                _lastInputSentTime = DateTime.Now;
                
                // If this input is a preprocessing method selection (value 1-3), start the timer
                if (!_processingTimerActive && _lastPrompt.Contains("preprocessing") && 
                    (input == "1" || input == "2" || input == "3"))
                {
                    StartProcessingTimer();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                if (_outputTextBox != null)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _outputTextBox.Text += $"Error sending input: {ex.Message}\n";
                    });
                }
                return false;
            }
        }
        
        private void CheckForUserInputPrompt(string output)
        {
            // If processing is completed, don't process any more prompts
            if (_isProcessingCompleted)
                return;
            
            // Prevent prompt detection too soon after sending input
            if ((DateTime.Now - _lastInputSentTime).TotalMilliseconds < INPUT_PROTECTION_MS)
            {
                return;
            }
            
            bool isPrompt = false;
            
            // Check for common input prompts
            foreach (var pattern in _inputPromptPatterns)
            {
                if (Regex.IsMatch(output, pattern, RegexOptions.IgnoreCase))
                {
                    isPrompt = true;
                    _lastPrompt = output;
                    break;
                }
            }
            
            // Additional heuristics to identify prompts
            if (!isPrompt)
            {
                // Typical patterns in prompts
                if (output.EndsWith(":") || output.EndsWith("?") || 
                    output.Contains("Enter") || output.Contains("Select") || 
                    output.Contains("Choose") || output.Contains("Option"))
                {
                    isPrompt = true;
                    _lastPrompt = output;
                }
            }
            
            // Only enable input controls if this is a new prompt
            if (isPrompt && !_waitingForUserInput)
            {
                _waitingForUserInput = true;
                
                // Check if this is the preprocessing method prompt to start timer
                if (output.Contains("Select preprocessing method") || output.Contains("preprocessing"))
                {
                    // Start timer when preprocessing method prompt is detected
                    StartProcessingTimer();
                }
                
                // If timer is active and this is a second prompt after images were processed,
                // stop the timer (detects any prompt after preprocessing is done)
                if (_processingTimerActive && _imagesProcessed > 0)
                {
                    _processingTimerActive = false;
                    _uiUpdateTimer?.Stop();
                    _uiUpdateTimer?.Dispose();
                    _uiUpdateTimer = null;
                    
                    // Freeze the final duration
                    _processingDuration = DateTime.Now - _processingStartTime;
                    
                    // Update UI one last time with the final duration
                    Dispatcher.UIThread.InvokeAsync(() => {
                        if (_durationTextBlock != null)
                        {
                            _durationTextBlock.Text = $"Final Duration: {FormatTimeSpan(_processingDuration)}";
                            _durationTextBlock.IsVisible = true;
                        }
                        
                        if (_progressNameTextBlock != null)
                        {
                            _progressNameTextBlock.Text = "Processing Complete";
                        }
                    });
                }
                
                // Enable input controls
                SetInputControlsEnabled(true);
                
                // Set placeholder text based on prompt
                if (_userInputTextBox != null)
                {
                    _userInputTextBox.Watermark = $"Enter your response for: {_lastPrompt.Trim()}";
                    _userInputTextBox.Focus();
                }
            }
        }

        // Add a dedicated method to start the processing timer
        private void StartProcessingTimer()
        {
            if (_processingTimerActive)
                return;
            
            // Stop any existing timer
            _uiUpdateTimer?.Stop();
            _uiUpdateTimer?.Dispose();
            _uiUpdateTimer = null;
            
            _processingStartTime = DateTime.Now;
            _processingTimerActive = true;
            _processingDuration = TimeSpan.Zero;
            
            // Start a timer to update the UI every second
            _uiUpdateTimer = new System.Timers.Timer(1000); // Update every second
            _uiUpdateTimer.Elapsed += (s, e) => {
                if (_processingTimerActive)
                {
                    _processingDuration = DateTime.Now - _processingStartTime;
                    
                    // Update the UI on the UI thread
                    Dispatcher.UIThread.InvokeAsync(() => {
                        if (_durationTextBlock != null)
                        {
                            _durationTextBlock.Text = $"Elapsed Duration: {FormatTimeSpan(_processingDuration)}";
                            _durationTextBlock.IsVisible = true;
                        }
                    });
                }
            };
            _uiUpdateTimer.Start();
            
            // Force immediate UI update
            Dispatcher.UIThread.InvokeAsync(() => {
                if (_durationTextBlock != null)
                {
                    _durationTextBlock.Text = $"Elapsed Duration: 00:00:00";
                    _durationTextBlock.IsVisible = true;
                }
            });
        }

        private void SetInputControlsEnabled(bool enabled)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_userInputTextBox != null)
                {
                    _userInputTextBox.IsEnabled = enabled;
                    if (!enabled)
                    {
                        _userInputTextBox.Text = string.Empty;
                        _userInputTextBox.Watermark = "Enter your response here when prompted...";
                    }
                }
                
                if (_sendInputButton != null)
                {
                    _sendInputButton.IsEnabled = enabled;
                }
            });
        }

        private async Task ShowErrorDialog(string message)
        {
            var errorDialog = new Window
            {
                Title = "Error",
                SizeToContent = SizeToContent.WidthAndHeight
            };
            
            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };
            
            okButton.Click += (sender, e) => errorDialog.Close();
            
            errorDialog.Content = new StackPanel
            {
                Margin = new Thickness(20),
                Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        Margin = new Thickness(0, 0, 0, 20)
                    },
                    okButton
                }
            };

            await errorDialog.ShowDialog(this);
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (_inputFolderTextBox != null) _inputFolderTextBox.IsEnabled = enabled;
            if (_outputFolderTextBox != null) _outputFolderTextBox.IsEnabled = enabled;
            if (_browseInputButton != null) _browseInputButton.IsEnabled = enabled;
            if (_browseOutputButton != null) _browseOutputButton.IsEnabled = enabled;
            
            // Process button state is controlled by UpdateProcessButtonState()
        }

        private async Task RunOcrApplication(string inputFolder, string outputFolder)
        {
            if (_outputTextBox == null) return;

            _processStartTime = DateTime.Now;
            _stepsCompleted = 0;
            _totalSteps = 0;
            _imagesProcessed = 0;  // Reset the counter at start
            
            // Count images before starting
            _totalImagesToProcess = CountImageFilesInDirectory(inputFolder);
            
            var ocrAppPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", 
                "ocrApplication", "bin", "Debug", "net8.0", "ocrApplication.dll"));

            if (!File.Exists(ocrAppPath))
            {
                // Try to find the executable in common locations
                var possiblePaths = new[]
                {
                    System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "ocrApplication", "bin", "Debug", "net8.0", "ocrApplication.dll")),
                    System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ocrApplication", "bin", "Debug", "net8.0", "ocrApplication.dll")),
                    System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ocrApplication.dll"))
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        ocrAppPath = path;
                        break;
                    }
                }

                if (!File.Exists(ocrAppPath))
                {
                    throw new FileNotFoundException("Could not find the OCR application executable. Please build the OCR application first.");
                }
            }

            // Start with a clean output
            _outputTextBox.Text = "Starting OCR process...\n";

            _currentProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"\"{ocrAppPath}\"",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            _currentProcess.OutputDataReceived += async (sender, e) =>
            {
                if (e.Data == null) return;

                string line = e.Data.TrimEnd();
                
                // Handle progress updates
                if (line.StartsWith("##PROGRESS:"))
                {
                    if (float.TryParse(line.Substring(11), out float progress))
                    {
                        await UpdateProgress(progress);
                    }
                    return;
                }

                // Handle processed image updates
                if (line.StartsWith("##PROCESSED:"))
                {
                    _imagesProcessed++;
                    await UpdateProgressText();
                    return;
                }

                // Process and display the output line
                await ProcessOutputLine(line);
            };

            _currentProcess.ErrorDataReceived += async (sender, e) =>
            {
                if (e.Data == null) return;
                await ProcessOutputLine($"Error: {e.Data}");
            };

            try
            {
                _currentProcess.Start();
                _currentProcess.BeginOutputReadLine();
                _currentProcess.BeginErrorReadLine();

                // Write the input and output folder paths to the console input of the OCR application
                await _currentProcess.StandardInput.WriteLineAsync(inputFolder);
                await _currentProcess.StandardInput.WriteLineAsync(outputFolder);
                await _currentProcess.StandardInput.FlushAsync();

                // Monitor for process exit
                _ = Task.Run(async () => 
                {
                    try 
                    {
                        await Task.Run(() => _currentProcess.WaitForExit());
                        
                        Dispatcher.UIThread.InvokeAsync(() => 
                        {
                            _isProcessing = false;
                            _isProcessingCompleted = true;
                            
                            // Lock the progress bar at 100%
                            _progressBarLocked = true;
                            
                            if (_progressBar != null)
                            {
                                _progressBar.Value = 100; // Ensure progress bar reaches 100%
                            }
                            
                            if (_progressNameTextBlock != null)
                            {
                                // Update progress name to show completion
                                _progressNameTextBlock.Text = "Processing Complete";
                            }
                            
                            if (_outputTextBox != null)
                            {
                                _outputTextBox.Text += "\nOCR process has completed.\n";
                                _outputTextBox.Text += "Application is locked. Close and reopen to process more images.\n";
                                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                            }
                            
                            // Keep UI controls disabled
                            SetInputControlsEnabled(false);
                            UpdateProcessButtonState(); // This will ensure process button stays disabled
                        });
                    }
                    catch
                    {
                        // Ignore exceptions during process exit
                    }
                });
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (_outputTextBox != null)
                    {
                        _outputTextBox.Text += $"\nError running OCR application: {ex.Message}\n";
                        _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                    }
                    
                    _isProcessing = false;
                    UpdateProcessButtonState();
                    SetControlsEnabled(true);
                });
                
                _currentProcess = null;
                throw;
            }

            // Store the output folder for later use with Excel viewer
            _currentOutputFolder = outputFolder;
        }
        
        private async Task ProcessOutputLine(string line)
        {
            if (_outputTextBox == null) return;

            // Don't display filtered lines
            if (ShouldFilterOutput(line))
            {
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _outputTextBox.Text += line + "\n";
                _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                
                // Check for user input prompts after adding the line
                if (!_waitingForUserInput)
                {
                    CheckForUserInputPrompt(line);
                }
            });

            // Check for processing complete message
            if (line.Contains("Processing complete.") || line.Contains("OCR process has completed."))
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // Show the result buttons when processing is complete
                    if (_viewExcelButton != null)
                    {
                        _viewExcelButton.IsVisible = true;
                    }
                    
                    if (_viewImagesButton != null)
                    {
                        _viewImagesButton.IsVisible = true;
                    }
                    
                    if (_viewTextButton != null)
                    {
                        _viewTextButton.IsVisible = true;
                    }
                });
            }
        }

        private async Task UpdateProgress(double progress)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                UpdateProgressBar(progress);
            });
        }

        private void UpdateProgressBar(double progress)
        {
            if (_progressBar == null || _progressBarLocked || _isProcessingCompleted)
                return;

            // Ensure progress is between 0 and 100
            progress = Math.Max(0, Math.Min(100, progress));
            
            // Simple direct progress update - no complex calculations
            _progressBar.Value = progress;
            
            // Update the progress text
            _ = UpdateProgressText();
            
            // Update window title with progress
            string remainingText = "";
            if (_processStartTime != DateTime.MinValue && progress < 100)
            {
                var elapsed = DateTime.Now - _processStartTime;
                if (progress > 0)
                {
                    var estimatedTotal = TimeSpan.FromTicks((long)(elapsed.Ticks / (progress / 100.0)));
                    var remaining = estimatedTotal - elapsed;
                    if (remaining.TotalSeconds > 0)
                    {
                        remainingText = $"(Est. {FormatTimeSpan(remaining)} remaining)";
                    }
                }
            }
            
            this.Title = $"OCR Application - {progress:0}% Complete - {remainingText}";
        }

        private async Task UpdateProgressText(string? customText = null)
        {
            if (_progressNameTextBlock == null)
                return;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (customText != null)
                {
                    _progressNameTextBlock.Text = customText;
                    _progressNameTextBlock.IsVisible = true;
                    
                    // Make sure duration is updated
                    if (_durationTextBlock != null)
                    {
                        _durationTextBlock.Text = _processingTimerActive 
                            ? $"Elapsed Duration: {FormatTimeSpan(_processingDuration)}" 
                            : $"Final Duration: {FormatTimeSpan(_processingDuration)}";
                        _durationTextBlock.IsVisible = true;
                    }
                    return;
                }

                // Create progress text showing processed images
                string progressText = "";
                if (_totalImagesToProcess > 0)
                {
                    progressText = $"Processing Images ({_imagesProcessed}/{_totalImagesToProcess})";
                }
                else
                {
                    progressText = $"Processing Images ({_imagesProcessed})";
                }

                _progressNameTextBlock.Text = progressText;
                _progressNameTextBlock.IsVisible = true;

                // Always show duration display
                if (_durationTextBlock != null)
                {
                    _durationTextBlock.Text = _processingTimerActive 
                        ? $"Elapsed Duration: {FormatTimeSpan(_processingDuration)}" 
                        : $"Final Duration: {FormatTimeSpan(_processingDuration)}";
                    _durationTextBlock.IsVisible = true;
                }
            });
        }

        private string FormatTimeSpan(TimeSpan span)
        {
            return $"{(int)span.TotalHours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }

        private void ExitButton_Click(object? sender, RoutedEventArgs e)
        {
            if (_outputTextBox != null)
            {
                ExitButtonHandler.HandleExit(this, _isProcessing, _currentProcess, _outputTextBox);
            }
        }

        private void SetCloseButtonVisibility(bool visible)
        {
            if (_exitButton != null)
            {
                _exitButton.IsVisible = !visible;
            }
        }

        private bool ShouldFilterOutput(string line)
        {
            // Filter out folder path prompts
            if (line.Contains("Enter the input folder path:") || 
                line.Contains("Enter the output folder path:"))
            {
                return true;
            }

            // Check for progress indicators
            if (line.StartsWith("##PROGRESS:") || line.StartsWith("##PROCESSED:"))
            {
                return true;
            }

            return false;
        }

        private async void ViewExcelButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentOutputFolder) || !Directory.Exists(_currentOutputFolder))
            {
                await ShowErrorDialog("Output folder not found. Please try again.");
                return;
            }
            
            // Find all Excel files in the output folder and its subdirectories
            var excelFiles = Directory.GetFiles(_currentOutputFolder, "*.xlsx", SearchOption.AllDirectories);
            
            if (excelFiles.Length == 0)
            {
                await ShowErrorDialog("No Excel files found in the output folder.");
                return;
            }
            
            // Show a file picker if there are multiple Excel files
            if (excelFiles.Length > 1)
            {
                var fileDialog = new OpenFileDialog
                {
                    Title = "Select Excel file to open",
                    AllowMultiple = false,
                    Directory = _currentOutputFolder,
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { Name = "Excel Files", Extensions = new List<string> { "xlsx" } }
                    }
                };
                
                var result = await fileDialog.ShowAsync(this);
                if (result != null && result.Length > 0)
                {
                    OpenExcelFile(result[0]);
                }
            }
            else
            {
                // Only one Excel file, open it directly
                OpenExcelFile(excelFiles[0]);
            }
        }
        
        private void OpenExcelFile(string filePath)
        {
            OpenFile(filePath, "Excel");
        }

        private async void ViewImagesButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentOutputFolder) || !Directory.Exists(_currentOutputFolder))
            {
                await ShowErrorDialog("Output folder not found. Please try again.");
                return;
            }
            
            // Find all image files in the output folder and its subdirectories
            var imageFiles = Directory.GetFiles(_currentOutputFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => {
                    string ext = Path.GetExtension(file).ToLower();
                    return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp" || ext == ".tiff" || ext == ".gif";
                }).ToArray();
            
            if (imageFiles.Length == 0)
            {
                await ShowErrorDialog("No image files found in the output folder.");
                return;
            }
            
            // Show a file picker if there are multiple image files
            if (imageFiles.Length > 1)
            {
                var fileDialog = new OpenFileDialog
                {
                    Title = "Select image file to open",
                    AllowMultiple = false,
                    Directory = _currentOutputFolder,
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { 
                            Name = "Image Files", 
                            Extensions = new List<string> { "jpg", "jpeg", "png", "bmp", "tiff", "gif" } 
                        }
                    }
                };
                
                var result = await fileDialog.ShowAsync(this);
                if (result != null && result.Length > 0)
                {
                    OpenFile(result[0], "image");
                }
            }
            else
            {
                // Only one image file, open it directly
                OpenFile(imageFiles[0], "image");
            }
        }
        
        private async void ViewTextButton_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentOutputFolder) || !Directory.Exists(_currentOutputFolder))
            {
                await ShowErrorDialog("Output folder not found. Please try again.");
                return;
            }
            
            // Find all text files in the output folder and its subdirectories
            var textFiles = Directory.GetFiles(_currentOutputFolder, "*.*", SearchOption.AllDirectories)
                .Where(file => {
                    string ext = Path.GetExtension(file).ToLower();
                    return ext == ".txt" || ext == ".csv" || ext == ".json" || ext == ".xml" || ext == ".html";
                }).ToArray();
            
            if (textFiles.Length == 0)
            {
                await ShowErrorDialog("No text files found in the output folder.");
                return;
            }
            
            // Show a file picker if there are multiple text files
            if (textFiles.Length > 1)
            {
                var fileDialog = new OpenFileDialog
                {
                    Title = "Select text file to open",
                    AllowMultiple = false,
                    Directory = _currentOutputFolder,
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter { 
                            Name = "Text Files", 
                            Extensions = new List<string> { "txt", "csv", "json", "xml", "html" } 
                        }
                    }
                };
                
                var result = await fileDialog.ShowAsync(this);
                if (result != null && result.Length > 0)
                {
                    OpenFile(result[0], "text");
                }
            }
            else
            {
                // Only one text file, open it directly
                OpenFile(textFiles[0], "text");
            }
        }
        
        private void OpenFile(string filePath, string fileType)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                
                Process.Start(startInfo);
                
                if (_outputTextBox != null)
                {
                    _outputTextBox.Text += $"\nOpening {fileType} file: {Path.GetFileName(filePath)}\n";
                    _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                }
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ShowErrorDialog($"Error opening {fileType} file: {ex.Message}");
                    
                    if (_outputTextBox != null)
                    {
                        _outputTextBox.Text += $"\nError opening {fileType} file: {ex.Message}\n";
                        _outputTextBox.CaretIndex = _outputTextBox.Text.Length;
                    }
                });
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
} 