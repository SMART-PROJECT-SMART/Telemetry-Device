using Core.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines
{
    public interface IPipeLine : IDisposable
    {
        public Task ProcessTelemetryDataAsync(byte[] telemetryData);
    }
}
