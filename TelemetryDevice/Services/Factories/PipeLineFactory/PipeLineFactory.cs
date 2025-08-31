using Shared.Common.Enums;
using TelemetryDevices.Common.Enums;
using TelemetryDevices.Services.PipeLines;
using Shared.Models.ICDModels;

namespace TelemetryDevices.Services.Factories.PipeLineFactory
{
    public class PipeLineFactory : IPipeLineFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PipeLineFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPipeLine GetPipeLine(CommunicationDataType dataType)
        {
            return dataType switch
            {
                CommunicationDataType.Telemetry => _serviceProvider.GetRequiredService<TelemetryPipeLine>(),
                _ => throw new NotImplementedException($"PipeLine for {dataType} is not implemented.")
            };
        }

        public IPipeLine GetPipeLine(ICD icd)
        {
            return GetPipeLine(icd.DataType);
        }
    }
}
