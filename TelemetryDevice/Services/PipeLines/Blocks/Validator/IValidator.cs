using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidator : IPipelineComponent
    {
        PipeLineComponents IPipelineComponent.ComponentType => PipeLineComponents.Validator;

        public bool Validate(byte[] compressedData);
    }
}
