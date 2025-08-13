using PacketDotNet;
using TelemetryDevices.Services.Factories.PacketHandler.Handlers;

namespace TelemetryDevices.Services.Factories.PacketHandler
{
    public interface IPacketHandlerFactory
    {
        public IPacketHandler GetHandler(Packet packet);
    }
}
