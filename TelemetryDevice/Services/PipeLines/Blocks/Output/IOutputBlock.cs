using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public interface IOutputBlock : ITargetBlock<DecodingResult>
    {
        void HandleOutput(DecodingResult decodingResult, ICD telemetryIcd);
    }
}
