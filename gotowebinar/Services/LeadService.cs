using gotowebinar.Models;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace gotowebinar.Services
{
    public interface ILeadService
    {
        Task<List<Lead>> ReadLeadsFromFilesAsync();
    }

    public class LeadService : ILeadService
    {
        private string InputDir;
        private readonly IConfiguration _configuration;

        public LeadService(IConfiguration configuration)
        {
            _configuration = configuration;
            InputDir = _configuration["FileConfig:InputDir"] ?? throw new ArgumentNullException("FileConfig:InputDir");
        }

        // Reads all lead data from JSON files in the input directory
        public async Task<List<Lead>> ReadLeadsFromFilesAsync()
        {
            var leadsList = new List<Lead>();

            // Check if the input directory exists
            if (!Directory.Exists(InputDir))
            {
                // Return an empty list if directory does not exist
                return leadsList;
            }

            // Get all JSON files in the input directory
            var jsonFiles = Directory.GetFiles(InputDir, "*.json");

            foreach (var file in jsonFiles)
            {
                try
                {
                    // Read file content as string
                    var jsonContent = await File.ReadAllTextAsync(file);

                    // Deserialize JSON content into a list of Lead objects, case insensitive
                    var leads = JsonSerializer.Deserialize<List<Lead>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // If deserialization succeeded, add leads to the result list
                    if (leads != null)
                    {
                        leadsList.AddRange(leads);
                    }

                    // Prepare new file name by changing extension from .json to .txt
                    var newFileName = file.Replace(".json", ".txt");

                    // If the new file name already exists, create a unique name with timestamp
                    if (File.Exists(newFileName))
                    {
                        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        var directory = Path.GetDirectoryName(file);
                        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                        var extension = ".txt";
                        newFileName = Path.Combine(directory, $"{fileNameWithoutExtension}_{timestamp}{extension}");
                    }

                    // Rename the original JSON file to the new file name
                    File.Move(file, newFileName);
                }
                catch
                {
                    // Silently ignore any errors (e.g. invalid JSON or file issues)
                }
            }

            return leadsList;
        }
    }
}
