using Ccs.Ppg.NotificationService.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.ConfigureServices();

var app = builder.Build();

app.ConfigurePipeline();


app.Run();
