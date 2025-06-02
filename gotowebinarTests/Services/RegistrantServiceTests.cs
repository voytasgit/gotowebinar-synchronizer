using Microsoft.VisualStudio.TestTools.UnitTesting;
using gotowebinar.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gotowebinar.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace gotowebinar.Services.Tests
{
    [TestClass()]
    public class RegistrantServiceTests
    {
        private IRegistrantService _service;
        private readonly IClientService _clientServices;
        public RegistrantServiceTests()
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
            _service = serviceProvider.GetRequiredService<IRegistrantService>();
            _clientServices = serviceProvider.GetRequiredService<IClientService>();
        }
        [TestMethod()]
        public async Task GetRegistrantsAsyncTest()
        {
            var accessToken = await _clientServices.RefreshAccessTokenAsync();
            if (accessToken == null)
                throw new Exception("Cant't get accessToken");
            string organiserKey = "1640932452130871566"; // ap
            string webinarKey = "6401117446996529757"; // ETF 5 Feb 2025
            var response = await _service.GetRegistrantsAsync(organiserKey, webinarKey, 0, 100, accessToken);
            var v2 = response.Data;
            Assert.Fail();
        }
    }
}