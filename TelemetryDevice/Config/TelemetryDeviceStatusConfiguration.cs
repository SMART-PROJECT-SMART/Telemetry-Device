namespace TelemetryDevices.Config
{
    public class TelemetryDeviceStatusConfiguration
    {
        public int JobInterval { get; set; }
        public string TopicName { get; set; }
        public TelemetryDeviceStatusConfiguration() { }
        public TelemetryDeviceStatusConfiguration(int jobInterval, string topicName)
        {
            JobInterval = jobInterval;
            TopicName = topicName;
        }
    }
}
