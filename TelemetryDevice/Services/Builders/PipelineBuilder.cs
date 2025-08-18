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
        this.Reset();
    }

    public void Reset()
    {
        this._components = new List<IPipelineComponent>();
    }

    public void AddValidator()
    {
        this._components.Add(_validator);
    }

    public void AddDecoder()
    {
        this._components.Add(_decoder);
    }

    public void AddOutputHandler()
    {
        this._components.Add(_outputHandler);
    }

    public IPipeLine GetProduct()
    {
        IPipeLine result = new TelemetryPipeLine(new List<IPipelineComponent>(_components), _logger);
        this.Reset();
        return result;
    }

    public string ListComponents()
    {
        if (_components.Count == 0) return "Pipeline components: None";

        string components = string.Join(", ", _components.Select(c => c.ComponentType));
        return $"Pipeline components: {components}";
    }
}