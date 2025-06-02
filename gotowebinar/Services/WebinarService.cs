using gotowebinar.Models.Webinar;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace gotowebinar.Services
{
    /// <summary>
    /// Interface for accessing webinar-related data from the external GoToWebinar API.
    /// </summary>
    public interface IWebinarService
    {
        Task<WebinarResponse?> GetAllWebinarsAsync(string fromTime, string toTime, int page = 0, int size = 20, string accessToken = "");
        Task<Webinar?> GetWebinarAsync(string organizerKey, string webinarKey, string accessToken = "");
    }

    /// <summary>
    /// Service implementation for retrieving webinar data via HTTP from GoToWebinar API.
    /// </summary>
    public class WebinarService : IWebinarService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _accountKey;
        private readonly string _baseApiUrl;

        public WebinarService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            // Load required API settings from configuration
            _accountKey = _configuration["ApiSettings:account_key_gw"]
                ?? throw new ArgumentNullException("ApiSettings:account_key_gw");

            _baseApiUrl = _configuration["ApiSettings:BaseApiUrl"]
                ?? throw new ArgumentNullException("ApiSettings:BaseApiUrl");
        }

        /// <summary>
        /// Gets detailed information for a single webinar using its keys.
        /// </summary>
        public async Task<Webinar?> GetWebinarAsync(string organizerKey, string webinarKey, string accessToken = "")
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            var requestUrl = $"{_baseApiUrl}/organizers/{organizerKey}/webinars/{webinarKey}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<Webinar>(responseContent, options);
        }

        /// <summary>
        /// Retrieves a paged list of all webinars within a specific time range.
        /// Handles paging internally and returns a combined response.
        /// </summary>
        public async Task<WebinarResponse?> GetAllWebinarsAsync(string fromTime, string toTime, int page = 0, int size = 200, string accessToken = "")
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            var allWebinars = new List<Webinar>();
            int totalPages = 1;

            while (page < totalPages)
            {
                var requestUrl = $"{_baseApiUrl}/accounts/{_accountKey}/webinars?" +
                                 $"fromTime={Uri.EscapeDataString(fromTime)}&" +
                                 $"toTime={Uri.EscapeDataString(toTime)}&" +
                                 $"page={page}&size={size}";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error retrieving webinars: {response.StatusCode} - {errorMessage}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var webinarResponse = JsonSerializer.Deserialize<WebinarResponse>(responseContent);

                if (webinarResponse?._embedded?.webinars == null)
                    break;

                allWebinars.AddRange(webinarResponse._embedded.webinars);
                totalPages = webinarResponse.page.totalPages;
                page++;
            }

            return new WebinarResponse
            {
                _embedded = new Embedded
                {
                    webinars = allWebinars.ToArray()
                },
                page = new Page
                {
                    size = allWebinars.Count,
                    totalElements = allWebinars.Count,
                    totalPages = totalPages,
                    number = page - 1
                }
            };
        }
    }
}
