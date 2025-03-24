using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ocrGui
{
    /// <summary>
    /// Handles application exit functionality with graceful process termination.
    /// Provides confirmation dialogs and resource cleanup when exiting during active OCR processing.
    /// </summary>
    public static class ExitButtonHandler
    {
        /// <summary>
        /// Handles the exit process for the application with confirmation dialog if OCR processing is active.
        /// Provides graceful termination of any running processes and proper cleanup of resources.
        /// </summary>
        /// <param name="window">The main application window</param>
        /// <param name="isProcessing">Flag indicating if OCR processing is currently active</param>
        /// <param name="currentProcess">Reference to the current OCR process if one is running</param>
        /// <param name="outputTextBox">TextBox control used for displaying output, may need clearing</param>
        /// <remarks>
        /// This method displays a confirmation dialog when attempting to exit during active processing.
        /// If the user confirms, it terminates any active processes and performs cleanup before closing.
        /// </remarks>
        public static async void HandleExit(MainWindow window, bool isProcessing, Process? currentProcess, TextBox? outputTextBox)
        {
            try
            {
                // Only show confirmation dialog if there's an actual running process
                if (isProcessing && currentProcess != null && !currentProcess.HasExited)
                {
                    // Create a simple confirmation dialog
                    var messageBox = new Window
                    {
                        Title = "Confirm Exit",
                        Width = 400,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        CanResize = false,
                        ShowInTaskbar = false,
                        SystemDecorations = SystemDecorations.BorderOnly
                    };

                    var tcs = new TaskCompletionSource<bool>();
                    var panel = new StackPanel { Margin = new Avalonia.Thickness(20) };
                    var message = new TextBlock
                    {
                        Text = "An OCR process is currently running.\nAre you sure you want to exit?",
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Avalonia.Thickness(0, 0, 0, 20)
                    };

                    var buttonPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Spacing = 10
                    };

                    var yesButton = new Button { 
                        Content = "Exit", 
                        Width = 75,
                        Background = new SolidColorBrush(Color.Parse("#DC3545")),
                        Foreground = Brushes.White,
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };
                    
                    var noButton = new Button { 
                        Content = "Stay", 
                        Width = 75,
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };

                    yesButton.Click += (_, _) =>
                    {
                        tcs.SetResult(true);
                        messageBox.Close();
                    };

                    noButton.Click += (_, _) =>
                    {
                        tcs.SetResult(false);
                        messageBox.Close();
                    };

                    messageBox.Closed += (_, _) => 
                    {
                        if (!tcs.Task.IsCompleted)
                        {
                            // If dialog is closed without a button click, assume "No"
                            tcs.SetResult(false);
                        }
                    };

                    buttonPanel.Children.Add(yesButton);
                    buttonPanel.Children.Add(noButton);
                    panel.Children.Add(message);
                    panel.Children.Add(buttonPanel);
                    messageBox.Content = panel;

                    // Show dialog and wait for result
                    await messageBox.ShowDialog(window);
                    
                    // Check user's choice
                    bool shouldExit = await tcs.Task;
                    
                    if (shouldExit)
                    {
                        try
                        {
                            // Kill the process if it's running
                            if (!currentProcess.HasExited)
                            {
                                if (outputTextBox != null)
                                {
                                    outputTextBox.Text += "Terminating OCR process...\n";
                                }
                                
                                try
                                {
                                    // First try a graceful shutdown
                                    currentProcess.CloseMainWindow();
                                    
                                    // Wait a short period for graceful shutdown
                                    if (!currentProcess.WaitForExit(1000))
                                    {
                                        // If not exited gracefully, force kill
                                        currentProcess.Kill(true); // Force kill the process and its children
                                    }
                                    
                                    // Wait to ensure it's terminated
                                    currentProcess.WaitForExit(3000);
                                    
                                    if (outputTextBox != null)
                                    {
                                        outputTextBox.Text += "Process terminated.\n";
                                    }
                                }
                                catch (InvalidOperationException)
                                {
                                    // Process already exited, continue with application close
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (outputTextBox != null)
                            {
                                outputTextBox.Text += $"Error terminating process: {ex.Message}\n";
                            }
                        }
                        finally
                        {
                            // Always close the window even if killing the process failed
                            Environment.Exit(0); // Ensure complete application shutdown
                        }
                    }
                }
                else
                {
                    // No process running or process already completed, just close
                    // If in a locked state, log that we're exiting
                    if (outputTextBox != null)
                    {
                        outputTextBox.Text += "\nExiting application...\n";
                    }
                    
                    // Brief delay to show the exit message
                    await Task.Delay(200);
                    Environment.Exit(0); // Ensure complete application shutdown
                }
            }
            catch (Exception ex)
            {
                // Last resort: log the error and force close
                if (outputTextBox != null)
                {
                    outputTextBox.Text += $"Error during exit: {ex.Message}\n";
                }
                
                // Force close the application after a short delay
                await Task.Delay(500);
                Environment.Exit(1); // Exit with error code
            }
        }
    }
} 