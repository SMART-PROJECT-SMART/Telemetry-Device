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
var tdManager = app.Services.GetRequiredService<TelemetryDeviceManager>();
var ports = new List<int>();
ports.Add(8000);
ports.Add(8001);
tdManager.AddTelemetryDevice(1, ports, new Location(0, 0));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

var sniffer = app.Services.GetRequiredService<IPacketSniffer>();
sniffer.AddPort(8000);

app.Run();
