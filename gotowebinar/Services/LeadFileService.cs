using gotowebinar.Models;
using Microsoft.Extensions.Configuration;

namespace gotowebinar.Services
{
    public interface ILeadFileService
    {
        Task<List<Lead>> FilterOldLeadsAsync(List<Lead> lstRegistrants);
        Task AppendProcessedLeadsKeysAsync(List<Lead> filteredRegistrants);
    }

    public class LeadFileService : ILeadFileService
    {
        private readonly IConfiguration _configuration;
        private string outputDir;
        private string registrantKeyFile;

        // Constructor reads necessary file path configurations
        public LeadFileService(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            outputDir = _configuration["FileConfig:OutputDir"] ?? throw new ArgumentNullException("FileConfig:OutputDir");
            registrantKeyFile = _configuration["FileConfig:UploadedKeyFile"] ?? throw new ArgumentNullException("FileConfig:UploadedKeyFile");
        }

        // Filters out leads that have already been processed by checking stored keys
        public async Task<List<Lead>> FilterOldLeadsAsync(List<Lead> listLeads)
        {
            var processedRegistrantKeys = await ReadProcessedRegistrantKeysAsync();

            // Return only leads whose ContactId is not yet recorded in processed keys
            var filteredRegistrants = listLeads
                .Where(regis => !processedRegistrantKeys.Contains(regis.ContactId.ToString()))
                .ToList();

            return filteredRegistrants;
        }

        // Appends the ContactIds of filtered leads to the processed keys file
        public async Task AppendProcessedLeadsKeysAsync(List<Lead> filteredListLeads)
        {
            if (filteredListLeads.Count > 0)
            {
                var remainingIds = filteredListLeads
                    .Select(contact => contact.ContactId)
                    .Where(id => id != null)
                    .Distinct()
                    .Select(id => id.ToString());

                // Build full file path for keys storage
                string filePath = Path.Combine(outputDir, registrantKeyFile);

                // Append the distinct ContactIds as new lines to the file
                await File.AppendAllLinesAsync(filePath, remainingIds);
            }
        }

        // Reads the set of already processed ContactIds from the keys file
        private async Task<HashSet<string>> ReadProcessedRegistrantKeysAsync()
        {
            string filePath = Path.Combine(outputDir, registrantKeyFile);

            // If the file doesn't exist, return an empty set
            if (!File.Exists(filePath))
            {
                return new HashSet<string>();
            }

            // Read all lines (ContactIds) and return as a HashSet for efficient lookup
            var lines = await File.ReadAllLinesAsync(filePath);
            return new HashSet<string>(lines);
        }
    }
}
