using Core.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class Channel : IDisposable
    {
        public int PortNumber { get; set; }
        public ITelemetryPipeLine TelemetryPipeLine { get; set; }

        public Channel(int portNumber, ITelemetryPipeLine telemetryPipeline)
        {
            PortNumber = portNumber;
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
