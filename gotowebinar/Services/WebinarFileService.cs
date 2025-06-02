using gotowebinar.Models.Webinar;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace gotowebinar.Services
{
    /// <summary>
    /// Interface for saving webinar responses to local files.
    /// </summary>
    public interface IWebinarFileService
    {
        Task SaveWebinarResponseAsync(WebinarResponse webinarResponse);
    }

    /// <summary>
    /// Service that serializes and writes webinar data to a JSON file on disk.
    /// </summary>
    public class WebinarFileService : IWebinarFileService
    {
        private readonly IConfiguration _configuration;
        private readonly string _outputDir;

        public WebinarFileService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _outputDir = _configuration["FileConfig:OutputDir"]
                ?? throw new ArgumentNullException("FileConfig:OutputDir");
        }

        /// <summary>
        /// Resolves the full output path and ensures the directory exists.
        /// </summary>
        private string GetOutputDirectory()
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(exeDir, _outputDir);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        /// <summary>
        /// Saves the webinar response as a formatted JSON file.
        /// </summary>
        public async Task SaveWebinarResponseAsync(WebinarResponse webinarResponse)
        {
            if (webinarResponse == null)
                throw new ArgumentNullException(nameof(webinarResponse));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
                // Optional: .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(webinarResponse, options);
            var filePath = Path.Combine(GetOutputDirectory(), "webinarResponse.txt");

            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
