using System.Threading.Tasks.Dataflow;
using TelemetryDevice.Services.Helpers;

namespace TelemetryDevice.Services.PipeLines
{
    public class PipeLine : IPipeLine
    {
        private readonly IValidator _validator;

        public PipeLine(IValidator validator)
        {
            _validator = validator;
        }

        public async Task ProcessDataAsync(byte[] data)
        {
            var validationResult = new TaskCompletionSource<bool>();

            var validationBlock = new ActionBlock<byte[]>(inputData =>
            {
                var isValid = _validator.Validate(inputData);
                validationResult.SetResult(isValid);
            });

            var posted = validationBlock.Post(data);
            validationBlock.Complete();


            await validationBlock.Completion;
        }

    }
}