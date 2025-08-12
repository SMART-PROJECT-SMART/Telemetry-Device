using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using TelemetryDevice.Models;
using TelemetryDevice.Services.Helpers.Decoder;
using TelemetryDevice.Services.Helpers.Validator;

namespace TelemetryDevice.Services.PipeLines
{
    public class TelemetryPipeLine : IPipeLine
    {
        private readonly IValidator _validator;
        private readonly ITelemetryDecoder _telemetryDecoder;
        private TransformBlock<byte[], Result> _validationBlock;
        private TransformBlock<Result, Dictionary<TelemetryFields, double>> _decodingBlock;
        private ActionBlock<Dictionary<TelemetryFields, double>> _outputBlock;
        private readonly ILogger<TelemetryPipeLine> _logger;

        public TelemetryPipeLine(IValidator validator, ITelemetryDecoder telemetryDecoder, ILogger<TelemetryPipeLine> logger)
        {
            _validator = validator;
            _telemetryDecoder = telemetryDecoder;
            _logger = logger;
            BuildPipeLine();
        }

        public async Task ProcessDataAsync(byte[] data)
        {
            _validationBlock.Post(data);
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
                result => _telemetryDecoder.DecodeData(result.Data)
            );
        }

        private void BuildOutputBlock()
        {
            _outputBlock = new ActionBlock<Dictionary<TelemetryFields, double>>(decodedData =>
            {
                _logger.LogInformation("Decoded {Count} telemetry fields", decodedData.Count);
                foreach (var kvp in decodedData)
                {
                    _logger.LogInformation("Field: {Key}, Value: {Value}", kvp.Key, kvp.Value);
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