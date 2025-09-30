using Core.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidatorBlock
    {
        ValidationResult ValidateTelemetryData(byte[] compressedTelemetryData, ICD icd);
    }
}
