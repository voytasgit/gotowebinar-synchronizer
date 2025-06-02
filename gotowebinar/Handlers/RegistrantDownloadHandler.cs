using gotowebinar.Models.Registrant.Registrant;
using gotowebinar.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace gotowebinar.Handlers
{
    /// <summary>
    /// Interface defining the contract for a registrant download handler.
    /// </summary>
    public interface IRegistrantDownloadHandler
    {
        /// <summary>
        /// Executes the registrant download process asynchronously.
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    /// Implementation of a handler that downloads registrant data for webinars.
    /// </summary>
    public class RegistrantDownloadHandler : IRegistrantDownloadHandler
    {
        private readonly IWebinarService _webinarServices;
        private readonly IClientService _clientServices;
        private readonly IRegistrantService _registrantServices;
        private readonly IRegistrantFileService _registrantFileServices;
        private readonly IConfiguration _configuration;

        // Number of months backward from current date to start fetching webinars
        private int fromDateBackward;

        // Number of months forward from current date to end fetching webinars
        private int toDateForward;

        /// <summary>
        /// Constructor with dependency injection and configuration reading.
        /// </summary>
        public RegistrantDownloadHandler(
            IWebinarService webinarServices,
            IClientService clientServices,
            IRegistrantService registrantService,
            IRegistrantFileService registrantFileService,
            IConfiguration configuration)
        {
            _webinarServices = webinarServices;
            _clientServices = clientServices;
            _registrantServices = registrantService;
            _registrantFileServices = registrantFileService;
            _configuration = configuration;

            // Read from configuration how far back to fetch webinars, or throw if missing
            fromDateBackward = int.TryParse(_configuration["FileConfig:FromDateBackward"], out var resultB)
                ? resultB
                : throw new ArgumentNullException("FileConfig:FromDateBackward");

            // Read from configuration how far forward to fetch webinars, or throw if missing
            toDateForward = int.TryParse(_configuration["FileConfig:ToDateForward"], out var resultF)
                ? resultF
                : throw new ArgumentNullException("FileConfig:FromDateBackward");
        }

        /// <summary>
        /// Main execution method to download registrants for webinars in the configured date range.
        /// </summary>
        public async Task ExecuteAsync()
        {
            Log.Debug("Start RegistrantDownloadHandler...");

            // Authenticate and get access token
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");

            // Current UTC date/time
            DateTime now = DateTime.UtcNow;

            // Calculate date range based on configured months backward and forward
            DateTime fromDate = now.AddMonths(fromDateBackward).Date; // Start date, beginning of day
            DateTime toDate = now.AddMonths(toDateForward).Date.AddDays(1).AddSeconds(-1); // End date, end of day

            // Format dates as ISO 8601 strings in UTC for API requests
            string fromTime = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string toTime = toDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Fetch all webinars within the date range
            var webinarResponse = await _webinarServices.GetAllWebinarsAsync(fromTime, toTime, page: 0, size: 200, accessToken: accessToken);

            // Check if webinar response and embedded webinar data exist
            if (webinarResponse != null && webinarResponse._embedded != null && webinarResponse._embedded?.webinars != null)
            {
                // Convert webinars to list for easier LINQ handling
                var lstWebinars = webinarResponse._embedded?.webinars.ToList();

                // Filter flag to control filtering behavior; false on first call can disable filtering
                bool filter = true; // false; false only for the first call

                // Filter webinars whose end time is in the future or if filtering is disabled, then sort by earliest end time
                var filteredWebinars = lstWebinars
                    .Where(webinar => webinar.times.Any(time =>
                        DateTime.TryParse(time.endTime, out var endTime) && endTime >= DateTime.UtcNow || filter == false))
                    .OrderBy(webinar => webinar.times
                        .Where(time => DateTime.TryParse(time.endTime, out var endTime))
                        .Min(time => DateTime.Parse(time.endTime)))
                    .ToList();

                // Loop through each filtered webinar
                foreach (var webinar in filteredWebinars)
                {
                    // Get registrants for the webinar (first page, 200 per page)
                    var lstRegistrants = await _registrantServices.GetRegistrantsAsync(webinar.organizerKey, webinar.webinarKey, 0, 200, accessToken);

                    // Check if registrants data is available
                    if (lstRegistrants != null && lstRegistrants.Data != null)
                    {
                        // Filter out old registrants that have already been processed or are irrelevant
                        var filteredRegistrants = await _registrantFileServices.FilterOldWebinarRegistrantAsync(lstRegistrants.Data);

                        // If any new registrants to process
                        if (filteredRegistrants.Count > 0)
                        {
                            List<RegistrantData> listRegistrantData = new List<RegistrantData>();

                            // Loop over filtered registrants to fetch detailed registrant data
                            foreach (var registrant in filteredRegistrants)
                            {
                                var registrantData = await _registrantServices.GetRegistrantDataAsync(webinar.organizerKey, webinar.webinarKey, registrant.RegistrantKey.ToString(), accessToken);

                                // Add registrant data if not already in list
                                if (registrantData != null && !listRegistrantData.Contains(registrantData))
                                    listRegistrantData.Add(registrantData);
                            }

                            // Save detailed registrant data to file storage
                            await _registrantFileServices.SaveRegistrantDataAsync(listRegistrantData, webinar.webinarKey);

                            // Append processed registrant keys to avoid reprocessing
                            await _registrantFileServices.AppendProcessedRegistrantKeysAsync(filteredRegistrants);
                        }
                    }
                }
            }

            Log.Debug($"Ende  RegistrantDownloadHandler.");
        }
    }
}
