using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.PipeLines;

public class PipeLineDirector
{
    private readonly IPipeLineBuilder _builder;

    public PipeLineDirector(IPipeLineBuilder builder)
    {
        _builder = builder;
    }

    public IPipeLine BuildTelemetryPipeline()
    {
        _builder.Reset();
        _builder.AddValidator();
        _builder.AddDecoder();
        _builder.AddOutputHandler();
        return _builder.GetProduct();
    }

    public IPipeLine BuildDebugPipeline()
    {
        _builder.Reset();
        _builder.AddValidator();
        _builder.AddOutputHandler();
        return _builder.GetProduct();
    }

    public IPipeLine BuildMinimalPipeline()
    {
        _builder.Reset();
        _builder.AddValidator();
        return _builder.GetProduct();
    }
}