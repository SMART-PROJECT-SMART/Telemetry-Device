using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Factories.PipeLineFactory
{
    public interface IPipeLineFactory
    {
        IPipeLine GetPipeLine(ICD telemetryIcd);
    }
}
