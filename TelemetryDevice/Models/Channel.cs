using Shared.Models.ICDModels;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Models
{
    public class Channel
    {
        public int PortNumber { get; set; }
        public IPipeLine PipeLine { get; set; }
        public ICD ICD { get; set; }

        public Channel() { }

        public Channel(int portNumber, ICD icd)
        {
            PortNumber = portNumber;
            ICD = icd;
        }

        public Channel(int portNumber, IPipeLine pipeLine, ICD icd)
        {
            PortNumber = portNumber;
            PipeLine = pipeLine;
            ICD = icd;
        }
    }
}
