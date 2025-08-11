using System.Text;
using PacketDotNet;
using SharpPcap;
using TelemetryDevice.Common;

namespace TelemetryDevice.Services
{
    public class PacketSniffer : IDisposable, IPacketSniffer
    {
        private readonly ILogger<PacketSniffer> _logger;
        private readonly ICaptureDevice _captureDevice;
        private readonly HashSet<int> _ports = new();
        private string _lastAppliedFilter = TelemetryDeviceConstants.Network.UdpFilter;

        public PacketSniffer(ILogger<PacketSniffer> logger)
        {
            _logger = logger;

            var devices = CaptureDeviceList.Instance;
            _logger.LogInformation("Found {DeviceCount} network devices", devices.Count);

            _device = devices.FirstOrDefault(d =>
                d.Description.Contains(TelemetryDeviceConstants.LoopbackInterface.LoopbackDescription)
                || d.Name.Contains(TelemetryDeviceConstants.LoopbackInterface.LoopbackDescription)
            )!;

            _device.Open();
            _device.OnPacketArrival += OnPacketArrival;
            _device.Filter = TelemetryDeviceConstants.Network.UdpFilter;
            _lastAppliedFilter = _device.Filter;
            _device.StartCapture();
            _logger.LogInformation("Packet capture started on {DeviceName}", _device.Description);
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
            var newFilter = BuildFilterFromPorts(_ports, TelemetryDeviceConstants.Network.UdpFilter);

            if (newFilter == _lastAppliedFilter) return;

            _device.Filter = newFilter;
            _lastAppliedFilter = newFilter;

            _logger.LogDebug("Updated filter: {Filter}", newFilter);
        }

        private static string BuildFilterFromPorts(IReadOnlyCollection<int> ports, string baseUdpFilter)
        {
            if (ports.Count == 0)
                return baseUdpFilter;

            var ordered = ports.OrderBy(p => p);

            var sb = new StringBuilder();
            sb.Append(baseUdpFilter);
            sb.Append(" and (");
            bool first = true;
            foreach (var p in ordered)
            {
                if (!first) sb.Append(" or ");
                sb.Append("dst port ").Append(p);
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
                payloadLength > TelemetryDeviceConstants.PacketProcessing.MaxHexPreviewLength
                    ? Convert.ToHexString(payload[..TelemetryDeviceConstants.PacketProcessing.MaxHexPreviewLength]) +
                      TelemetryDeviceConstants.PacketProcessing.HexPreviewSuffix
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
