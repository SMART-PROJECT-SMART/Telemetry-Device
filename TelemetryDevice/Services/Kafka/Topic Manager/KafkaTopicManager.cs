using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Core.Services.ICDsDirectory;
using TelemetryDevices.Common;

namespace TelemetryDevices.Services.Kafka.Topic_Manager
{
    public class KafkaTopicManager : IKafkaTopicManager
    {
        private readonly IAdminClient _adminClient;
        private readonly IICDDirectory _icdDirectory;
        
        public KafkaTopicManager(IAdminClient adminClient, IICDDirectory icdDirectory)
        {
            _adminClient = adminClient;
            _icdDirectory = icdDirectory;
        }

        public async Task EnsureTopicExistsAsync(string topicName)
        {
            try
            {
                TopicSpecification topicSpec = BuildTopicSpecification(topicName);
                await _adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });
            }
            catch (CreateTopicsException ex) when (ex.Results[0].Error.Code == ErrorCode.TopicAlreadyExists)
            {
            }
        }

        private TopicSpecification BuildTopicSpecification(string topicName)
        {
            int partitionCount = _icdDirectory.GetICDCount();
            return new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitionCount,
                ReplicationFactor = TelemetryDeviceConstants.Kafka.REPLICATION_FACTOR
            };
        }
    }
}
