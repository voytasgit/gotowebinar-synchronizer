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
    public class ClientServiceTests
    {
        // manually got, s. readme
        string authorizationCode = "eyJraXX...";

        private readonly IClientService _clientServices;
        public ClientServiceTests()
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

            _clientServices = serviceProvider.GetRequiredService<IClientService>();
        }

        [TestMethod()]
        public async Task GetAccessTokenAsyncTest()
        {
            var response = await _clientServices.GetAccessTokenAsync(authorizationCode);
            Assert.Fail();
        }

    }
}
