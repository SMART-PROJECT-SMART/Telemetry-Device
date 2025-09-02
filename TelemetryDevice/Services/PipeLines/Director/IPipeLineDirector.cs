using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.PipeLines.Director
{
    public interface IPipeLineDirector
    {
        Pipeline CreateStandardTelemetryPipeline(ICD icd);
    }
}
