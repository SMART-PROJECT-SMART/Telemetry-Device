using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public interface ITelemetryDecoder : IPipelineComponent
    {
        PipeLineComponents IPipelineComponent.ComponentType => PipeLineComponents.Decoder;
        public Dictionary<TelemetryFields, double> DecodeData(byte[] data, ICD icd);
    }
}
