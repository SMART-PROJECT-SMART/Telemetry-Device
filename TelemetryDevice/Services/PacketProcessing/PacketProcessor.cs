using PacketDotNet;

namespace TelemetryDevices.Services.PacketProcessing;

public class PacketProcessor : IPacketProcessor
{
    public void ProcessPacket(Packet networkPacket, Action<byte[], int> packetCapturedCallback)
    {
        if (networkPacket.PayloadPacket is not IPv4Packet ipv4Packet)
            return;

        switch (ipv4Packet.PayloadPacket)
        {
            case UdpPacket { PayloadData.Length: > 0 } udpPacket:
                packetCapturedCallback.Invoke(udpPacket.PayloadData, udpPacket.DestinationPort);
                break;

            case TcpPacket { PayloadData.Length: > 0 } tcpPacket:
                packetCapturedCallback.Invoke(tcpPacket.PayloadData, tcpPacket.DestinationPort);
                break;
        }
    }
}
