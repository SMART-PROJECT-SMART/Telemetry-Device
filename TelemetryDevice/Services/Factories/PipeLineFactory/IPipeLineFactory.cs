using Shared.Common.Enums;
using TelemetryDevices.Services.PipeLines;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.Factories.PipeLineFactory
{
    public interface IPipeLineFactory
    {
        IPipeLine GetPipeLine(ICD icd);
    }
}
