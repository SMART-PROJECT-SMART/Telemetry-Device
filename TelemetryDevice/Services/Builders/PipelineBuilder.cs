using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Output;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

public class PipeLineBuilder : IPipeLineBuilder
{
    private readonly IValidator _validator;
    private readonly ITelemetryDecoder _decoder;
    private readonly IOutputHandler _outputHandler;
    private readonly ILogger<TelemetryPipeLine> _logger;
    private List<IPipelineComponent> _components = new List<IPipelineComponent>();

    public PipeLineBuilder(
        IValidator validator,
        ITelemetryDecoder decoder,
        IOutputHandler outputHandler,
        ILogger<TelemetryPipeLine> logger)
    {
        _validator = validator;
        _decoder = decoder;
        _outputHandler = outputHandler;
        _logger = logger;
        Reset();
    }

    public void Reset()
    {
        _components = new List<IPipelineComponent>();
    }

    public void AddValidator()
    {
        _components.Add(_validator);
    }

    public void AddDecoder()
    {
        _components.Add(_decoder);
    }

    public void AddOutputHandler()
    {
        _components.Add(_outputHandler);
    }

    public IPipeLine GetProduct()
    {
        IPipeLine result = new TelemetryPipeLine(new List<IPipelineComponent>(_components), _logger);
        Reset();
        return result;
    }
}