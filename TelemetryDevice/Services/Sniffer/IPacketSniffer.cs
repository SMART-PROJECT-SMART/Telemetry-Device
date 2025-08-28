namespace TelemetryDevices.Services.Sniffer
{
    public interface IPacketSniffer
    {
        public event Action<byte[], int> PacketReceived;
        public void AddPort(int port);
        public void RemovePort(int port);
        public List<int> GetPorts();
        public void Dispose();
    }
}
