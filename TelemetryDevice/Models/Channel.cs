using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class Channel
    {
        public int PortNumber { get; set; }
        public IPipeLine PipeLine { get; set; }
        public ICD ICD { get; set; }

        public Channel(int portNumber, IPipeLine pipeLine, ICD icd)
        {
            PortNumber = portNumber;
            PipeLine = pipeLine;
            ICD = icd;
        }

        public void ProcessTelemetryData(byte[] telemetryData)
        {
            if (PipeLine != null)
            {
                var channelIcd = ICD;
                if (channelIcd != null)
                {
                    PipeLine.SetTelemetryICD(channelIcd);
                    _ = PipeLine.ProcessTelemetryDataAsync(telemetryData);
                }
            }
        }
    }
}
