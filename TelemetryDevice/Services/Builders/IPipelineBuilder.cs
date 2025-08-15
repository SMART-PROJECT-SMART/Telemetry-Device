using Shared.Common.Enums;
using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Builders
{
    public interface IPipelineBuilder
    {
        IPipelineBuilder WithValidator(IValidator validator);
        IPipelineBuilder WithDecoder(ITelemetryDecoder decoder);
        IPipelineBuilder WithOutputHandler(IOutputHandler outputHandler);
        IPipeLine Build();
    }

    public interface IOutputHandler
    {
        void HandleOutput(Dictionary<TelemetryFields, double> decodedData);
    }
}
