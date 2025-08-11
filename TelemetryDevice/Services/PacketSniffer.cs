using PacketDotNet;
using SharpPcap;
using TelemetryDevice.Common;
using TelemetryDevice.Config;
using Microsoft.Extensions.Options;
using System.Text;

namespace TelemetryDevice.Services
{
    public class PacketSniffer : IDisposable, IPacketSniffer
    {
        private readonly ILogger<PacketSniffer> _logger;
        private readonly IOptions<NetworkingConfiguration> _networkingConfig;
        private readonly List<ICaptureDevice> _devices = new();
        private readonly HashSet<int> _ports = new();
        private string _lastAppliedFilter = string.Empty;

        public PacketSniffer(ILogger<PacketSniffer> logger, IOptions<NetworkingConfiguration> networkingConfig)
        {
            _logger = logger;
            _networkingConfig = networkingConfig;

            var devices = CaptureDeviceList.Instance;
            _logger.LogInformation("Found {DeviceCount} network devices", devices.Count);

            _device = GetCaptureDevice(devices);

            _device.Open();
            _device.OnPacketArrival += OnPacketArrival;
            ApplyFilterLocked();
            _device.StartCapture();
            _logger.LogInformation("Packet capture started on {DeviceName}", _device.Description);
        }

        private ICaptureDevice GetCaptureDevice(CaptureDeviceList devices)
        {
            var config = _networkingConfig.Value;

            foreach (var interfaceName in config.Interfaces)
            {
                var matchedDevice = devices.FirstOrDefault(d =>
                    d.Description.Contains(interfaceName, StringComparison.OrdinalIgnoreCase) ||
                    d.Name.Contains(interfaceName, StringComparison.OrdinalIgnoreCase));

                if (matchedDevice != null)
                    return matchedDevice;
            }

            return devices.First();
        }

        public void AddPort(int port)
        {
            lock (_sync)
            {
                if (!_ports.Add(port)) return;
                ApplyFilterLocked();
                _logger.LogInformation("Added port {Port} to monitoring. Total ports: {Count}", port, _ports.Count);
            }
        }

        public void RemovePort(int port)
        {
            lock (_sync)
            {
                if (!_ports.Remove(port)) return;
                ApplyFilterLocked();
                _logger.LogInformation("Removed port {Port} from monitoring. Total ports: {Count}", port, _ports.Count);
            }
        }

        public void ClearPorts()
        {
            lock (_sync)
            {
                _ports.Clear();
                ApplyFilterLocked();
            }
            _logger.LogInformation("Cleared all ports from monitoring");
        }

        public List<int> GetPorts()
        {
            lock (_sync) return _ports.ToList();
        }

        private void ApplyFilterLocked()
        {
            var config = _networkingConfig.Value;
            var baseFilter = BuildProtocolFilter(config.Protocols);
            var newFilter = BuildFilterFromPorts(_ports, baseFilter);

            if (newFilter == _lastAppliedFilter) return;

            _device.Filter = newFilter;
            _lastAppliedFilter = newFilter;

            _logger.LogDebug("Updated filter: {Filter}", newFilter);
        }

        private string BuildProtocolFilter(List<string> protocols)
        {
            if (protocols == null || protocols.Count == 0)
                return TelemetryDeviceConstants.Network.UDP_FILTER;

            if (protocols.Count == 1)
                return protocols[0];

            return $"({string.Join(TelemetryDeviceConstants.Network.FILTER_SEPARATOR, protocols)})";
        }

        private static string BuildFilterFromPorts(IReadOnlyCollection<int> ports, string baseFilter)
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
                if (!first) sb.Append(TelemetryDeviceConstants.Network.FILTER_SEPARATOR);
                sb.Append(string.Format(TelemetryDeviceConstants.Network.DESTINATION_PORT_FILTER, p));
                first = false;
            }
            sb.Append(')');
            return sb.ToString();
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            var raw = e.GetPacket();
            var packet = Packet.ParsePacket(raw.LinkLayerType, raw.Data);
            var udp = packet.Extract<UdpPacket>();
            if (udp == null) return;

            HandlePacket(udp);
        }

        private void HandlePacket(UdpPacket udp)
        {
            var ipPacket = udp.ParentPacket as IPPacket;
            var sourceIp = ipPacket?.SourceAddress?.ToString();
            var destIp = ipPacket?.DestinationAddress?.ToString();

            var payload = udp.PayloadData;
            var payloadLength = payload.Length;

            var hexPreview =
                payloadLength > TelemetryDeviceConstants.PacketProcessing.MAX_HEX_PREVIEW_LENGTH
                    ? Convert.ToHexString(payload[..TelemetryDeviceConstants.PacketProcessing.MAX_HEX_PREVIEW_LENGTH]) +
                      TelemetryDeviceConstants.PacketProcessing.HEX_PREVIEW_SUFFIX
                    : Convert.ToHexString(payload);

            _logger.LogInformation(
                "UDP Packet: {SourceIp}:{SourcePort} -> {DestIp}:{DestPort}, Length: {Length} bytes, Data: {HexPreview}",
                sourceIp,
                udp.SourcePort,
                destIp,
                udp.DestinationPort,
                payloadLength,
                hexPreview
            );
        }

        public void Dispose()
        {
            _device.OnPacketArrival -= OnPacketArrival;
            _device.StopCapture();
            _device.Close();
        }
    }
}