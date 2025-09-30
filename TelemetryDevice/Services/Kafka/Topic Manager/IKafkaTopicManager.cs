namespace TelemetryDevices.Services.Kafka.Topic_Manager
{
    public interface IKafkaTopicManager
    {
        Task EnsureTopicExistsAsync(string topicName);
    }
}
