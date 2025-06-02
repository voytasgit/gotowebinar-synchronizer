using gotowebinar.Handlers;
using gotowebinar.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace gotowebinar.Utils
{
    /// <summary>
    /// Registers all required services, handlers, and logging for dependency injection.
    /// </summary>
    public class ServiceRegistrator
    {
        /// <summary>
        /// Registers application services, handlers, and configuration into the DI container.
        /// </summary>
        /// <param name="services">The service collection to register dependencies into.</param>
        /// <param name="config">The application configuration root.</param>
        public void RegisterServices(IServiceCollection services, IConfigurationRoot config)
        {
            // Register IConfiguration
            services.AddSingleton<IConfiguration>(config);

            // Register HTTP-based API clients
            services.AddHttpClient<IClientService, ClientService>();
            services.AddHttpClient<IRegistrantService, RegistrantService>();
            services.AddHttpClient<IWebinarService, WebinarService>();
            services.AddHttpClient<IAttendeeService, AttendeeService>();

            // Register file processing services
            services.AddTransient<IRegistrantFileService, RegistrantFileService>();
            services.AddTransient<IWebinarFileService, WebinarFileService>();
            services.AddTransient<ILeadFileService, LeadFileService>();

            // Register application services
            services.AddTransient<IAttendeeDownloadService, AttendeeDownloadService>();
            services.AddTransient<ILeadService, LeadService>();

            // Register business logic handlers
            services.AddTransient<ILeadUploadHandler, LeadUploadHandler>();
            services.AddTransient<ILeadHandler, LeadHandler>();
            services.AddTransient<IRegistrantDownloadHandler, RegistrantDownloadHandler>();
            services.AddTransient<IAttendeeDownloadHandler, AttendeeDownloadHandler>();
            services.AddTransient<IWebinarHandler, WebinarHandler>();

            // Register main application entry point
            services.AddSingleton<App>();

            // Register logging using Serilog
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog(dispose: true);
            });
        }
    }
}
