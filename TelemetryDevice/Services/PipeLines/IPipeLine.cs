using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines
{
    public interface IPipeLine
    {
        public Task ProcessTelemetryDataAsync(byte[] telemetryData);
        public void SetTelemetryICD(ICD telemetryIcd);
        public ICD GetTelemetryICD();
    }
}
