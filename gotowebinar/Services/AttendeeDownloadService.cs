using gotowebinar.Models.Attendee;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace gotowebinar.Services
{
    public interface IAttendeeDownloadService
    {
        /// <summary>
        /// Filters out registrants that were already processed based on a stored list of keys.
        /// </summary>
        Task<List<AttendeeParticipationResponse>> FilterOldWebinarRegistrantAsync(List<AttendeeParticipationResponse> lstRegistrants, string WebinarKey);

        /// <summary>
        /// Appends newly processed registrant keys to a persistent storage file.
        /// </summary>
        Task AppendProcessedRegistrantKeysAsync(List<AttendeeParticipationResponse> filteredRegistrants, string WebinarKey);

        /// <summary>
        /// Saves detailed attendee data to a JSON file.
        /// </summary>
        Task SaveRegistrantDataAsync(List<AttendeeData> ListAttendeeData, string WebinarKey);
    }

    public class AttendeeDownloadService : IAttendeeDownloadService
    {
        private readonly IConfiguration _configuration;
        private readonly string outputDir;
        private readonly string registrantKeyFile;

        public AttendeeDownloadService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Read output directory and registrant key filename from config
            outputDir = _configuration["FileConfig:OutputDir"] ?? throw new ArgumentNullException("FileConfig:OutputDir");
            registrantKeyFile = _configuration["FileConfig:AttendeeKeyFile"] ?? throw new ArgumentNullException("FileConfig:AttendeeKeyFile");
        }

        /// <summary>
        /// Filters out registrants whose keys are already stored in the processed keys file.
        /// </summary>
        public async Task<List<AttendeeParticipationResponse>> FilterOldWebinarRegistrantAsync(List<AttendeeParticipationResponse> lstRegistrants, string WebinarKey)
        {
            var processedRegistrantKeys = await ReadProcessedRegistrantKeysAsync();

            // Filter out registrants whose keys are found in the processed keys set
            var filteredRegistrants = lstRegistrants
                .Where(regis => !processedRegistrantKeys.Contains(WebinarKey + regis.RegistrantKey.ToString()))
                .ToList();

            return filteredRegistrants;
        }

        /// <summary>
        /// Appends the keys of newly processed registrants to the persistent file.
        /// </summary>
        public async Task AppendProcessedRegistrantKeysAsync(List<AttendeeParticipationResponse> filteredRegistrants, string WebinarKey)
        {
            if (filteredRegistrants.Count > 0)
            {
                // Build the combined keys (WebinarKey + RegistrantKey) and ensure distinct entries
                var remainingIds = filteredRegistrants
                    .Select(contact => WebinarKey + contact.RegistrantKey.ToString())
                    .Where(id => id != null)
                    .Distinct();

                string filePath = GetOutputDirFilePath();

                // Append new keys to the file asynchronously
                await File.AppendAllLinesAsync(filePath, remainingIds);
            }
        }

        /// <summary>
        /// Reads the set of previously processed registrant keys from the file.
        /// Returns an empty set if the file does not exist.
        /// </summary>
        private async Task<HashSet<string>> ReadProcessedRegistrantKeysAsync()
        {
            string filePath = GetOutputDirFilePath();

            if (!File.Exists(filePath))
            {
                return new HashSet<string>();
            }

            var lines = await File.ReadAllLinesAsync(filePath);

            return new HashSet<string>(lines);
        }

        /// <summary>
        /// Gets the full path to the file where processed registrant keys are stored.
        /// </summary>
        private string GetOutputDirFilePath() =>
            Path.Combine(GetOutputDir(), registrantKeyFile);

        /// <summary>
        /// Gets or creates the output directory where files will be saved.
        /// </summary>
        private string GetOutputDir()
        {
            // Base directory where the application is running
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;

            // Combine base dir and configured output directory name
            string downloadPath = Path.Combine(exeDir, outputDir);

            // Create the directory if it doesn't exist
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }

            return downloadPath;
        }

        /// <summary>
        /// Serializes and saves the detailed attendee data as a JSON file in the output directory.
        /// The filename includes the webinar key and timestamp to avoid overwriting.
        /// </summary>
        public async Task SaveRegistrantDataAsync(List<AttendeeData> ListAttendeeData, string WebinarKey)
        {
            if (ListAttendeeData.Count > 0)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // Uncomment below if you need to allow special characters directly:
                    // Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(ListAttendeeData, options);

                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");

                string filePath = Path.Combine(GetOutputDir(), $"attendee_{WebinarKey}_{timestamp}.json");

                await File.WriteAllTextAsync(filePath, json);
            }
        }
    }
}
