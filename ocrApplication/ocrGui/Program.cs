using Avalonia;

namespace ocrGui
{
    /// <summary>
    /// Main entry point for the OCR GUI application.
    /// Handles the initialization and startup of the Avalonia UI framework.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point for the application. Initializes the Avalonia framework and starts the application.
        /// Important: Don't use any Avalonia, third-party APIs or any SynchronizationContext-reliant code
        /// before AppMain is called, as doing so may lead to unexpected behavior.
        /// </summary>
        /// <param name="args">Command-line arguments passed to the application</param>
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        /// <summary>
        /// Configures the Avalonia application builder with necessary settings.
        /// Sets up the core rendering system, platform detection, and logging configuration.
        /// </summary>
        /// <returns>Configured AppBuilder instance ready to start the application</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()  // Automatically detect and use appropriate platform backend
                .LogToTrace();        // Configure logging to use System.Diagnostics.Trace
    }
}