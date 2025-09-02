using System.Threading.Tasks.Dataflow;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;
using Shared.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidator
    {
        public bool Validate(byte[] compressedTelemetryData, ICD icd);
        TransformBlock<byte[], DecodingResult> GetBlock(ICD icd);
    }
}
