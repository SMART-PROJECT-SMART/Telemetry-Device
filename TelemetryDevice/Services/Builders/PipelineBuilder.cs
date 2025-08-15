using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Builders
{
    public class PipelineBuilder : IPipelineBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private IValidator? _customValidator;
        private ITelemetryDecoder? _customDecoder;
        private IOutputHandler? _customOutputHandler;

        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPipelineBuilder WithValidator(IValidator validator)
        {
            _customValidator = validator;
            return this;
        }

        public IPipelineBuilder WithDecoder(ITelemetryDecoder decoder)
        {
            _customDecoder = decoder;
            return this;
        }

        public IPipelineBuilder WithOutputHandler(IOutputHandler outputHandler)
        {
            _customOutputHandler = outputHandler;
            return this;
        }

        public IPipeLine Build()
        {
            var validator = _customValidator ?? _serviceProvider.GetRequiredService<IValidator>();
            var decoder = _customDecoder ?? _serviceProvider.GetRequiredService<ITelemetryDecoder>();
            var outputHandler = _customOutputHandler ?? _serviceProvider.GetRequiredService<IOutputHandler>();
            var logger = _serviceProvider.GetRequiredService<ILogger<TelemetryPipeLine>>();

            return new TelemetryPipeLine(validator, decoder, outputHandler, logger);
        }
    }
}
