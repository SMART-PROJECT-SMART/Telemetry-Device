namespace TelemetryDevices.Services.Sniffer
{
    public interface IPacketSniffer : IDisposable
    {
        public event Action<byte[], int> PacketReceived;
        public void AddPort(int port);
        public void RemovePort(int port);
        public List<int> GetPorts();
    }
}
