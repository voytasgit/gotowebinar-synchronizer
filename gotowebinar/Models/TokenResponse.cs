using System.Text.Json.Serialization;

namespace gotowebinar.Models
{
    /// <summary>
    /// Legacy token response model that uses property setters for JSON mapping.
    /// Maps JSON fields (snake_case) to C# properties (PascalCase) via setters.
    /// </summary>
    public class TokenResponse_Old
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public string Scope { get; set; }
        public string Principal { get; set; }

        // JSON property mapping via setters
        public string access_token { set => AccessToken = value; }
        public string token_type { set => TokenType = value; }
        public string refresh_token { set => RefreshToken = value; }
        public int expires_in { set => ExpiresIn = value; }
        public string scope { set => Scope = value; }
        public string principal { set => Principal = value; }
    }

    /// <summary>
    /// Recommended token response model.
    /// Uses JsonPropertyName attribute to map JSON property names to C# properties.
    /// </summary>
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }

        [JsonPropertyName("principal")]
        public string Principal { get; set; }
    }

    /// <summary>
    /// Represents an internal API error response with code and message.
    /// </summary>
    public class ApiErrorResponse
    {
        public string IntErrCode { get; set; }
        public string Msg { get; set; }
    }

    /// <summary>
    /// Represents a standard OAuth or API error response with error and description.
    /// Maps JSON fields "error" and "error_description".
    /// </summary>
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; }

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; }
    }
}
