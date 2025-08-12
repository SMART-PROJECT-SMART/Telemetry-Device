using TelemetryDevice.Services.PipeLines;

namespace TelemetryDevice.Models
{
    public class Channel
    {
        public int PortNumber { get; set; }
        public List<IPipeLine> PipeLines { get; set; }

        public Channel() { }

        public Channel(int portNumber)
        {
            PortNumber = portNumber;
            PipeLines = new List<IPipeLine>();
        }

        public Channel(int portNumber, List<IPipeLine> pipeLines)
        {
            PortNumber = portNumber;
            PipeLines = pipeLines;
        }

        public void RunPayloadOnPipelines(byte[] payload)
        {
            foreach (var pipeLine in PipeLines)
            {
                pipeLine.ProcessDataAsync(payload);
            }
        }
    }
}
