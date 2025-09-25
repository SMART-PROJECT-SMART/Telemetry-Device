using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;

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
