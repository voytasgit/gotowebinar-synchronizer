using gotowebinar.Models;
using gotowebinar.Models.Registrant.Registrant;
using gotowebinar.Models.Webinar;
using gotowebinar.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace gotowebinar.Handlers
{
    /// <summary>
    /// Interface defining the contract for uploading leads asynchronously.
    /// </summary>
    public interface ILeadUploadHandler
    {
        /// <summary>
        /// Executes the upload process for a list of leads.
        /// </summary>
        /// <param name="ListLead">List of leads to upload</param>
        Task ExecuteAsync(List<Lead> ListLead);
    }

    /// <summary>
    /// Handler class responsible for uploading leads as registrants to webinars.
    /// </summary>
    public class LeadUploadHandler : ILeadUploadHandler
    {
        private readonly IWebinarService _webinarServices;
        private readonly IClientService _clientServices;
        private readonly IRegistrantService _registrantServices;
        private readonly IConfiguration _configuration;
        private string dummyPhone = "";
        private readonly ILeadFileService _leadFileService;

        /// <summary>
        /// Constructor with dependencies injected and configuration loaded.
        /// </summary>
        public LeadUploadHandler(
            IWebinarService webinarServices,
            IClientService clientServices,
            IRegistrantService registrantService,
            IConfiguration configuration,
            ILeadFileService leadFileService)
        {
            _webinarServices = webinarServices;
            _clientServices = clientServices;
            _registrantServices = registrantService;
            _configuration = configuration;

            // Read dummy phone number from configuration; throw if missing
            dummyPhone = _configuration["FileConfig:DummyPhone"] ?? throw new ArgumentNullException("FileConfig:DummyPhone");

            _leadFileService = leadFileService;
        }

        /// <summary>
        /// Executes the lead upload process:
        /// - Authenticates
        /// - Fetches webinars in date range
        /// - Filters old leads
        /// - Uploads new leads as registrants if they don't already exist
        /// - Marks processed leads
        /// </summary>
        public async Task ExecuteAsync(List<Lead> ListLead)
        {
            Log.Debug("Start registrantHandler...");

            // Proceed only if there are leads to process
            if (ListLead.Count > 0)
            {
                // Authenticate and get access token
                var accessToken = await _clientServices.RefreshAccessTokenAsync();
                if (accessToken == null)
                    throw new Exception("Cant't get accessToken");

                // Get current UTC date/time
                DateTime now = DateTime.UtcNow;

                // Define date range: from 3 months ago to 3 months ahead
                DateTime fromDate = now.AddMonths(-3).Date; // Start date, beginning of day
                DateTime toDate = now.AddMonths(3).Date.AddDays(1).AddSeconds(-1); // End date, end of day

                // Format dates as ISO 8601 strings for API request
                string fromTime = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
                string toTime = toDate.ToString("yyyy-MM-ddTHH:mm:ssZ");

                // Retrieve webinars within the date range
                var webinarResponse = await _webinarServices.GetAllWebinarsAsync(fromTime, toTime, page: 0, size: 20, accessToken: accessToken);

                // Check if webinars are available in the response
                if (webinarResponse != null && webinarResponse._embedded != null)
                {
                    // Filter leads to exclude ones already processed
                    var filteredListLead = await _leadFileService.FilterOldLeadsAsync(ListLead);

                    // Proceed only if there are filtered leads to process
                    if (filteredListLead != null && filteredListLead.Count() > 0)
                    {
                        // Loop through each lead in filtered list
                        foreach (var lead in filteredListLead)
                        {
                            // Extract the webinar key from the lead's destination
                            string targetWebinarKey = lead.Destination;

                            // Find the matching webinar in the retrieved webinars
                            Webinar webinar = webinarResponse._embedded?.webinars.FirstOrDefault(w => w.webinarKey == targetWebinarKey);

                            // Proceed only if the webinar exists
                            if (webinar != null)
                            {
                                // Prepare a new registrant object from lead info
                                ExternerRegistrant registrant = new ExternerRegistrant
                                {
                                    FirstName = lead.FirstName,
                                    LastName = lead.LastName,
                                    Email = lead.Email,
                                    Phone = dummyPhone, // Use configured dummy phone number
                                    Source = lead.Quelle + "_" + lead.Uquelle
                                };

                                // Get existing registrants for the webinar
                                var registrants = await _registrantServices.GetRegistrantsAsync(webinar.organizerKey, webinar.webinarKey, 0, 100, accessToken);

                                // Check if the registrant with the same email already exists
                                bool registrantExists = registrants != null
                                    && registrants.Data != null
                                    && registrants.Data.Any(x => x.Email == registrant.Email);

                                // If registrant does not exist, create a new one
                                if (!registrantExists)
                                {
                                    var registrantResponse = await _registrantServices.CreateRegistrantAsync(webinar, registrant, accessToken);
                                    // Optionally handle registrantResponse (e.g. logging)
                                }
                            }
                        }

                        // Append the keys of processed leads to avoid duplicate processing
                        await _leadFileService.AppendProcessedLeadsKeysAsync(filteredListLead);
                    }
                }
            }

            Log.Debug($"Ende {ListLead.Count} registrantHandler.");
        }
    }
}
