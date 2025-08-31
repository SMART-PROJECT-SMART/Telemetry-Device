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

    public async Task ProcessDataAsync(byte[] data)
    {
        _validationBlock.Post(data);
        await _outputBlock.Completion;
    }

    public void SetICD(ICD icd)
    {
        ICD = icd;
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
        _validationBlock = new TransformBlock<byte[], DecodingResult>(data =>
        {
            bool isValid = _validator.Validate(data);
            return new DecodingResult(isValid, data);
        });
    }

    private void BuildDecodingBlock()
    {
        _decodingBlock = new TransformBlock<DecodingResult, Dictionary<TelemetryFields, double>>(result =>
            result.IsValid
                ? _telemetryDecoder.DecodeData(result.Data, ICD)
                : new Dictionary<TelemetryFields, double>()
        );
    }

    private void BuildOutputBlock()
    {
        _outputBlock = new ActionBlock<Dictionary<TelemetryFields, double>>(decodedData =>
        {
            _outputHandler.HandleOutput(decodedData, ICD);
        });
    }

    private void LinkBlocks()
    {
        _validationBlock.LinkTo(_decodingBlock, result => result.IsValid);
        _validationBlock.LinkTo(DataflowBlock.NullTarget<DecodingResult>(), result => !result.IsValid);
        _decodingBlock.LinkTo(_outputBlock);
    }
}
