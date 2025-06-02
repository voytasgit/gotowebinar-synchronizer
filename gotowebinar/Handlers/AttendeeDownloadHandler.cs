using gotowebinar.Models.Attendee;
using gotowebinar.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace gotowebinar.Handlers
{
    /// <summary>
    /// Interface defining the contract for attendee download handling.
    /// </summary>
    public interface IAttendeeDownloadHandler
    {
        /// <summary>
        /// Executes the attendee download process asynchronously.
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    /// Handler class responsible for downloading and processing attendees of webinars.
    /// </summary>
    public class AttendeeDownloadHandler : IAttendeeDownloadHandler
    {
        private readonly IWebinarService _webinarServices;
        private readonly IAttendeeService _attendeeServices;
        private readonly IClientService _clientServices;
        private readonly IAttendeeDownloadService _attendeeDownloadServices;
        private readonly IConfiguration _configuration;

        // Configuration settings for date range filtering
        private int fromDateBackward;
        private int toDateForward;

        /// <summary>
        /// Constructor injecting required services and configuration.
        /// Reads date range configuration values or throws if missing.
        /// </summary>
        public AttendeeDownloadHandler(
            IWebinarService webinarServices,
            IAttendeeService attendeeService,
            IAttendeeDownloadService attendeeDownloadServices,
            IClientService clientServices,
            IConfiguration configuration)
        {
            _attendeeServices = attendeeService;
            _webinarServices = webinarServices;
            _attendeeDownloadServices = attendeeDownloadServices;
            _clientServices = clientServices;
            _configuration = configuration;

            // Parse backward and forward month offsets from config
            fromDateBackward = int.TryParse(_configuration["FileConfig:FromDateBackward"], out var resultB) ? resultB : throw new ArgumentNullException("FileConfig:FromDateBackward");
            toDateForward = int.TryParse(_configuration["FileConfig:ToDateForward"], out var resultF) ? resultF : throw new ArgumentNullException("FileConfig:ToDateForward");
        }

        /// <summary>
        /// Main execution method to download attendees for webinars within the configured date range.
        /// </summary>
        public async Task ExecuteAsync()
        {
            Log.Debug("Start AttendeeDownloadHandler...");

            // Authenticate and obtain access token
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");

            // Get current UTC time
            DateTime now = DateTime.UtcNow;

            // Calculate date range to filter webinars
            DateTime fromDate = now.AddMonths(fromDateBackward).Date; // Start of fromDate day
            DateTime toDate = now.AddMonths(toDateForward).Date.AddDays(1).AddSeconds(-1); // End of toDate day

            // Format dates as ISO 8601 strings in UTC
            string fromTime = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string toTime = toDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Retrieve all webinars within the date range
            var webinarResponse = await _webinarServices.GetAllWebinarsAsync(fromTime, toTime, page: 0, size: 200, accessToken: accessToken);

            if (webinarResponse != null && webinarResponse._embedded != null && webinarResponse._embedded?.webinars != null)
            {
                var lstWebinars = webinarResponse._embedded?.webinars.ToList();

                // Flag to optionally disable filtering on the first run
                bool filter = true; // Set false to skip filtering on first call

                // Filter webinars that ended before now or bypass filter if disabled
                var filteredWebinars = lstWebinars
                    .Where(webinar => webinar.times.Any(time =>
                        DateTime.TryParse(time.endTime, out var endTime) && endTime < DateTime.UtcNow || filter == false))
                    // Sort webinars by their earliest end time
                    .OrderBy(webinar => webinar.times
                        .Where(time => DateTime.TryParse(time.endTime, out var endTime))
                        .Min(time => DateTime.Parse(time.endTime)))
                    .ToList();

                // Process each filtered webinar
                foreach (var webinar in filteredWebinars)
                {
                    // Retrieve attendees for the webinar
                    var attendeeResponse = await _attendeeServices.GetAllAttendeesAsync(webinar.organizerKey, webinar.webinarKey, 0, 200, accessToken);

                    if (attendeeResponse != null && attendeeResponse.Embedded != null)
                    {
                        var listAttendeeParticipationResponses = attendeeResponse.Embedded.AttendeeParticipationResponses.ToList();

                        // Filter out attendees that were processed previously for this webinar
                        var filteredRegistrants = await _attendeeDownloadServices.FilterOldWebinarRegistrantAsync(listAttendeeParticipationResponses, webinar.webinarKey);

                        if (filteredRegistrants.Count > 0)
                        {
                            var listAttendeeData = new List<AttendeeData>();

                            // For each new attendee, fetch detailed data
                            foreach (var attendeeParticipationResponse in filteredRegistrants)
                            {
                                var attendeeData = await _attendeeServices.GetAttendeeDataAsync(
                                    webinar.organizerKey,
                                    webinar.webinarKey,
                                    attendeeParticipationResponse.RegistrantKey.ToString(),
                                    attendeeParticipationResponse.SessionKey.ToString(),
                                    accessToken);

                                // Add unique attendee data to list
                                if (attendeeData != null && !listAttendeeData.Contains(attendeeData))
                                    listAttendeeData.Add(attendeeData);
                            }

                            // Save the new attendee data and mark processed keys to avoid duplicates next time
                            await _attendeeDownloadServices.SaveRegistrantDataAsync(listAttendeeData, webinar.webinarKey);
                            await _attendeeDownloadServices.AppendProcessedRegistrantKeysAsync(filteredRegistrants, webinar.webinarKey);
                        }
                    }
                }
            }

            Log.Debug("Ende  AttendeeDownloadHandler.");
        }
    }
}
