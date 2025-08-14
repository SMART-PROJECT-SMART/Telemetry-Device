using TelemetryDevices.Models;

namespace TelemetryDevices.Services
{
    public interface IPortManager
    {
        void AddPort(int portNumber, Channel channel);
        void RemovePort(int portNumber);
        Channel? GetChannelByPort(int portNumber);
        void SwitchPorts(int sourcePort, int destinationPort);
        IEnumerable<int> GetAllPorts();
    }
}
