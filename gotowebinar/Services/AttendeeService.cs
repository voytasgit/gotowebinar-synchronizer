using gotowebinar.Models.Attendee;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace gotowebinar.Services
{
    public interface IAttendeeService
    {
        // Gets all attendees for a given webinar, supports paging
        Task<AttendeeResponse?> GetAllAttendeesAsync(string organizerKey, string webinarKey, int page = 0, int size = 100, string accessToken = "");

        // Gets detailed data about a specific attendee in a specific session
        Task<AttendeeData?> GetAttendeeDataAsync(string organizerKey, string webinarKey, string registrantKey, string sessionKey, string accessToken = "");
    }

    public class AttendeeService : IAttendeeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Constructor injects HttpClient and IConfiguration and validates arguments
        public AttendeeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Retrieves detailed data about a specific attendee in a given session.
        /// Requires a valid OAuth access token.
        /// </summary>
        public async Task<AttendeeData?> GetAttendeeDataAsync(string organizerKey, string webinarKey, string registrantKey, string sessionKey, string accessToken = "")
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            // Read base API URL from configuration
            var baseApiUrl = _configuration["ApiSettings:BaseApiUrl"]
                             ?? throw new ArgumentNullException("ApiSettings:BaseApiUrl");

            // Build the request URL for the specific attendee in a session
            var requestUrl = $"{baseApiUrl}/organizers/{organizerKey}/webinars/{webinarKey}/sessions/{sessionKey}/attendees/{registrantKey}";

            // Create HTTP GET request with authorization header
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            // Send the request and ensure a successful response
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Read response JSON content
            var responseContent = await response.Content.ReadAsStringAsync();

            // Configure JSON serializer options to be case-insensitive on property names
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Deserialize the JSON into AttendeeData object and return
            var ret = JsonSerializer.Deserialize<AttendeeData>(responseContent, options);
            return ret;
        }

        /// <summary>
        /// Retrieves all attendees for a given webinar with pagination support.
        /// Throws if access token is missing or page size > 200.
        /// Aggregates attendees across all pages and returns a combined response.
        /// </summary>
        public async Task<AttendeeResponse?> GetAllAttendeesAsync(string organizerKey, string webinarKey, int page = 0, int size = 200, string accessToken = "")
        {
            var allDataSets = new List<AttendeeParticipationResponse>();
            int totalPages = 1; // Initial total pages count

            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            if (size > 200)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be <= 200");

            var BaseApiUrl = _configuration["ApiSettings:BaseApiUrl"]
                          ?? throw new ArgumentNullException("ApiSettings:BaseApiUrl");

            // Loop through all pages until all attendees are retrieved
            while (page < totalPages)
            {
                // Build URL for attendees endpoint with paging parameters
                var requestUrl = $"{BaseApiUrl}/organizers/{organizerKey}/webinars/{webinarKey}/attendees?page={page}&size={size}";

                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var attendeeResponse = JsonSerializer.Deserialize<AttendeeResponse>(responseContent, options);

                if (attendeeResponse == null || attendeeResponse.Embedded?.AttendeeParticipationResponses == null)
                {
                    // Break if no attendees found or response is invalid
                    break;
                }

                // Add the retrieved attendees to the aggregated list
                allDataSets.AddRange(attendeeResponse.Embedded.AttendeeParticipationResponses);

                // Update totalPages to the actual number of pages
                totalPages = attendeeResponse.Page.TotalPages;

                // Move to the next page
                page++;
            }

            // Create a combined response containing all attendees and paging info
            var finalResponse = new AttendeeResponse
            {
                Embedded = new Models.Attendee.Embedded
                {
                    AttendeeParticipationResponses = allDataSets.ToArray()  // Return all as array
                },
                Page = new Models.Attendee.Page
                {
                    Size = allDataSets.Count,
                    TotalElements = allDataSets.Count,
                    TotalPages = totalPages,
                    Number = page - 1 // Current page index after loop ends
                }
            };

            return finalResponse;
        }
    }
}
