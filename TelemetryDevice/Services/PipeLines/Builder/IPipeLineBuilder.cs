using System.Threading.Tasks.Dataflow;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines.Builder
{
    public interface IPipeLineBuilder
    {
        IPipeLineBuilder AddValidator(IValidator telemetryValidator, ICD telemetryIcd);
        IPipeLineBuilder AddDecoder(ITelemetryDecoder telemetryDecoder, ICD telemetryIcd);
        IPipeLineBuilder AddOutput(IOutputHandler telemetryOutputHandler, ICD telemetryIcd);
        IPipeLineBuilder AddBlock(IDataflowBlock telemetryBlock);
        Pipeline BuildPipeline();
        void Reset();
    }
}
