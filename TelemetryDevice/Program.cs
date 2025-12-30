using Core.Models;
using Core.Services;
using TelemetryDevices.Common;
using TelemetryDevices.Extensions;
using TelemetryDevices.Services.Sniffer;
using TelemetryDevices.Services.TelemetryDevicesManager;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();
builder.Services.AddAppConfiguration(builder.Configuration);
builder.Services.AddTelemetryDevicesUpdateJobConfiguration(builder.Configuration);
builder.Services.AddPacketSniffer();
builder.Services.AddTelemetryDeviceManager();
builder.Services.AddIcdDirectory();
builder.Services.AddICDConfiguration(builder.Configuration);
builder.Services.AddPortManager();
builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddTelemetryPipelineServices();
builder.Services.AddQuartzServices();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

IPacketSniffer sniffer = app.Services.GetRequiredService<IPacketSniffer>();
sniffer.AddPort(TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1);
sniffer.AddPort(TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2);

ITelemetryDeviceManager telemetryDeviceManager =
    app.Services.GetRequiredService<ITelemetryDeviceManager>();
await telemetryDeviceManager.AddTelemetryDeviceAsync(
    TelemetryDeviceConstants.DefaultValues.DEFAULT_TAIL_ID,
    [
        TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1,
        TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2,
    ],
    new Location(
        TelemetryDeviceConstants.DefaultValues.DEFAULT_LOCATION_LAT,
        TelemetryDeviceConstants.DefaultValues.DEFAULT_LOCATION_LON,
        0.0
    )
);

app.Run();
