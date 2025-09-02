using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines
{
    public interface IPipeLine
    {
        public Task ProcessDataAsync(byte[] telemetryData);
        public void SetICD(ICD telemetryIcd);
        public ICD GetICD();
    }
}
