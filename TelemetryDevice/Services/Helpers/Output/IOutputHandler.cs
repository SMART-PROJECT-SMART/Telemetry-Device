using Shared.Common.Enums;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Helpers.Output
{
    public interface IOutputHandler : IPipelineComponent
    {
        PipeLineComponents IPipelineComponent.ComponentType => PipeLineComponents.Output;

        void HandleOutput(Dictionary<TelemetryFields, double> decodedData);
    }
}
