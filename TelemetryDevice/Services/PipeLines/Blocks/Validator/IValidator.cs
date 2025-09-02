using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidator
    {

        public bool Validate(byte[] compressedTelemetryData, ICD icd);
    }
}
