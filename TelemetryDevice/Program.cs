using Shared.Services;
using TelemetryDevices.Models;
using TelemetryDevices.Services;
using TelemetryDevices.Services.Sniffer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();

builder.Services.AddAppConfiguration(builder.Configuration);

builder.Services.AddPacketSniffer();

builder.Services.AddPipeline();
builder.Services.AddTelemetryServices();
builder.Services.AddFactories();
builder.Services.AddPacketHandlers();
builder.Services.AddIcdDirectory();
builder.Services.AddSharedConfiguration(builder.Configuration);
builder.Services.AddPortManager();
builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddProducer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

var sniffer = app.Services.GetRequiredService<IPacketSniffer>();
sniffer.AddPort(8000);
sniffer.AddPort(8001);

var tdManager = app.Services.GetRequiredService<TelemetryDeviceManager>();
tdManager.AddTelemetryDevice(1, [8000, 8001], new Location(0, 0));
app.Run();
