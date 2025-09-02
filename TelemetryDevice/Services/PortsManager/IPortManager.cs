using TelemetryDevices.Models;

namespace TelemetryDevices.Services.PortsManager
{
    public interface IPortManager
    {
        void AddPort(int portNumber, Channel assignedChannel);
        void RemovePort(int portNumber);
        Channel? GetChannelByPort(int portNumber);
        void SwitchPorts(int sourcePort, int destinationPort);
        IEnumerable<int> GetAllPorts();
        void ProcessPacketOnPort(int portNumber, byte[] packetPayload);
    }
}
