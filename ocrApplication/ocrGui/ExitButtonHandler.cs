using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ocrGui
{
    public static class ExitButtonHandler
    {
        public static async void HandleExit(MainWindow window, bool isProcessing, Process? currentProcess, TextBox? outputTextBox)
        {
            try
            {
                if (isProcessing && currentProcess != null && !currentProcess.HasExited)
                {
                    // Create a simple confirmation dialog
                    var messageBox = new Window
                    {
                        Title = "Confirm Exit",
                        Width = 400,
                        Height = 120,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
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

                    yesButton.Click += (s, e) =>
                    {
                        tcs.SetResult(true);
                        messageBox.Close();
                    };

                    noButton.Click += (s, e) =>
                    {
                        tcs.SetResult(false);
                        messageBox.Close();
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
                                
                                // Force kill process and wait to ensure it's terminated
                                currentProcess.Kill(true); // Force kill the process and its children
                                currentProcess.WaitForExit(3000); // Wait up to 3 seconds for it to exit
                                
                                if (outputTextBox != null)
                                {
                                    outputTextBox.Text += "Process terminated.\n";
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
                            window.Close();
                        }
                    }
                }
                else
                {
                    // No process running or process already completed, just close
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                // Last resort: log the error and force close
                if (outputTextBox != null)
                {
                    outputTextBox.Text += $"Error during exit: {ex.Message}\n";
                }
                
                // Force close the window after a short delay
                await Task.Delay(500);
                window.Close();
            }
        }
    }
} 