using gotowebinar.Models;
using gotowebinar.Services;
using Serilog;

namespace gotowebinar.Handlers
{
    /// <summary>
    /// Interface defining the contract for handling leads asynchronously.
    /// </summary>
    public interface ILeadHandler
    {
        /// <summary>
        /// Executes the lead retrieval process and returns a list of leads.
        /// </summary>
        /// <returns>List of leads</returns>
        Task<List<Lead>> ExecuteAsync();
    }

    /// <summary>
    /// Handler class responsible for reading leads from files via the lead service.
    /// </summary>
    public class LeadHandler : ILeadHandler
    {
        private readonly ILeadService _leadService;

        /// <summary>
        /// Constructor with lead service dependency injection.
        /// </summary>
        /// <param name="leadService">Service to read leads</param>
        public LeadHandler(ILeadService leadService)
        {
            _leadService = leadService;
        }

        /// <summary>
        /// Executes the asynchronous reading of leads from files.
        /// </summary>
        /// <returns>List of leads read</returns>
        public async Task<List<Lead>> ExecuteAsync()
        {
            Log.Debug("Start LeadHandler...");

            // Read leads asynchronously from files using the injected service
            var ListLeads = await _leadService.ReadLeadsFromFilesAsync();

            Log.Debug("Ende LeadHandler.");

            // Return the list of leads retrieved
            return ListLeads;
        }
    }
}
