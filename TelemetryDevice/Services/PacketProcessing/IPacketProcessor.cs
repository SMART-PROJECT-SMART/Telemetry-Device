using PacketDotNet;

namespace TelemetryDevices.Services.PacketProcessing;

public interface IPacketProcessor
{
    void ProcessPacket(Packet networkPacket, Action<byte[], int> packetCapturedCallback);
}
