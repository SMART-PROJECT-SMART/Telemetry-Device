using Core.Services;
using TelemetryDevices.Extensions;

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
builder.Services.AddDeviceManagerHttpClient(builder.Configuration);
builder.Services.AddDeviceManagerServices();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
