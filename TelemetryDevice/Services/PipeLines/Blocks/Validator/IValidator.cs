using Core.Models.ICDModels;
using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PipeLines.Blocks.Validator
{
    public interface IValidator
    {
        ValidationResult Validate(byte[] compressedTelemetryData, ICD icd);
    }
}
