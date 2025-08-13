using PacketDotNet;
using TelemetryDevices.Services.Factories.PacketHandler;
using TelemetryDevices.Services.Factories.PacketHandler.Handlers;

public class PacketHandlerFactory : IPacketHandlerFactory
{
    private readonly IEnumerable<IPacketHandler> _handlers;

    public PacketHandlerFactory(IEnumerable<IPacketHandler> handlers)
    {
        _handlers = handlers ?? throw new ArgumentNullException(nameof(handlers));
    }

    public IPacketHandler GetHandler(Packet packet)
    {
        return _handlers.FirstOrDefault(handler => handler.CanHandle(packet))
               ?? throw new InvalidOperationException("No suitable packet handler found.");
    }
}