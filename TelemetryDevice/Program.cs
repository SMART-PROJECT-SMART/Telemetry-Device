using Core.Services;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services;
using TelemetryDevices.Services.Extensions;
using TelemetryDevices.Services.Sniffer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();

builder.Services.AddAppConfiguration(builder.Configuration);

builder.Services.AddPacketSniffer();

builder.Services.AddTelemetryDeviceManager();
builder.Services.AddIcdDirectory();
builder.Services.AddICDConfiguration(builder.Configuration);
builder.Services.AddPortManager();
builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddKafkaTelemetryProducer();
builder.Services.AddKafkaTopicManager();
builder.Services.AddKafkaTopicManager();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();
var sniffer = app.Services.GetRequiredService<IPacketSniffer>();
sniffer.AddPort(TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1);
sniffer.AddPort(TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2);

var tdManager = app.Services.GetRequiredService<TelemetryDeviceManager>();
await tdManager.AddTelemetryDeviceAsync(
    TelemetryDeviceConstants.DefaultValues.DEFAULT_TAIL_ID,
    [
        TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1,
        TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2,
    ],
    new Location(
        TelemetryDeviceConstants.DefaultValues.DEFAULT_LOCATION_LAT,
        TelemetryDeviceConstants.DefaultValues.DEFAULT_LOCATION_LON
    )
);

app.Run();
