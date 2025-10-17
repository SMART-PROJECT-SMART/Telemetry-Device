using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Options;
using Quartz;
using TelemetryDevices.Common;
using TelemetryDevices.Config;
using TelemetryDevices.Services.Kafka.Producers.TelemetryProducer;
using TelemetryDevices.Services.Kafka.Producers.TelemetryDevicesStatusProducer;
using TelemetryDevices.Services.Kafka.Topic_Manager;
using TelemetryDevices.Services.PipeLines;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder;
using TelemetryDevices.Services.PipeLines.Blocks.Decoder.North_South;
using TelemetryDevices.Services.PipeLines.Blocks.Output;
using TelemetryDevices.Services.PipeLines.Blocks.Output.Kafka;
using TelemetryDevices.Services.PipeLines.Blocks.Validator;
using TelemetryDevices.Services.PipeLines.Blocks.Validator.CheckSum;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.Quartz.TelemetryDeviceStatusManager;
using TelemetryDevices.Services.Sniffer;
using TelemetryDevices.Services.TelemetryDevicesManager;

namespace TelemetryDevices.Extensions
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

        public static IServiceCollection AddTelemetryDevicesUpdateJobConfiguration(this IServiceCollection services,
            IConfiguration appConfiguration)
        {
            return services.Configure<TelemetryDeviceStatusConfiguration>(
                appConfiguration.GetSection(
                    TelemetryDeviceConstants.Configuration.TELEMETRY_DEVICE_STATUS_SECTION
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

            KafkaConfiguration? kafkaSettings = appConfiguration
                .GetSection(TelemetryDeviceConstants.Configuration.KAFKA)
                .Get<KafkaConfiguration>();

            ProducerConfig kafkaProducerConfig = new()
            {
                BootstrapServers = kafkaSettings?.BootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                CompressionType = CompressionType.Gzip,
            };

            services.AddSingleton(kafkaProducerConfig);
            services.AddSingleton<IProducer<string, byte[]>>(provider =>
            {
                ProducerConfig config = provider.GetRequiredService<ProducerConfig>();
                return new ProducerBuilder<string, byte[]>(config).Build();
            });
            AddKafkaTelemetryProducer(services);
            AddKafkaTelemetryDeviceStatusProducer(services);
            AddKafkaTopicManager(services);
            return services;
        }

        public static IServiceCollection AddKafkaTelemetryProducer(this IServiceCollection services)
        {
            services.AddSingleton<IKafkaTelemetryProducer, KafkaTelemetryProducer>();
            return services;
        }

        public static IServiceCollection AddKafkaTelemetryDeviceStatusProducer(
            this IServiceCollection services
        )
        {
            services.AddSingleton<ITelemetryDeviceStatusProducer, TelemetryDeviceStatusProducer>();
            return services;
        }

        public static IServiceCollection AddTelemetryPipelineServices(
            this IServiceCollection services
        )
        {
            services.AddSingleton<ITelemetryValidatorBlock, ChecksumTelemetryValidatorBlock>();
            services.AddSingleton<ITelemetryDecoderBlock, TelemetryDecoderBlock>();
            services.AddSingleton<ITelemetryOutputBlock, KafkaTelemetryOutputBlock>();

            services.AddTransient<ITelemetryPipeLine, TelemetryPipeline>();

            return services;
        }

        public static IServiceCollection AddPacketSniffer(this IServiceCollection services)
        {
            services.AddSingleton<IPacketSniffer, PacketSniffer>();
            return services;
        }

        public static IServiceCollection AddPortManager(this IServiceCollection services)
        {
            services.AddSingleton<IPortManager, PortManager>();
            return services;
        }

        public static IServiceCollection AddTelemetryDeviceManager(this IServiceCollection services)
        {
            services.AddSingleton<ITelemetryDeviceManager, TelemetryDeviceManager>();
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

        public static IServiceCollection AddKafkaTopicManager(this IServiceCollection services)
        {
            services.AddSingleton(provider =>
            {
                IOptions<KafkaConfiguration>? kafkaConfig = provider.GetService<
                    IOptions<KafkaConfiguration>
                >();
                AdminClientConfig adminConfig = new()
                {
                    BootstrapServers = kafkaConfig?.Value?.BootstrapServers,
                };
                return new AdminClientBuilder(adminConfig).Build();
            });

            services.AddSingleton<IKafkaTopicManager, KafkaTopicManager>();
            return services;
        }

        public static IServiceCollection AddQuartzServices(this IServiceCollection services)
        {
            services.AddQuartz();
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            services.AddSingleton(provider =>
                provider
                    .GetRequiredService<ISchedulerFactory>()
                    .GetScheduler()
                    .GetAwaiter()
                    .GetResult()
            );
            services.AddSingleton<IQuartzTelemetryDeviceStatusManager, QuartzTelemetryDeviceStatusManager>();
            return services;
        }
    }
}
