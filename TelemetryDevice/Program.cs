using TelemetryDevices.Models;
using TelemetryDevices.Services;
using TelemetryDevices.Services.Sniffer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();

builder.Services.AddAppConfiguration(builder.Configuration);

builder.Services.AddPacketSniffer();

builder.Services.AddPipeline();
builder.Services.AddSingleton<TelemetryDeviceManager>();
builder.Services.AddFactories();
builder.Services.AddPacketHandlers();

Shared.Services.ServiceCollectionExtensions.AddIcdDirectoryServices(builder.Services);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

var sniffer = app.Services.GetRequiredService<IPacketSniffer>();

app.Run();
