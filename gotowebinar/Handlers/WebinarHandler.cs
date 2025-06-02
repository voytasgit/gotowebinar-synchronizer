using gotowebinar.Services;
using Serilog;

namespace gotowebinar.Handlers
{
    /// <summary>
    /// Interface defining the contract for a webinar handler.
    /// </summary>
    public interface IWebinarHandler
    {
        /// <summary>
        /// Executes the handler logic asynchronously.
        /// </summary>
        Task ExecuteAsync();
    }

    /// <summary>
    /// Implementation of the webinar handler that coordinates fetching and saving webinar data.
    /// </summary>
    public class WebinarHandler : IWebinarHandler
    {
        private readonly IWebinarService _webinarService;
        private readonly IWebinarFileService _webinarFileServices;
        private readonly IClientService _clientServices;

        /// <summary>
        /// Constructor injecting required services.
        /// </summary>
        /// <param name="webinarService">Service to interact with webinar data API</param>
        /// <param name="webinarFileServices">Service to save webinar data to files</param>
        /// <param name="clientServices">Service to manage client authentication/token</param>
        public WebinarHandler(IWebinarService webinarService, IWebinarFileService webinarFileServices, IClientService clientServices)
        {
            _webinarService = webinarService;
            _clientServices = clientServices;
            _webinarFileServices = webinarFileServices;
        }

        /// <summary>
        /// Executes the process of fetching webinars within a date range and saving the response.
        /// </summary>
        public async Task ExecuteAsync()
        {
            Log.Debug("Start WebinarHandler...");

            // Get access token needed for API calls
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");

            // Current UTC date and time
            DateTime now = DateTime.UtcNow;

            // Define date range: 120 months (10 years) back to 3 months forward
            DateTime fromDate = now.AddMonths(-120).Date; // Start date (10 years ago)
            DateTime toDate = now.AddMonths(3).Date.AddDays(1).AddSeconds(-1); // End date (3 months ahead, end of day)

            // Format dates as ISO 8601 strings in UTC (e.g. "2025-06-02T00:00:00Z")
            string fromTime = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string toTime = toDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

            // Request all webinars within the date range from the webinar service
            var webinarResponse = await _webinarService.GetAllWebinarsAsync(fromTime, toTime, page: 0, size: 200, accessToken: accessToken);

            if (webinarResponse != null)
            {
                // Save the received webinar response to file storage
                await _webinarFileServices.SaveWebinarResponseAsync(webinarResponse);
                Log.Debug($"Ende WebinarHandler.");
            }
            else
            {
                // Log if no webinars were found in the given date range
                Log.Debug($"No Webinars in WebinarHandler.");
            }
        }
    }
}
