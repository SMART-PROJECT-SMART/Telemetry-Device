using System.Threading.Tasks.Dataflow;
using Shared.Models.ICDModels;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidator
    {
        public bool Validate(byte[] compressedTelemetryData, ICD icd);
        TransformBlock<byte[], DecodingResult> GetBlock(ICD icd);
    }
}
