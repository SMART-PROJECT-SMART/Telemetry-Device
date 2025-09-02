using System.Threading.Tasks.Dataflow;
using Shared.Common.Enums;
using Shared.Models.ICDModels;
using TelemetryDevices.Models;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;

public class TelemetryPipeLine : IPipeLine
{
    private readonly IValidator _validator;
    private readonly ITelemetryDecoder _telemetryDecoder;
    private readonly IOutputHandler _outputHandler;
    private TransformBlock<byte[], DecodingResult> _validationBlock;
    private TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>> _decodingBlock;
    private ActionBlock<Dictionary<TelemetryFields, double>> _outputBlock;
    public ICD ICD { get; set; }

    public TelemetryPipeLine(IValidator validator, ITelemetryDecoder telemetryDecoder, IOutputHandler outputHandler)
    {
        _validator = validator;
        _telemetryDecoder = telemetryDecoder;
        _outputHandler = outputHandler;
    }

    public async Task ProcessDataAsync(byte[] telemetryData)
    {
        _validationBlock.Post(telemetryData);
        await _outputBlock.Completion;
    }

    public void SetICD(ICD telemetryIcd)
    {
        ICD = telemetryIcd;
        BuildPipeLine();
    }

    public ICD GetICD()
    {
        return ICD;
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
        _validationBlock = new TransformBlock<byte[], DecodingResult>(rawTelemetryData =>
        {
            bool isDataValid = _validator.Validate(rawTelemetryData);
            return new DecodingResult(isDataValid, rawTelemetryData);
        });
    }

    private void BuildDecodingBlock()
    {
        _decodingBlock = new TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>>(validationResult =>
            validationResult.IsValid
                ? _telemetryDecoder.DecodeData(validationResult.Data, ICD)
                : new Dictionary<TelemetryFields, double>()
        );
    }

    private void BuildOutputBlock()
    {
        _outputBlock = new ActionBlock<Dictionary<TelemetryFields, double>>(decodedTelemetryData =>
        {
            _outputHandler.HandleOutput(decodedTelemetryData, ICD);
        });
    }

    private void LinkBlocks()
    {
        _validationBlock.LinkTo(_decodingBlock, validationResult => validationResult.IsValid);
        _validationBlock.LinkTo(DataflowBlock.NullTarget<DecodingResult>(), validationResult => !validationResult.IsValid);
        _decodingBlock.LinkTo(_outputBlock);
    }
}
