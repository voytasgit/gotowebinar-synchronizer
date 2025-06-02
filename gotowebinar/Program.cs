using gotowebinar;
using gotowebinar.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

// -------------------- Load Configuration --------------------

var loader = new ConfigurationLoader();
var config = loader.LoadConfiguration();

// -------------------- Initialize Refresh Token Environment Variable --------------------

var refreshToken = Environment.GetEnvironmentVariable("ApiSettings__refreshToken_gw", EnvironmentVariableTarget.User);

if (string.IsNullOrEmpty(refreshToken))
{
    // Try to read from app configuration
    refreshToken = config["ApiSettings:initial_refresh_token"];

    if (!string.IsNullOrEmpty(refreshToken))
    {
        // Store token as environment variable for later usage
        Environment.SetEnvironmentVariable("ApiSettings__refreshToken_gw", refreshToken, EnvironmentVariableTarget.User);

        Console.WriteLine("Environment variable 'ApiSettings__refreshToken_gw' initialized.");
        Log.Information("Environment variable 'ApiSettings__refreshToken_gw' initialized.");
    }
    else
    {
        Console.WriteLine("No initial refresh token found in configuration.");
        Log.Warning("No initial refresh token found in configuration.");
    }
}
else
{
    Console.WriteLine("Environment variable 'ApiSettings__refreshToken_gw' is already set.");
    Log.Information("Environment variable 'ApiSettings__refreshToken_gw' is already set.");
}

// -------------------- Configure Dependency Injection --------------------

var services = new ServiceCollection();

// Register all application and infrastructure services
var registrator = new ServiceRegistrator();
registrator.RegisterServices(services, config);

// Build service provider for resolving dependencies
var serviceProvider = services.BuildServiceProvider();

// Optionally resolve ILogger<App> for application-level logging
var logger = serviceProvider.GetRequiredService<ILogger<App>>();

// -------------------- Start Application --------------------

try
{
    var app = serviceProvider.GetRequiredService<App>();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
