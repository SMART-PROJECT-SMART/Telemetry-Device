using Shared.Services;
using TelemetryDevices.Common;
using TelemetryDevices.Models;
using TelemetryDevices.Services;
using TelemetryDevices.Services.Extensions;
using TelemetryDevices.Services.Sniffer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();

builder.Services.AddAppConfiguration(builder.Configuration);

builder.Services.AddPacketSniffer();

builder.Services.AddPipelineBlocks();
builder.Services.AddTelemetryDeviceManager();
builder.Services.AddPacketProcessor();
builder.Services.AddIcdDirectory();
builder.Services.AddICDConfiguration(builder.Configuration);
builder.Services.AddPortManager();
builder.Services.AddKafkaServices(builder.Configuration);
builder.Services.AddKafkaTelemetryProducer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
