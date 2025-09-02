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

        public Channel() { }

        public Channel(int portNumber, ICD channelIcd)
        {
            PortNumber = portNumber;
            ICD = channelIcd;
        }

        public Channel(int portNumber, IPipeLine telemetryPipeline, ICD channelIcd)
        {
            PortNumber = portNumber;
            PipeLine = telemetryPipeline;
            ICD = channelIcd;
            if (telemetryPipeline != null && channelIcd != null)
            {
                telemetryPipeline.SetICD(channelIcd);
            }
        }
    }
}
