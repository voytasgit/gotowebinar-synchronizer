using Microsoft.Extensions.Configuration;
using Serilog;

namespace gotowebinar.Utils
{
    /// <summary>
    /// Loads application configuration and initializes logging.
    /// </summary>
    public class ConfigurationLoader
    {
        /// <summary>
        /// Loads configuration from JSON files, user secrets, and environment variables.
        /// Also initializes Serilog for structured logging.
        /// </summary>
        /// <returns>The fully built configuration root.</returns>
        public IConfigurationRoot LoadConfiguration()
        {
            // Get machine/environment name to load environment-specific config
            var environmentName = Environment.MachineName;

            // Build configuration from multiple sources
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false, reloadOnChange: true)
                .AddUserSecrets<Program>() // Loads user secrets (development use)
                .AddEnvironmentVariables() // Loads environment variables (deployment settings)
                .Build();

            // Setup Serilog logging configuration
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log_.txt");

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder) // Use settings from configuration (e.g. minimum level)
                .WriteTo.File(
                    logPath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            return builder;
        }
    }
}
