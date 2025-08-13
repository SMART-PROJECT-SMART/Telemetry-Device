using PacketDotNet;

namespace TelemetryDevices.Services.Factories.PacketHandler.Handlers
{
    public interface IPacketHandler
    {
        public bool CanHandle(Packet packet);
        public void Handle(Packet packet, Action<byte[]> packetCaught);
    }
}
