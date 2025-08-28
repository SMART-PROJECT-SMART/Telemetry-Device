namespace TelemetryDevices.Config
{
    public class KafkaConfiguration
    {
        public string BootstrapServers { get; set; } = String.Empty;

        public KafkaConfiguration() { }

        public KafkaConfiguration(string bootstrapServers)
        {
            BootstrapServers = bootstrapServers;
        }
    }
}
