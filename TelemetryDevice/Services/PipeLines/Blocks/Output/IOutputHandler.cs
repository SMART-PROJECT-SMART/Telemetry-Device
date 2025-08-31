using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.PipeLines.Blocks.Output
{
    public interface IOutputHandler : IPipelineComponent
    {
        PipeLineComponents IPipelineComponent.ComponentType => PipeLineComponents.Output;

        void HandleOutput(Dictionary<TelemetryFields, double> decodedData,ICD icd);
    }
}
