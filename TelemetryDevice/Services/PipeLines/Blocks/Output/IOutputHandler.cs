using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public interface IOutputHandler : ITargetBlock<Dictionary<TelemetryFields, double>>
    {
        void HandleOutput(
            Dictionary<TelemetryFields, double> decodedTelemetryData,
            ICD telemetryIcd
        );
    }
}
