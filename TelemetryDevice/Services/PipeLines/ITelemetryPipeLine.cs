using Core.Models;
using Core.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines
{
    public interface ITelemetryPipeLine : IDisposable
    {
        public Task ProcessTelemetryDataAsync(byte[] telemetryData);
        public void BuildPipelineBlocks(ICD telemetryIcd, Action<int, Location> onTelemetryDecoded);
    }
}
