using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public interface IOutputHandler
    {
        void HandleOutput(
            Dictionary<TelemetryFields, double> decodedTelemetryData,
            ICD telemetryIcd
        );
        ActionBlock<Dictionary<TelemetryFields, double>> GetBlock(ICD icd);
    }
}
