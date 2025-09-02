using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public interface ITelemetryDecoder
    {
        public Dictionary<TelemetryFields, double> DecodeData(
            byte[] rawTelemetryData,
            ICD telemetryIcd
        );
        TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> GetBlock(ICD icd);
    }
}
