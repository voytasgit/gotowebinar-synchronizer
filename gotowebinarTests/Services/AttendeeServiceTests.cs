using Microsoft.VisualStudio.TestTools.UnitTesting;
using gotowebinar.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gotowebinar.Utils;
using Microsoft.Extensions.DependencyInjection;
using gotowebinar.Models.Attendee;

namespace gotowebinar.Services.Tests
{
    [TestClass()]
    public class AttendeeServiceTests
    {
        private IAttendeeService _service;
        private readonly IClientService _clientServices;
        public AttendeeServiceTests() 
        {
            // Set up dependency injection
            var services = new ServiceCollection();

            // Load configuration
            var loader = new ConfigurationLoader();
            var config = loader.LoadConfiguration();

            // Register services EXE
            var registrator = new ServiceRegistrator();
            registrator.RegisterServices(services, config);

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();

            // Start App
            _service = serviceProvider.GetRequiredService<IAttendeeService>();
            _clientServices = serviceProvider.GetRequiredService<IClientService>();
        }
        [TestMethod()]
        public async Task GetAttendeeAsyncTest()
        {
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");
            string organiserKey = "1234567890"; // Firm
            string webinarKey = "12345"; // actual webinar
            var attendeeResponse = await _service.GetAllAttendeesAsync(organiserKey, webinarKey,0,100, accessToken);
            var v2 =  attendeeResponse.Embedded.AttendeeParticipationResponses;
            Assert.Fail();
        }
    }
}
