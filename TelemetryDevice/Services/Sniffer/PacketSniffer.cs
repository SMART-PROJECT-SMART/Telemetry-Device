using System.Text;
using Microsoft.Extensions.Options;
using PacketDotNet;
using SharpPcap;
using TelemetryDevice.Common;
using TelemetryDevice.Config;
using TelemetryDevice.Models;
using TelemetryDevice.Services.PipeLines;

namespace TelemetryDevice.Services.Sniffer
{
    public class PacketSniffer : IDisposable, IPacketSniffer
    {
        private readonly ILogger<PacketSniffer> _logger;
        private readonly IOptions<NetworkingConfiguration> _networkingConfig;
        private readonly List<ICaptureDevice> _devices = new();
        private readonly HashSet<int> _ports = new();
        private readonly IPipeLine _pipeLine;
        private string _lastAppliedFilter = string.Empty;

        public PacketSniffer(
            ILogger<PacketSniffer> logger,
            IOptions<NetworkingConfiguration> networkingConfig,
            IPipeLine pipeLine
        )
        {
            _logger = logger;
            _networkingConfig = networkingConfig;
            _pipeLine = pipeLine;
            var availableDevices = CaptureDeviceList.Instance;
            InitializeDevices(availableDevices);
        }

        private void InitializeDevices(CaptureDeviceList availableDevices)
        {
            var config = _networkingConfig.Value;

            foreach (
                var matchedDevice in config
                    .Interfaces.Select(interfaceName =>
                        GetCaptureDevice(availableDevices, interfaceName)
                    )
                    .OfType<ICaptureDevice>()
            )
            {
                InitializeDevice(matchedDevice);
                _devices.Add(matchedDevice);
            }

            if (_devices.Count != 0)
                return;
            _logger.LogWarning(
                "No devices found for configured interfaces, using first available device"
            );
            var fallbackDevice = availableDevices.First();
            InitializeDevice(fallbackDevice);
            _devices.Add(fallbackDevice);
        }

        private ICaptureDevice? GetCaptureDevice(
            CaptureDeviceList availableDevices,
            string interfaceName
        )
        {
            return availableDevices.FirstOrDefault(d =>
                d.Description.Contains(interfaceName, StringComparison.OrdinalIgnoreCase)
                || d.Name.Contains(interfaceName, StringComparison.OrdinalIgnoreCase)
            );
        }

        private void InitializeDevice(ICaptureDevice device)
        {
            device.Open();
            device.OnPacketArrival += OnPacketArrival;
            ApplyFilterToDevice(device);
            device.StartCapture();
            _logger.LogInformation("Started capture on device: {DeviceName}", device.Description);
        }

        public void AddPort(int port)
        {
            if (!_ports.Add(port))
                return;
            ApplyFilterToAllDevices();
        }

        public void RemovePort(int port)
        {
            if (!_ports.Remove(port))
                return;
            ApplyFilterToAllDevices();
        }

        public void ClearPorts()
        {
            _ports.Clear();
            ApplyFilterToAllDevices();
        }

        public List<int> GetPorts()
        {
            return _ports.ToList();
        }

        private void ApplyFilterToAllDevices()
        {
            var config = _networkingConfig.Value;
            var baseFilter = BuildProtocolFilter(config.Protocols);
            var newFilter = BuildFilterFromPorts(_ports, baseFilter);

            if (newFilter == _lastAppliedFilter)
                return;

            foreach (var device in _devices)
            {
                ApplyFilterToDevice(device, newFilter);
            }

            _lastAppliedFilter = newFilter;
            _logger.LogDebug(
                "Updated filter on {DeviceCount} devices: {Filter}",
                _devices.Count,
                newFilter
            );
        }

        private void ApplyFilterToDevice(ICaptureDevice device, string? filter = null)
        {
            if (filter == null)
            {
                var config = _networkingConfig.Value;
                var baseFilter = BuildProtocolFilter(config.Protocols);
                filter = BuildFilterFromPorts(_ports, baseFilter);
            }

            device.Filter = filter;
        }

        private string BuildProtocolFilter(List<string> protocols)
        {
            if (protocols.Count == 0)
                return TelemetryDeviceConstants.Network.UDP_FILTER;

            if (protocols.Count == 1)
                return protocols[0];

            return $"({string.Join(TelemetryDeviceConstants.Network.FILTER_SEPARATOR, protocols)})";
        }

        private static string BuildFilterFromPorts(
            IReadOnlyCollection<int> ports,
            string baseFilter
        )
        {
            if (ports.Count == 0)
                return baseFilter;

            var ordered = ports.OrderBy(p => p);

            var sb = new StringBuilder();
            sb.Append(baseFilter);
            sb.Append(" and (");
            bool first = true;
            foreach (var p in ordered)
            {
                if (!first)
                    sb.Append(TelemetryDeviceConstants.Network.FILTER_SEPARATOR);
                sb.Append(
                    string.Format(TelemetryDeviceConstants.Network.DESTINATION_PORT_FILTER, p)
                );
                first = false;
            }
            sb.Append(')');
            return sb.ToString();
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            RawCapture raw = e.GetPacket();
            var packet = Packet.ParsePacket(raw.LinkLayerType, raw.Data);

            HandlePacket(packet);
        }

        private void HandlePacket(Packet packet)
        {
            byte[] payload = packet.PayloadData;

            _pipeLine.ProcessDataAsync(payload);
        }

        public void Dispose()
        {
            foreach (var device in _devices)
            {
                device.OnPacketArrival -= OnPacketArrival;
                device.StopCapture();
                device.Close();
            }
            _devices.Clear();
        }
    }
}
