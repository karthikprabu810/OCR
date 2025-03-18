using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ocrGui
{
    /// <summary>
    /// Main application class for the OCR GUI.
    /// Handles the application lifecycle, XAML loading, and window initialization.
    /// </summary>
    public class App : Application
    {
        /// <summary>
        /// Initializes the application by loading XAML resources.
        /// This method is called during the early stages of application startup
        /// and is responsible for setting up the application's visual appearance.
        /// </summary>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Called when the Avalonia framework initialization has completed.
        /// This method creates and configures the main application window.
        /// </summary>
        /// <remarks>
        /// This is where we create the main window instance and assign it to the
        /// desktop application lifetime. This ensures proper window management and lifecycle.
        /// </remarks>
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Create the main window for the application
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
} 