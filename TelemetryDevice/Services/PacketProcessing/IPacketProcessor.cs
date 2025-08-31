using PacketDotNet;

namespace TelemetryDevices.Services.PacketProcessing;

public interface IPacketProcessor
{
    void ProcessPacket(Packet packet, Action<byte[], int> packetCaught);
}