using Microsoft.Extensions.Options;
using TelemetryDevice.Common;
using TelemetryDevice.Config;
using TelemetryDevice.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddOpenApi();

builder.Services.Configure<NetworkingConfiguration>(
    builder.Configuration.GetSection(TelemetryDeviceConstants.Configuration.NETWORKING_SECTION));

builder.Services.AddSingleton<IPacketSniffer, PacketSniffer>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
