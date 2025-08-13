using PacketDotNet;
using TelemetryDevices.Services.Factories.PacketHandler.Handlers;

public class UdpHandler : IPacketHandler
{
    public bool CanHandle(Packet packet)
    {
        var udpPacket = packet.Extract<UdpPacket>();
        return udpPacket?.PayloadData is { Length: > 0 };
    }

    public void Handle(Packet packet, Action<byte[]> packetCaught)
    {
        var udpPacket = packet.Extract<UdpPacket>();
        if (udpPacket?.PayloadData is { Length: > 0 })
        {
            packetCaught.Invoke(udpPacket.PayloadData);
        }
    }
}