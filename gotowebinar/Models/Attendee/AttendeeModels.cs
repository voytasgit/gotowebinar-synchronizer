using System.Text.Json.Serialization;

namespace gotowebinar.Models.Attendee
{
    /// <summary>
    /// Represents the overall response containing embedded attendee data and paging information.
    /// </summary>
    public class AttendeeResponse
    {
        /// <summary>
        /// Embedded attendee participation responses.
        /// </summary>
        [JsonPropertyName("_embedded")]
        public Embedded? Embedded { get; set; }

        /// <summary>
        /// Paging information for the response.
        /// </summary>
        [JsonPropertyName("page")]
        public Page? Page { get; set; }
    }

    /// <summary>
    /// Holds the collection of attendee participation responses.
    /// </summary>
    public class Embedded
    {
        /// <summary>
        /// Array of attendee participation responses.
        /// </summary>
        [JsonPropertyName("attendeeParticipationResponses")]
        public AttendeeParticipationResponse[]? AttendeeParticipationResponses { get; set; }
    }

    /// <summary>
    /// Represents a single attendee's participation response details.
    /// </summary>
    public class AttendeeParticipationResponse
    {
        /// <summary>
        /// Unique key identifying the registrant.
        /// </summary>
        [JsonPropertyName("registrantKey")]
        public string RegistrantKey { get; set; }

        /// <summary>
        /// Unique key identifying the session attended.
        /// </summary>
        [JsonPropertyName("sessionKey")]
        public string SessionKey { get; set; }

        /// <summary>
        /// Email address of the attendee.
        /// </summary>
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        /// <summary>
        /// Total attendance time in seconds for this attendee.
        /// </summary>
        [JsonPropertyName("attendanceTimeInSeconds")]
        public int AttendanceTimeInSeconds { get; set; }

        /// <summary>
        /// Array of individual attendance intervals (join and leave times).
        /// </summary>
        [JsonPropertyName("attendance")]
        public Attendance[]? Attendance { get; set; }

        /// <summary>
        /// First name of the attendee.
        /// </summary>
        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        /// <summary>
        /// Last name of the attendee.
        /// </summary>
        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }
    }

    /// <summary>
    /// Represents a single attendance interval with join and leave timestamps.
    /// </summary>
    public class Attendance
    {
        /// <summary>
        /// DateTime when the attendee joined.
        /// </summary>
        [JsonPropertyName("joinTime")]
        public DateTime JoinTime { get; set; }

        /// <summary>
        /// DateTime when the attendee left.
        /// </summary>
        [JsonPropertyName("leaveTime")]
        public DateTime LeaveTime { get; set; }
    }

    /// <summary>
    /// Represents paging details such as page size, total elements, and current page number.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Number of items per page.
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// Total number of elements available.
        /// </summary>
        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        /// <summary>
        /// Current page number.
        /// </summary>
        [JsonPropertyName("number")]
        public int Number { get; set; }
    }

    /// <summary>
    /// Represents detailed attendee data including personal and registration information.
    /// </summary>
    public class AttendeeData
    {
        /// <summary>
        /// Last name of the attendee.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Email address of the attendee.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// First name of the attendee.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Unique registrant key (uses Int64 for large values).
        /// </summary>
        public long RegistrantKey { get; set; } // Int64 für große Werte

        /// <summary>
        /// Date and time when the attendee registered.
        /// </summary>
        public DateTime RegistrationDate { get; set; } // DateTime für Datumswerte

        /// <summary>
        /// Status of the attendee registration; could be represented as an enum if fixed set of values.
        /// </summary>
        public string Status { get; set; } // Enum kann verwendet werden, falls Status feste Werte hat

        /// <summary>
        /// URL for the attendee to join the webinar.
        /// </summary>
        public string JoinUrl { get; set; }

        /// <summary>
        /// Time zone of the attendee.
        /// </summary>
        public string TimeZone { get; set; }
    }

}
