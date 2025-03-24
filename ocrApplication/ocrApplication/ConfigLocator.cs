namespace ocrApplication
{
    /// <summary>
    /// Utility class for locating the config.json file across different environments.
    /// </summary>
    public static class ConfigLocator
    {
        /// <summary>
        /// Finds the config.json file by searching in multiple locations.
        /// The search order is:
        /// 1. Base directory (parent of ocrApplication and ocrGui)
        /// 2. Current application directory
        /// 3. Project directory structure
        /// </summary>
        /// <param name="throwIfNotFound">If true, throws FileNotFoundException when config file is not found</param>
        /// <returns>Path to the config.json file if found; otherwise null (if throwIfNotFound is false)</returns>
        /// <exception cref="FileNotFoundException">Thrown if config file is not found and throwIfNotFound is true</exception>
        public static string? FindConfigFile(bool throwIfNotFound = true)
        {
            // First try to find the config.json in the base directory (parent of ocrApplication, ocrGui, and unitTestProject)
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string? configFilePath;
            
            // Navigate up until we find the directory containing ocrApplication and ocrGui
            while (!string.IsNullOrEmpty(baseDirectory))
            {
                var parentDir = Directory.GetParent(baseDirectory)?.FullName;
                if (parentDir == null) break;
                
                if (Directory.Exists(Path.Combine(parentDir, "ocrApplication")) && 
                    Directory.Exists(Path.Combine(parentDir, "ocrGui")))
                {
                    // Found the base directory
                    configFilePath = Path.Combine(parentDir, "config.json");
                    if (File.Exists(configFilePath))
                    {
                        return configFilePath;
                    }
                    break;
                }
                baseDirectory = parentDir;
            }
            
            // If not found in base directory, check the current application directory
            configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configFilePath))
            {
                return configFilePath;
            }
            
            // Try to locate the config file in the project directory structure
            var projectDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            while (projectDir != null)
            {
                // Check if we're in either ocrApplication or unitTestProject directory
                string dirName = Path.GetFileName(projectDir);
                if (dirName.Equals("ocrApplication", StringComparison.OrdinalIgnoreCase) || 
                    dirName.Equals("unitTestProject", StringComparison.OrdinalIgnoreCase))
                {
                    configFilePath = Path.Combine(projectDir, "config.json");
                    if (File.Exists(configFilePath))
                    {
                        return configFilePath;
                    }
                }
                projectDir = Path.GetDirectoryName(projectDir);
            }
            
            if (throwIfNotFound)
            {
                throw new FileNotFoundException("Could not find config.json file. Please ensure it exists in the base directory or provide a valid path.");
            }
            
            return null;
        }
    }
} 