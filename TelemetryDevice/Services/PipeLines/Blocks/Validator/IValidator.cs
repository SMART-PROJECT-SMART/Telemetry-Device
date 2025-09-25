using System.Threading.Tasks.Dataflow;
using Shared.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidator : IPropagatorBlock<byte[], DecodingResult>
    {
        bool Validate(byte[] compressedTelemetryData, ICD icd);
    }
}
