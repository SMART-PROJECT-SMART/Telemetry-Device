using Core.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface ITelemetryValidatorBlock
    {
        ValidationResult ValidateTelemetryData(byte[] compressedTelemetryData, ICD icd);
    }
}
