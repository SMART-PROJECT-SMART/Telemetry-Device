using Confluent.Kafka;
using Shared.Configuration;
using TelemetryDevices.Common;
using TelemetryDevices.Config;
using TelemetryDevices.Services.Kafka.Producers;
using TelemetryDevices.Services.PacketProcessing;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;
using TelemetryDevices.Services.PipeLines.Director;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            services.AddControllers();
            services.AddOpenApi();
            return services;
        }

        public static IServiceCollection AddAppConfiguration(
            this IServiceCollection services,
            IConfiguration appConfiguration
        )
        {
            return services.Configure<NetworkingConfiguration>(
                appConfiguration.GetSection(
                    TelemetryDeviceConstants.Configuration.NETWORKING_SECTION
                )
            );
        }

        public static IServiceCollection AddKafkaServices(
            this IServiceCollection services,
            IConfiguration appConfiguration
        )
        {
            services.Configure<KafkaConfiguration>(
                appConfiguration.GetSection(TelemetryDeviceConstants.Configuration.KAFKA)
            );

            var kafkaSettings = appConfiguration
                .GetSection(TelemetryDeviceConstants.Configuration.KAFKA)
                .Get<KafkaConfiguration>()!;

            var kafkaProducerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                CompressionType = CompressionType.Gzip,
            };

            services.AddSingleton(kafkaProducerConfig);
            return services;
        }

        public static IServiceCollection AddKafkaTelemetryProducer(this IServiceCollection services)
        {
            services.AddSingleton<ITelemetryProducer, TelemetryProducer>();
            return services;
        }

        public static IServiceCollection AddPacketSniffer(this IServiceCollection services)
        {
            services.AddSingleton<IPacketSniffer, PacketSniffer>();
            return services;
        }

        public static IServiceCollection AddPipelineBlocks(this IServiceCollection services)
        {
            services.AddTransient<IValidator, ChecksumValidator>();
            services.AddTransient<ITelemetryDecoder, TelemetryDataDecoder>();
            services.AddTransient<IOutputHandler, KafkaOutputHandler>();
            services.AddTransient<IPipeLineDirector, PipelineDirector>();
            return services;
        }

        public static IServiceCollection AddPortManager(this IServiceCollection services)
        {
            services.AddSingleton<IPortManager, PortManager>();
            return services;
        }

        public static IServiceCollection AddTelemetryDeviceManager(this IServiceCollection services)
        {
            services.AddSingleton<TelemetryDeviceManager>();
            return services;
        }

        public static IServiceCollection AddPacketProcessor(this IServiceCollection services)
        {
            services.AddSingleton<IPacketProcessor, PacketProcessor>();
            return services;
        }

        public static IServiceCollection AddICDConfiguration(
            this IServiceCollection services,
            IConfiguration appConfiguration
        )
        {
            services.Configure<ICDSettings>(
                appConfiguration.GetSection(TelemetryDeviceConstants.Config.ICD_DIRECTORY)
            );
            return services;
        }
    }
}
