using Microsoft.VisualStudio.TestTools.UnitTesting;
using gotowebinar.Services;
using gotowebinar.Utils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace gotowebinarTests.Services
{
    [TestClass()]
    public class WebinarServiceTests
    {
        private IWebinarService _webinarServices;
        private IWebinarFileService _webinarFileServices;
        private readonly IClientService _clientServices;

        public WebinarServiceTests()
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
            _webinarServices = serviceProvider.GetRequiredService<IWebinarService>();
            _clientServices = serviceProvider.GetRequiredService<IClientService>();
            _webinarFileServices = serviceProvider.GetRequiredService<IWebinarFileService>();
        }
        [TestMethod()]
        public async Task GetAllWebinarsAsyncTest()
        {
            // Authentifizierung und Token abrufen
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");
            // Aktuelles Datum
            DateTime now = DateTime.UtcNow;
            // Einen Monat zurück und nach vorne
            DateTime fromDate = now.AddMonths(-120).Date; // Anfang des Tages
            DateTime toDate = now.AddMonths(3).Date.AddDays(1).AddSeconds(-1); // Ende des Tages

            // ISO 8601 UTC Format
            string fromTime = fromDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string toTime = toDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var webinarResponse = await _webinarServices.GetAllWebinarsAsync(fromTime, toTime, page: 0, size: 200, accessToken: accessToken);
            await _webinarFileServices.SaveWebinarResponseAsync(webinarResponse);
            Assert.Fail();
        }
        [TestMethod()]
        public async Task GetWebinarAsyncTest()
        {
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");
            //accessToken = "";
            string organiserKey = "1234567890"; // ap
            string webinarKey = "12345"; // 
            var response = await _webinarServices.GetWebinarAsync(organiserKey, webinarKey, accessToken);
            //var v2 = response.Data;
            Assert.Fail();
        }
    }
}
