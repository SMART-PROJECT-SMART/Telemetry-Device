using TelemetryDevice.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSingleton<PacketSniffer>();

var app = builder.Build();

var sniffer = app.Services.GetRequiredService<PacketSniffer>();
sniffer.AddPort(8000);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
