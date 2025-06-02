using System.Text.Json.Serialization;

namespace gotowebinar.Models.Registrant.Registrant
{
    /// <summary>
    /// Represents a paginated response containing a list of registrants along with paging metadata.
    /// </summary>
    public class RegistrantPaginatedResponse
    {
        /// <summary>
        /// List of registrants returned in the current page.
        /// </summary>
        public List<Registrant>? Data { get; set; } // Die Liste der Registranten, jetzt unter "data"

        /// <summary>
        /// Total number of registrants available.
        /// </summary>
        public int Total { get; set; }             // Gesamtanzahl der Registranten

        /// <summary>
        /// Current page number (zero-based or one-based depending on API).
        /// </summary>
        public int Page { get; set; }              // Aktuelle Seite

        /// <summary>
        /// Number of items requested per page.
        /// </summary>
        public int Limit { get; set; }             // Anzahl der Einträge pro Seite

        /// <summary>
        /// Actual number of entries returned on the current page.
        /// </summary>
        public int PageSize { get; set; }          // Tatsächliche Anzahl der Einträge auf der aktuellen Seite
    }

    /// <summary>
    /// Represents a single registrant for a webinar.
    /// </summary>
    public class Registrant
    {
        /// <summary>
        /// Last name of the registrant.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Email address of the registrant.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// First name of the registrant.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Unique key identifying the registrant.
        /// </summary>
        public long RegistrantKey { get; set; }

        /// <summary>
        /// Date and time when the registrant registered.
        /// </summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Status of the registration. Possible values: APPROVED, WAITING, DENIED.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// URL for the registrant to join the webinar.
        /// </summary>
        public string? JoinUrl { get; set; }

        /// <summary>
        /// Time zone of the registrant.
        /// </summary>
        public string? TimeZone { get; set; }
    }

    /// <summary>
    /// Represents an external registrant with JSON property mapping.
    /// </summary>
    public class ExternerRegistrant
    {
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; } = "";

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; } = "";

        [JsonPropertyName("email")]
        public string? Email { get; set; } = "";

        [JsonPropertyName("phone")]
        public string? Phone { get; set; } = "";

        [JsonPropertyName("source")]
        public string? Source { get; set; } = "";
    }

    /// <summary>
    /// Represents detailed registrant data including optional fields and custom responses.
    /// </summary>
    public class RegistrantData
    {
        // Required fields
        public string LastName { get; set; } // required
        public string Email { get; set; } // required
        public string FirstName { get; set; } // required
        public long RegistrantKey { get; set; } // required
        public DateTime RegistrationDate { get; set; } // required
        public string Status { get; set; } // required, Enum: "APPROVED", "DENIED", "WAITING"
        public string JoinUrl { get; set; } // required
        public string TimeZone { get; set; } // required

        // Optional fields
        public string Source { get; set; } // optional
        public string Phone { get; set; } // optional
        public string State { get; set; } // optional, US only
        public string City { get; set; } // optional
        public string Organization { get; set; } // optional
        public string ZipCode { get; set; } // optional
        public string NumberOfEmployees { get; set; } // optional
        public string Industry { get; set; } // optional
        public string JobTitle { get; set; } // optional
        public string PurchasingRole { get; set; } // optional
        public string ImplementationTimeFrame { get; set; } // optional
        public string PurchasingTimeFrame { get; set; } // optional
        public string QuestionsAndComments { get; set; } // optional
        public string EmployeeCount { get; set; } // optional
        public string Country { get; set; } // optional
        public string Address { get; set; } // optional

        /// <summary>
        /// Type of registrant, e.g. "LATE" or "REGULAR".
        /// </summary>
        public string Type { get; set; } // optional, Enum: "LATE", "REGULAR"

        /// <summary>
        /// Indicates if the registrant has unsubscribed.
        /// </summary>
        public bool? Unsubscribed { get; set; } // optional

        /// <summary>
        /// List of custom answers provided by the registrant.
        /// </summary>
        public List<CustomAnswer> Responses { get; set; } // optional
    }

    /// <summary>
    /// Represents a custom question and answer pair from a registrant.
    /// </summary>
    public class CustomAnswer
    {
        /// <summary>
        /// The question asked.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        /// The answer provided.
        /// </summary>
        public string Answer { get; set; }
    }

    /// <summary>
    /// Represents a simplified registrant response with selected properties.
    /// </summary>
    public class RegistrantResponse
    {
        [JsonPropertyName("registrantKey")]
        public long RegistrantKey { get; set; }

        [JsonPropertyName("joinUrl")]
        public string? JoinUrl { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("asset")]
        public bool Asset { get; set; }
    }
}
