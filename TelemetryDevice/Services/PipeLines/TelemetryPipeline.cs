using System.Threading.Tasks.Dataflow;
using Core.Common.Enums;
using Core.Models;
using Core.Models.ICDModels;
using Microsoft.Extensions.Logging;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

namespace TelemetryDevices.Services.PipeLines
{
    public class TelemetryPipeline : ITelemetryPipeLine
    {
        private readonly ITelemetryValidatorBlock _telemetryValidatorBlock;
        private readonly ITelemetryDecoderBlock _telemetryDecoderBlock;
        private readonly ITelemetryOutputBlock _telemetryOutputBlock;
        private readonly ILogger<TelemetryPipeline> _logger;
        private TransformBlock<byte[], ValidationResult> _pipelineValidatorBlock;
        private TransformBlock<ValidationResult, DecodingResult> _pipelineDecoderBlock;
        private ActionBlock<DecodingResult> _pipelineOutputBlock;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private int? _currentTailId;
        private bool _hasLoggedFirstDecode;

        public TelemetryPipeline(
            ITelemetryValidatorBlock telemetryValidatorBlock,
            ITelemetryDecoderBlock telemetryDecoderBlock,
            ITelemetryOutputBlock telemetryOutputBlock,
            ILogger<TelemetryPipeline> logger
        )
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _telemetryValidatorBlock = telemetryValidatorBlock;
            _telemetryDecoderBlock = telemetryDecoderBlock;
            _telemetryOutputBlock = telemetryOutputBlock;
            _logger = logger;
        }

        public void BuildPipelineBlocks(ICD telemetryIcd, Action<int, Location> onTelemetryDecoded)
        {
            _pipelineValidatorBlock = new TransformBlock<byte[], ValidationResult>(
                data => _telemetryValidatorBlock.ValidateTelemetryData(data, telemetryIcd),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token,
                }
            );

            _pipelineDecoderBlock = new TransformBlock<ValidationResult, DecodingResult>(
                validationResult =>
                    DecodeAndNotify(validationResult, telemetryIcd, onTelemetryDecoded),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token,
                }
            );

            _pipelineOutputBlock = new ActionBlock<DecodingResult>(
                decodingResult =>
                    _telemetryOutputBlock.OutputTelemetryData(decodingResult, telemetryIcd),
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token,
                }
            );
            LinkTelemetryPipelineBlocks();
        }

        private DecodingResult DecodeAndNotify(
            ValidationResult validationResult,
            ICD telemetryIcd,
            Action<int, Location> onTelemetryDecoded
        )
        {
            DecodingResult result = _telemetryDecoderBlock.DecodeTelemetryData(
                validationResult,
                telemetryIcd
            );
            int decodedTailId = ExtractTailId(result);
            Location location = ExtractLocation(result);
            _currentTailId = decodedTailId;

            if (!_hasLoggedFirstDecode)
            {
                _logger.LogInformation("First telemetry decoded for tailId {TailId}", decodedTailId);
                _hasLoggedFirstDecode = true;
            }

            onTelemetryDecoded(decodedTailId, location);
            return result;
        }

        private int ExtractTailId(DecodingResult result)
        {
            return (int)result.GetValue(TelemetryFields.TailId)!.Value;
        }

        private Location ExtractLocation(DecodingResult result)
        {
            double latitude = result.GetValue(TelemetryFields.Latitude) ?? 0;
            double longitude = result.GetValue(TelemetryFields.Longitude) ?? 0;
            double altitude = result.GetValue(TelemetryFields.Altitude) ?? 0;
            return new Location(latitude, longitude, altitude);
        }

        public async Task ProcessTelemetryDataAsync(byte[] telemetryData)
        {
            bool posted = await _pipelineValidatorBlock.SendAsync(
                telemetryData,
                _cancellationTokenSource.Token
            );
            if (!posted)
            {
                throw new InvalidOperationException("Failed to post data to pipeline");
            }
        }

        private void LinkTelemetryPipelineBlocks()
        {
            _pipelineValidatorBlock.LinkTo(
                _pipelineDecoderBlock,
                new DataflowLinkOptions { PropagateCompletion = true },
                validationResult => validationResult.IsValid
            );
            _pipelineValidatorBlock.LinkTo(DataflowBlock.NullTarget<ValidationResult>());
            _pipelineDecoderBlock.LinkTo(
                _pipelineOutputBlock,
                new DataflowLinkOptions { PropagateCompletion = true }
            );
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _pipelineValidatorBlock.Complete();
            _cancellationTokenSource.Dispose();
        }
    }
}
