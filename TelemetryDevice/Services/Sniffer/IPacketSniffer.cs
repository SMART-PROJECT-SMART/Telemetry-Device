namespace TelemetryDevice.Services.Sniffer
{
    public interface IPacketSniffer
    {
        public void AddPort(int port);
        public void RemovePort(int port);
        public List<int> GetPorts();
        public void Dispose();

    }
}
