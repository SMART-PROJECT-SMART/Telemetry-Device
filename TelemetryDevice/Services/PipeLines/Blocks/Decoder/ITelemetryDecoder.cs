using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Decoder
{
    public interface ITelemetryDecoder
    {
        public DecodingResult DecodeData(ValidationResult validationResult, ICD telemetryIcd);

    }
}
