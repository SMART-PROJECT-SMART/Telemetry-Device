using System.Threading.Tasks.Dataflow;
using TelemetryDevice.Services.Helpers;

namespace TelemetryDevice.Models
{
    public class PipeLine
    {
        public byte[] Data { get; set; } = [];
        
        public PipeLine() { }

        public PipeLine(byte[] data)
        {
            Data = data;
        }

        public async Task<bool> ProccessData()
        {
            if (Data.Length == 0)
                return false;

            var validationResult = false;
            var validationBlock = new ActionBlock<byte[]>(data =>
            {
                validationResult = ChecksumValidator.ValidateChecksum(data);
            });

            var posted = validationBlock.Post(Data);
            validationBlock.Complete();
            
            if (!posted)
                return false;

            await validationBlock.Completion;
            
            return validationResult;
        }
    }
}
