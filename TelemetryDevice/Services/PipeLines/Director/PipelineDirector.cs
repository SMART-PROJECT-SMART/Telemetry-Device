using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines.Builder;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines.Director
{
    public class PipelineDirector : IPipeLineDirector
    {
        private readonly IValidator _telemetryValidator;
        private readonly ITelemetryDecoder _telemetryDecoder;
        private readonly IOutputHandler _telemetryOutputHandler;

        public PipelineDirector(
            IValidator telemetryValidator,
            ITelemetryDecoder telemetryDecoder,
            IOutputHandler telemetryOutputHandler)
        {
            _telemetryValidator = telemetryValidator;
            _telemetryDecoder = telemetryDecoder;
            _telemetryOutputHandler = telemetryOutputHandler;
        }

        public Pipeline CreateStandardTelemetryPipeline(ICD telemetryIcd)
        {
            var telemetryPipelineBuilder = new PipelineBuilder();
            
            return telemetryPipelineBuilder
                .AddValidator(_telemetryValidator, telemetryIcd)
                .AddDecoder(_telemetryDecoder, telemetryIcd)
                .AddOutput(_telemetryOutputHandler, telemetryIcd)
                .BuildPipeline();
        }
    }
}
