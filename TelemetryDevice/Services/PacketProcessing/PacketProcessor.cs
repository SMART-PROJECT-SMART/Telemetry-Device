using PacketDotNet;

namespace TelemetryDevices.Services.PacketProcessing;

public class PacketProcessor : IPacketProcessor
{
    public void ProcessPacket(Packet packet, Action<byte[], int> packetCaught)
    {
        switch (packet.PayloadPacket?.PayloadPacket)
        {
            case UdpPacket udpPacket:
                if (udpPacket.PayloadData is { Length: > 0 })
                {
                    packetCaught.Invoke(udpPacket.PayloadData, udpPacket.DestinationPort);
                }
                break;

            case TcpPacket tcpPacket:
                if (tcpPacket.PayloadData is { Length: > 0 })
                {
                    packetCaught.Invoke(tcpPacket.PayloadData, tcpPacket.DestinationPort);
                }
                break;

            default:
                break;
        }
    }
}