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
        private HashSet<string> _createdTopics;
        
        public KafkaTopicManager(IAdminClient adminClient, IICDDirectory icdDirectory)
        {
            _adminClient = adminClient;
            _icdDirectory = icdDirectory;
            _createdTopics = new HashSet<string>();
        }

        public async Task EnsureTopicExistsAsync(string topicName)
        {
            if (IsTopicAlreadyTracked(topicName))
                return;

            if (!DoesTopicExistInClusterAsync(topicName))
            {
                await CreateTopicAsync(topicName);
                MarkTopicAsCreated(topicName);
            }
        }

        private bool IsTopicAlreadyTracked(string topicName)
        {
            return _createdTopics.Contains(topicName);
        }

        private bool DoesTopicExistInClusterAsync(string topicName)
        {
            Metadata clusterInfo = _adminClient.GetMetadata(
                TimeSpan.FromSeconds(TelemetryDeviceConstants.Kafka.WAIT_TIMEOUT_SECONDS)
            );
            return clusterInfo.Topics.Any(t => t.Topic == topicName);
        }

        private async Task CreateTopicAsync(string topicName)
        {
            var topicSpec = BuildTopicSpecification(topicName);
            await _adminClient.CreateTopicsAsync(new List<TopicSpecification> { topicSpec });
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

        private void MarkTopicAsCreated(string topicName)
        {
            _createdTopics.Add(topicName);
        }
    }
}
