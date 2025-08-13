using PacketDotNet;
using TelemetryDevices.Services.Factories.PacketHandler.Handlers;

namespace TelemetryDevices.Services.Factories.PacketHandler
{
    public class PacketHandlerFactory : IPacketHandlerFactory
    {
        private readonly IEnumerable<IPacketHandler> _handlers;

        public IPacketHandler GetHandler(Packet packet)
        {
            return _handlers.FirstOrDefault(handler => handler.CanHandle(packet)) 
                   ?? throw new InvalidOperationException("No suitable packet handler found.");
        }
    }
}
