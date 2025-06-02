using gotowebinar.Models.Registrant.Registrant;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace gotowebinar.Services
{
    public interface IRegistrantFileService
    {
        Task<List<Registrant>> FilterOldWebinarRegistrantAsync(List<Registrant> lstRegistrants);
        Task AppendProcessedRegistrantKeysAsync(List<Registrant> filteredRegistrants);
        Task SaveRegistrantDataAsync(List<RegistrantData> ListRegistrantData, string WebinarKey);
    }

    public class RegistrantFileService : IRegistrantFileService
    {
        private readonly IConfiguration _configuration;
        private string DummyPhone = "";
        private string outputDir;
        private string registrantKeyFile;

        public RegistrantFileService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            DummyPhone = _configuration["FileConfig:DummyPhone"] ?? throw new ArgumentNullException("FileConfig:DummyPhone");
            outputDir = _configuration["FileConfig:OutputDir"] ?? throw new ArgumentNullException("FileConfig:OutputDir");
            registrantKeyFile = _configuration["FileConfig:RegistrantKeyFile"] ?? throw new ArgumentNullException("FileConfig:RegistrantKeyFile");
        }

        // Filters out registrants that have already been processed
        public async Task<List<Registrant>> FilterOldWebinarRegistrantAsync(List<Registrant> lstRegistrants)
        {
            var processedRegistrantKeys = await ReadProcessedRegistrantKeysAsync();
            var filteredRegistrants = lstRegistrants
                .Where(regis => !processedRegistrantKeys.Contains(regis.RegistrantKey.ToString()))
                .ToList();
            return filteredRegistrants;
        }

        // Appends the keys of filtered registrants to the processed keys file
        public async Task AppendProcessedRegistrantKeysAsync(List<Registrant> filteredRegistrants)
        {
            if (filteredRegistrants.Count > 0)
            {
                var remainingIds = filteredRegistrants
                    .Select(contact => contact.RegistrantKey.ToString())
                    .Where(id => id != null)
                    .Distinct();

                string filePath = GetOutputDirFilePath();
                await File.AppendAllLinesAsync(filePath, remainingIds);
            }
        }

        // Gets the full path to the registrant key file
        private string GetOutputDirFilePath() =>
            Path.Combine(GetOutputDir(), registrantKeyFile);

        // Reads the processed registrant keys from file into a HashSet
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

        // Ensures output directory exists and returns its path
        private string GetOutputDir()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string downloadPath = Path.Combine(exeDir, outputDir);
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            return downloadPath;
        }

        // Saves the list of registrant data as a formatted JSON file
        // Replaces dummy phone numbers with empty strings before saving
        public async Task SaveRegistrantDataAsync(List<RegistrantData> ListRegistrantData, string WebinarKey)
        {
            if (ListRegistrantData.Count() > 0) // Ensure list is not empty
            {
                ListRegistrantData
                    .Where(r => r.Phone == DummyPhone)
                    .ToList()
                    .ForEach(r => r.Phone = "");

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    // Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // enables direct output of special characters if needed
                };

                var json = JsonSerializer.Serialize(ListRegistrantData, options);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string filePath = Path.Combine(GetOutputDir(), $"registrant_{WebinarKey}_{timestamp}.json");

                await File.WriteAllTextAsync(filePath, json);
            }
        }
    }
}
