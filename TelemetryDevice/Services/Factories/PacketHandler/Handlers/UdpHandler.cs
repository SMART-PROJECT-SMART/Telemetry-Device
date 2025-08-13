using PacketDotNet;

namespace TelemetryDevices.Services.Factories.PacketHandler.Handlers
{
    public class UdpHandler : IPacketHandler
    {
        public bool CanHandle(Packet packet)
        {
            return packet is UdpPacket;
        }

        public void Handle(Packet packet, Action<byte[]> packetCaught)
        {
            var udpPacket = packet.Extract<UdpPacket>();
            byte[] payload = udpPacket.PayloadData;
            packetCaught.Invoke(payload);
        }
    }
}
