using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using Ccs.Ppg.NotificationService.API;
using Ccs.Ppg.NotificationService.API.CustomOptions;

var builder = WebApplication.CreateBuilder(args);

String envName = "";

if (!String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")))
{
  envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
}

builder.Configuration
  .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"appsettings.{envName}.json", optional: true);

var vaultEnabled = builder.Configuration.GetValue<bool>("VaultEnabled");
if (!vaultEnabled)
{
  builder.Configuration.AddJsonFile("appsecrets.json", optional: false, reloadOnChange: true);
}
else
{
  var source = builder.Configuration.GetValue<string>("Source");
  if (source.ToUpper() == "AWS")
  {
    builder.Configuration.AddParameterStore();
  }
  else
  {
    builder.Configuration.AddVault(options =>
    {
      var vaultOptions = builder.Configuration.GetSection("Vault");
      options.Address = vaultOptions["Address"];
    });
  }
}

// Add services to the container.
builder.Services.ConfigureServices(builder.Configuration);

string startupUrl = Environment.GetEnvironmentVariable("STARTUP_URL");
if (!string.IsNullOrWhiteSpace(startupUrl))
{
  builder.WebHost.UseUrls(startupUrl);
}

var app = builder.Build();

if (!string.IsNullOrEmpty(builder.Configuration["EnableXRay"]) && Convert.ToBoolean(builder.Configuration["EnableXRay"]))
{
  Console.WriteLine("x-ray is enabled.");
  AWSXRayRecorder.InitializeInstance(configuration: builder.Configuration);
  app.UseXRay("NotificationApi");
  AWSSDKHandler.RegisterXRayForAllServices();
}

app.ConfigurePipeline();

app.Run();