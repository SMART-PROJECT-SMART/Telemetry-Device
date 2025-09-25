using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public interface ITelemetryDecoder : IPropagatorBlock<DecodingResult, Dictionary<TelemetryFields, double>>
    {
        Dictionary<TelemetryFields, double> DecodeData(byte[] rawTelemetryData, ICD telemetryIcd);
    }
}
