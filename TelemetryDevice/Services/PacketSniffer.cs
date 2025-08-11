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

            _captureDevice = devices.FirstOrDefault(d =>
                d.Description.Contains(
                    TelemetryDeviceConstants.LoopbackInterface.LoopbackDescription
                ) || d.Name.Contains(TelemetryDeviceConstants.LoopbackInterface.LoopbackDescription)
            )!;

            _captureDevice.Open();
            _captureDevice.OnPacketArrival += OnPacketArrival;
            _captureDevice.Filter = TelemetryDeviceConstants.Network.UdpFilter;
            _lastAppliedFilter = _captureDevice.Filter;
            _captureDevice.StartCapture();
        }

        public void AddPort(int port)
        {
            if (!_ports.Add(port))
                return;
            ApplyFilterLocked();
        }

        public void RemovePort(int port)
        {
            if (!_ports.Remove(port))
                return;
            ApplyFilterLocked();
        }

        public void ClearPorts()
        {
            _ports.Clear();
            ApplyFilterLocked();
        }

        public List<int> GetPorts()
        {
            return _ports.ToList();
        }

        private void ApplyFilterLocked()
        {
            var newFilter = BuildFilterFromPorts(
                _ports,
                TelemetryDeviceConstants.Network.UdpFilter
            );

            if (newFilter == _lastAppliedFilter)
                return;

            _captureDevice.Filter = newFilter;
            _lastAppliedFilter = newFilter;
        }

        private string BuildFilterFromPorts(IReadOnlyCollection<int> ports, string baseUdpFilter)
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
                if (!first)
                    sb.Append(" or ");
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
            if (udp == null)
                return;

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
            _captureDevice.OnPacketArrival -= OnPacketArrival;
            _captureDevice.StopCapture();
            _captureDevice.Close();
        }
    }
}
