using gotowebinar.Handlers;
using Serilog;

namespace gotowebinar
{
    /// <summary>
    /// Main application entry point that orchestrates the processing workflow.
    /// </summary>
    public class App
    {
        private readonly ILeadUploadHandler _leadUploadHandler;
        private readonly ILeadHandler _leadHandler;
        private readonly IRegistrantDownloadHandler _registrantDownloadHandler;
        private readonly IAttendeeDownloadHandler _attendeeDownloadHandler;
        private readonly IWebinarHandler _webinarHandler;

        /// <summary>
        /// Constructor injecting all necessary handlers.
        /// </summary>
        public App(
            ILeadUploadHandler leadUploadHandler,
            ILeadHandler leadHandler,
            IRegistrantDownloadHandler registrantDownloadHandler,
            IAttendeeDownloadHandler attendeeDownloadHandler,
            IWebinarHandler webinarHandler)
        {
            _leadUploadHandler = leadUploadHandler;
            _leadHandler = leadHandler;
            _registrantDownloadHandler = registrantDownloadHandler;
            _attendeeDownloadHandler = attendeeDownloadHandler;
            _webinarHandler = webinarHandler;
        }

        /// <summary>
        /// Executes the full processing pipeline: webinars, leads, registrants, attendees.
        /// </summary>
        public async Task RunAsync()
        {
            try
            {
                Log.Debug("Starting GoToWebinar synchronization process...");

                // Step 0 - Load all webinar data
                Log.Debug("Executing webinar data handler...");
                await _webinarHandler.ExecuteAsync();

                // Step 1 - Read all leads from external download folder
                Log.Debug("Executing lead handler...");
                var listLeads = await _leadHandler.ExecuteAsync();

                // Step 2 - Refresh token and upload leads to corresponding webinars
                Log.Debug("Executing lead upload handler...");
                await _leadUploadHandler.ExecuteAsync(listLeads);

                // Step 3 - Download all registrants
                Log.Debug("Executing registrant download handler...");
                await _registrantDownloadHandler.ExecuteAsync();

                // Step 4 - Download all attendees
                Log.Debug("Executing attendee download handler...");
                await _attendeeDownloadHandler.ExecuteAsync();

                Log.Debug("GoToWebinar synchronization process completed successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during the GoToWebinar synchronization process.");
            }
        }
    }
}
