using Core.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class Channel : IDisposable
    {
        public int PortNumber { get; set; }
        public ITelemetryPipeLine TelemetryPipeLine { get; set; }
        public ICD ICD { get; set; }

        public Channel(int portNumber, ICD icd, ITelemetryPipeLine telemetryPipeline)
        {
            PortNumber = portNumber;
            ICD = icd;
            TelemetryPipeLine = telemetryPipeline;
        }

        public void ProcessTelemetryData(byte[] telemetryData)
        {
            _ = TelemetryPipeLine.ProcessTelemetryDataAsync(telemetryData);
        }

        public void Dispose()
        {
            TelemetryPipeLine?.Dispose();
        }
    }
}
