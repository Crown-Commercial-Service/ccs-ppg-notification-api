using Ccs.Ppg.NotificationService.API;
using Ccs.Ppg.NotificationService.API.CustomOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

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

var app = builder.Build();

app.ConfigurePipeline();


app.Run();
