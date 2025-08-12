using System.Threading.Tasks.Dataflow;
using TelemetryDevice.Common.Enums;
using TelemetryDevice.Models;
using TelemetryDevice.Services.Helpers.Decoder;
using TelemetryDevice.Services.Helpers.Validator;

namespace TelemetryDevice.Services.PipeLines
{
    public class TelemetryPipeLine : IPipeLine
    {
        private readonly IValidator _validator;
        private readonly IDecoder<TelemetryFields> _decoder;
        private TransformBlock<byte[], Result> _validationBlock;
        private TransformBlock<Result, Dictionary<TelemetryFields, double>> _decodingBlock;
        private ActionBlock<Dictionary<TelemetryFields, double>> _outputBlock;

        public TelemetryPipeLine(IValidator validator, IDecoder<TelemetryFields> decoder)
        {
            _validator = validator;
            _decoder = decoder;
            BuildPipeLine();
        }

        public async Task ProcessDataAsync(byte[] data)
        {
            _validationBlock.Post(data);
            _validationBlock.Complete();
            await _outputBlock.Completion;
        }

        private void BuildPipeLine()
        {
            BuildValidationBlock();
            BuildDecodingBlock();
            BuildOutputBlock();
            LinkBlocks();
        }

        private void BuildValidationBlock()
        {
            _validationBlock = new TransformBlock<byte[], Result>(data =>
            {
                bool isValid = _validator.Validate(data);
                return new Result(isValid, data);
            });
        }

        private void BuildDecodingBlock()
        {
            _decodingBlock = new TransformBlock<Result, Dictionary<TelemetryFields, double>>(
                result => _decoder.DecodeData(result.Data)
            );
        }

        private void BuildOutputBlock()
        {
            _outputBlock = new ActionBlock<Dictionary<TelemetryFields, double>>(decodedData =>
            {
                foreach (var kvp in decodedData)
                {
                    Console.WriteLine($"{kvp.Key}:{kvp.Value}");
                }
            });
        }

        private void LinkBlocks()
        {
            _validationBlock.LinkTo(_decodingBlock, result => result.IsValid);
            _validationBlock.LinkTo(DataflowBlock.NullTarget<Result>(), result => !result.IsValid);
            _decodingBlock.LinkTo(_outputBlock);
        }
    }
}
