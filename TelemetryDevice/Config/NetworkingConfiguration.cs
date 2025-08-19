namespace TelemetryDevices.Config
{
    public class NetworkingConfiguration
    {
        public List<string> Protocols { get; set; } = new();
        public List<string> Interfaces { get; set; } = new();

        public NetworkingConfiguration() { }

        public NetworkingConfiguration(List<string> protocols, List<string> interfaces)
        {
            Protocols = protocols;
            Interfaces = interfaces;
        }
    }
}
