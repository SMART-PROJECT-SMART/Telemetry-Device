using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;

namespace TelemetryDevices.Services.Builders
{
    public class PipelineBuilder : IPipelineBuilder
    {
        private readonly IServiceProvider _serviceProvider;
        private IValidator _validator;
        private ITelemetryDecoder _decoder;
        private IOutputHandler _outputHandler;

        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            this.Reset();
        }

        public void Reset()
        {
            this._validator = null;
            this._decoder = null;
            this._outputHandler = null;
        }

        public void BuildValidator()
        {
            this._validator = _serviceProvider.GetRequiredService<IValidator>();
        }

        public void BuildDecoder()
        {
            this._decoder = _serviceProvider.GetRequiredService<ITelemetryDecoder>();
        }

        public void BuildOutputHandler()
        {
            this._outputHandler = _serviceProvider.GetRequiredService<IOutputHandler>();
        }

        public IPipeLine GetProduct()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<TelemetryPipeLine>>();

            IPipeLine result = new TelemetryPipeLine(_validator, _decoder, _outputHandler, logger);
            this.Reset();
            return result;
        }
    }
}
