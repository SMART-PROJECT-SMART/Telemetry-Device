using Microsoft.Extensions.DependencyInjection;
using Shared.Configuration;
using Shared.Services;
using Shared.Services.ICDsDirectory;
using TelemetryDevices.Common;
using TelemetryDevices.Config;
using TelemetryDevices.Models;
using TelemetryDevices.Services.Builders;
using TelemetryDevices.Services.Factories.PacketHandler;
using TelemetryDevices.Services.Factories.PacketHandler.Handlers;
using TelemetryDevices.Services.Helpers.Decoder;
using TelemetryDevices.Services.Helpers.Output;
using TelemetryDevices.Services.Helpers.Validator;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenApi();
            services.AddLogging();
            return services;
        }

        public static IServiceCollection AddAppConfiguration(
            this IServiceCollection services,
            IConfiguration config
        )
        {
            return services.Configure<NetworkingConfiguration>(
                config.GetSection(TelemetryDeviceConstants.Configuration.NETWORKING_SECTION)
            );
        }

        public static IServiceCollection AddPacketSniffer(this IServiceCollection services)
        {
            services.AddSingleton<IPacketSniffer, PacketSniffer>();
            return services;
        }

        public static IServiceCollection AddICDDirectory(this IServiceCollection services)
        {
            services.AddSingleton<IICDDirectory, ICDDirectory>();
            return services;
        }

        public static IServiceCollection AddPipeline(this IServiceCollection services)
        {
            services.AddTransient<IPipeLine, TelemetryPipeLine>();
            services.AddSingleton<IValidator, ChecksumValidator>();
            services.AddSingleton<ITelemetryDecoder, TelemetryDataDecoder>();
            services.AddSingleton<IOutputHandler, LoggingOutputHandler>();
            services.AddSingleton<IPipelineBuilder, PipelineBuilder>();
            return services;
        }

        public static IServiceCollection AddPortManager(this IServiceCollection services)
        {
            services.AddSingleton<IPortManager, PortManager>();
            return services;
        }

        public static IServiceCollection AddTelemetryServices(this IServiceCollection services)
        {
            services.AddSingleton<IPortManager, PortManager>();
            services.AddSingleton<TelemetryDeviceManager>();
            return services;
        }

        public static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddSingleton<IPacketHandlerFactory, PacketHandlerFactory>();
            return services;
        }

        public static IServiceCollection AddPacketHandlers(this IServiceCollection services)
        {
            services.AddSingleton<IPacketHandler, UdpHandler>();
            return services;
        }

        public static IServiceCollection AddSharedConfiguration(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<ICDSettings>(config.GetSection(TelemetryDeviceConstants.Config.ICD_DIRECTORY));
            return services;
        }
    }
}
