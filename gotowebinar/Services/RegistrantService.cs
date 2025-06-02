using gotowebinar.Models.Registrant.Registrant;
using gotowebinar.Models.Webinar;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace gotowebinar.Services
{
    /// <summary>
    /// Interface for managing webinar registrants via the GoToWebinar API.
    /// </summary>
    public interface IRegistrantService
    {
        Task<RegistrantResponse> CreateRegistrantAsync(Webinar webinar, ExternerRegistrant registrant, string accessToken, bool resendConfirmation = false, object? additionalDetails = null);
        Task<RegistrantPaginatedResponse?> GetRegistrantsAsync(string organizerKey, string webinarKey, int page = 0, int limit = 100, string accessToken = "");
        Task<RegistrantData?> GetRegistrantDataAsync(string organizerKey, string webinarKey, string registrantKey, string accessToken = "");
    }

    /// <summary>
    /// Implementation of IRegistrantService for interacting with GoToWebinar's registrant endpoints.
    /// </summary>
    public class RegistrantService : IRegistrantService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseApiUrl;

        public RegistrantService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _baseApiUrl = _configuration["ApiSettings:BaseApiUrl"]
                ?? throw new ArgumentNullException("ApiSettings:BaseApiUrl");
        }

        /// <summary>
        /// Creates a new registrant for the given webinar.
        /// </summary>
        public async Task<RegistrantResponse> CreateRegistrantAsync(Webinar webinar, ExternerRegistrant registrant, string accessToken, bool resendConfirmation = false, object? additionalDetails = null)
        {
            if (string.IsNullOrWhiteSpace(webinar.organizerKey))
                throw new ArgumentException("Organizer key is required.", nameof(webinar.organizerKey));

            if (string.IsNullOrWhiteSpace(webinar.webinarKey))
                throw new ArgumentException("Webinar key is required.", nameof(webinar.webinarKey));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            if (string.IsNullOrWhiteSpace(registrant.FirstName) || string.IsNullOrWhiteSpace(registrant.LastName) || string.IsNullOrWhiteSpace(registrant.Email))
                throw new ArgumentException("First name, last name, and email are required fields.");

            var requestUrl = $"{_baseApiUrl}/organizers/{webinar.organizerKey}/webinars/{webinar.webinarKey}/registrants?resendConfirmation={resendConfirmation.ToString().ToLower()}";

            var requestBody = new
            {
                firstName = registrant.FirstName,
                lastName = registrant.LastName,
                email = registrant.Email,
                phone = registrant.Phone,
                source = registrant.Source,
                additionalDetails
            };

            var jsonBody = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error creating registrant: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RegistrantResponse>(responseContent);

            return result ?? throw new Exception("Failed to deserialize registrant response.");
        }

        /// <summary>
        /// Retrieves all registrants for a specific webinar (paginated).
        /// </summary>
        public async Task<RegistrantPaginatedResponse?> GetRegistrantsAsync(string organizerKey, string webinarKey, int page = 0, int limit = 100, string accessToken = "")
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            if (limit > 200)
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be <= 200");

            var requestUrl = $"{_baseApiUrl}/organizers/{organizerKey}/webinars/{webinarKey}/registrants?page={page}&limit={limit}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<RegistrantPaginatedResponse>(responseContent, options);
        }

        /// <summary>
        /// Gets detailed information for a specific registrant.
        /// </summary>
        public async Task<RegistrantData?> GetRegistrantDataAsync(string organizerKey, string webinarKey, string registrantKey, string accessToken = "")
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            var requestUrl = $"{_baseApiUrl}/organizers/{organizerKey}/webinars/{webinarKey}/registrants/{registrantKey}";

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            return JsonSerializer.Deserialize<RegistrantData>(responseContent, options);
        }
    }
}
