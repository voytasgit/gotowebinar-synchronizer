using gotowebinar.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;
using System.Text.Json;

namespace gotowebinar.Services
{
    public interface IClientService
    {
        Task<string> RefreshAccessTokenAsync();
        Task<string> GetAccessTokenAsync(string token_manuell);
    }

    public class ClientService : IClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        // Configuration values read from appsettings or environment variables
        string TokenEndpoint;
        string redirectUri;
        string clientId_gw;
        string clientSecret_gw;
        string refreshToken_gw;

        // Constructor injects HttpClient and IConfiguration, and initializes config values
        public ClientService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            TokenEndpoint = _configuration["ApiSettings:TokenEndpoint"] ?? throw new ArgumentNullException("ApiSettings:TokenEndpoint");

            redirectUri = _configuration["ApiSettings:redirectUri"] ?? throw new ArgumentNullException("ApiSettings:redirectUri");
            
            clientId_gw = _configuration["ApiSettings:clientId_gw"] ?? throw new ArgumentNullException("ApiSettings:clientId_gw");
            clientSecret_gw = _configuration["ApiSettings:clientSecret_gw"] ?? throw new ArgumentNullException("ApiSettings:clientSecret_gw");

            // Read refresh token from environment variables (user scope)
            refreshToken_gw = Environment.GetEnvironmentVariable("ApiSettings__refreshToken_gw", EnvironmentVariableTarget.User)
                              ?? throw new ArgumentNullException("Null ApiSettings__refreshToken_gw");

        }

        /// <summary>
        /// Refreshes the OAuth access token using the stored refresh token.
        /// </summary>
        public async Task<string> RefreshAccessTokenAsync()
        {
            // Create Basic Auth header value by encoding clientId and clientSecret
            string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId_gw}:{clientSecret_gw}"));

            // Prepare POST content for token refresh request
            var postData = new StringContent(
                $"grant_type=refresh_token&refresh_token={refreshToken_gw}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            // Clear existing headers and add new ones
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {basicAuth}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Send POST request to token endpoint
            var response = await _httpClient.PostAsync(TokenEndpoint, postData);

            // If response is not successful, try to read error details and log them
            if (!response.IsSuccessStatusCode)
            {
                string errorJson = await response.Content.ReadAsStringAsync();

                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(errorJson);
                    Log.Error($"Error: {response.StatusCode}, {errorResponse?.Error}, {errorResponse?.ErrorDescription}");
                    Log.Information($"Error: {response.StatusCode}, {errorResponse?.Error}, {errorResponse?.ErrorDescription}");
                }
                catch (JsonException ex)
                {
                    Log.Error($"Error while parsing error response: {ex.Message}");
                }

                return null;
            }

            // On success, deserialize token response JSON
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse);

            // If refresh token is returned, update environment variable to keep it current
            if (tokenResponse?.RefreshToken != null)
            {
                try
                {
                    Environment.SetEnvironmentVariable("ApiSettings__refreshToken_gw", tokenResponse.RefreshToken, EnvironmentVariableTarget.User);
                }
                catch (Exception ex)
                {
                    string s = ex.Message; // Handle or log this if needed
                }
                Log.Error("refreshToken_gw updated");
                Log.Information("refreshToken_gw updated");
            }

            // Return the new access token
            return tokenResponse?.AccessToken;
        }

        /// <summary>
        /// Obtains an access token by exchanging a manually provided authorization code.
        /// </summary>
        public async Task<string> GetAccessTokenAsync(string authorizationCode_manuell)
        {
            // Create Basic Auth header value
            string basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId_gw}:{clientSecret_gw}"));

            // Prepare POST content with authorization code grant parameters
            var requestContent = new StringContent(
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&grant_type=authorization_code&code={authorizationCode_manuell}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded");

            // Set HTTP headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {basicAuth}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Send POST request to token endpoint
            var response = await _httpClient.PostAsync(TokenEndpoint, requestContent);

            // If request failed, log error and return null
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
                return null;
            }

            // Deserialize response content to extract access token
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            return tokenResponse?.AccessToken;
        }
    }
}
