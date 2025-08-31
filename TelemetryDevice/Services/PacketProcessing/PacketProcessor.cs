using PacketDotNet;

namespace TelemetryDevices.Services.PacketProcessing;

public class PacketProcessor : IPacketProcessor
{
    public void ProcessPacket(Packet packet, Action<byte[], int> packetCaught)
    {
        if (packet.PayloadPacket is not IPv4Packet ipv4Packet)
            return;

        switch (ipv4Packet.PayloadPacket)
        {
            case UdpPacket { PayloadData.Length: > 0 } udpPacket:
                packetCaught.Invoke(udpPacket.PayloadData, udpPacket.DestinationPort);
                break;

            case TcpPacket { PayloadData.Length: > 0 } tcpPacket:
                packetCaught.Invoke(tcpPacket.PayloadData, tcpPacket.DestinationPort);
                break;
        }
    }
}