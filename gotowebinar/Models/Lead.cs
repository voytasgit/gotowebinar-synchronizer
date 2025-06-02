using System.Text.Json.Serialization;

namespace gotowebinar.Models
{
    /// <summary>
    /// Represents a Lead with typical contact and source information.
    /// JSON properties are mapped explicitly to C# properties.
    /// </summary>
    public class Lead
    {
        [JsonPropertyName("contact_id")]
        public string? ContactId { get; set; }

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("quelle")]
        public string? Quelle { get; set; }  // Source or origin of the lead

        [JsonPropertyName("uquelle")]
        public string? Uquelle { get; set; } // Possibly a sub-source or detailed source

        [JsonPropertyName("destination")]
        public string? Destination { get; set; } // Target location or endpoint
    }
}
