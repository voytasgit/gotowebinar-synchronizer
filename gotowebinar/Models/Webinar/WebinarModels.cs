namespace gotowebinar.Models.Webinar
{
    /// <summary>
    /// Represents the response returned for a webinar list request.
    /// Contains embedded webinar data and paging information.
    /// </summary>
    public class WebinarResponse
    {
        /// <summary>
        /// Embedded webinar data.
        /// </summary>
        public Embedded _embedded { get; set; }

        /// <summary>
        /// Paging information about the response.
        /// </summary>
        public Page page { get; set; }
    }

    /// <summary>
    /// Contains an array of Webinar objects.
    /// </summary>
    public class Embedded
    {
        /// <summary>
        /// Array of webinars.
        /// </summary>
        public Webinar[] webinars { get; set; }
    }

    /// <summary>
    /// Represents details of a single webinar.
    /// </summary>
    public class Webinar
    {
        /// <summary>
        /// Unique key identifying the webinar.
        /// </summary>
        public string webinarKey { get; set; }

        /// <summary>
        /// Webinar ID.
        /// </summary>
        public string webinarId { get; set; }

        /// <summary>
        /// Subject or title of the webinar.
        /// </summary>
        public string subject { get; set; }

        /// <summary>
        /// Description of the webinar.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// Organizer key for the webinar.
        /// </summary>
        public string organizerKey { get; set; }

        /// <summary>
        /// OMID (organizer meeting ID) of the webinar.
        /// </summary>
        public string omid { get; set; }

        /// <summary>
        /// Array of time slots when the webinar takes place.
        /// </summary>
        public Time[] times { get; set; }

        /// <summary>
        /// URL where participants can register for the webinar.
        /// </summary>
        public string registrationUrl { get; set; }

        /// <summary>
        /// Time zone of the webinar.
        /// </summary>
        public string timeZone { get; set; }

        /// <summary>
        /// Locale for the webinar (language/region).
        /// </summary>
        public string locale { get; set; }

        /// <summary>
        /// Indicates whether the webinar is impromptu (spontaneous).
        /// </summary>
        public bool impromptu { get; set; }
    }

    /// <summary>
    /// Represents a time slot with start and end times.
    /// </summary>
    public class Time
    {
        /// <summary>
        /// Start time of the webinar slot.
        /// </summary>
        public string startTime { get; set; }

        /// <summary>
        /// End time of the webinar slot.
        /// </summary>
        public string endTime { get; set; }
    }

    /// <summary>
    /// Contains paging metadata for the response.
    /// </summary>
    public class Page
    {
        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int size { get; set; }

        /// <summary>
        /// Total number of elements across all pages.
        /// </summary>
        public int totalElements { get; set; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        public int totalPages { get; set; }

        /// <summary>
        /// Current page number (zero-based).
        /// </summary>
        public int number { get; set; }
    }
}
