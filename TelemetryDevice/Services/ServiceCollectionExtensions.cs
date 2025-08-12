using TelemetryDevice.Common;
using TelemetryDevice.Config;
using TelemetryDevice.Services.Helpers;
using TelemetryDevice.Services.PipeLines;
using TelemetryDevice.Services.Sniffer;

namespace TelemetryDevice.Services
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

        public static IServiceCollection AddAppConfiguration(this IServiceCollection services,IConfiguration config)
        {
            return services.Configure<NetworkingConfiguration>(
                config.GetSection(TelemetryDeviceConstants.Configuration.NETWORKING_SECTION));
        }

        public static IServiceCollection AddPacketSniffer(this IServiceCollection services)
        {
            services.AddSingleton<IPacketSniffer, PacketSniffer>();
            return services;
        }

        public static IServiceCollection AddValidation(this IServiceCollection services)
        {
            services.AddSingleton<IValidator,ChecksumValidator>();
            return services;
        }

        public static IServiceCollection AddPipeline(this IServiceCollection services)
        {
            services.AddSingleton<IPipeLine, PipeLine>();
            return services;
        }
    }
}
