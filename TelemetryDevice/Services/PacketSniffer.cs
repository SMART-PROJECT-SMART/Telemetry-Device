using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using PacketDotNet;
using SharpPcap;
using TelemetryDevice.Common;

namespace TelemetryDevice.Services
{
    public class PacketSniffer : IDisposable
    {
        private readonly ILogger<PacketSniffer> _logger;
        private ICaptureDevice _device;
        private readonly List<int> _ports = new();

        public PacketSniffer(ILogger<PacketSniffer> logger)
        {
            _logger = logger;

            var devices = CaptureDeviceList.Instance;
            _logger.LogInformation("Found {DeviceCount} network devices", devices.Count);

            _device = devices.FirstOrDefault(d =>
                d.Description.Contains(
                    TelemetryDeviceConstants.LoopbackInterface.LoopbackDescription
                )
                || d.Name.Contains(TelemetryDeviceConstants.LoopbackInterface.LoopbackName)
                || d.Description.Contains(TelemetryDeviceConstants.LoopbackInterface.LoopbackLo0)
                || d.Name.Contains(TelemetryDeviceConstants.LoopbackInterface.LoopbackNpfDevice)
            );

            _device.Open();
            _device.OnPacketArrival += OnPacketArrival;
            _device.Filter = TelemetryDeviceConstants.Network.UdpFilter;
            _device.StartCapture();
            _logger.LogInformation("Packet capture started on {DeviceName}", _device.Description);
        }

        public void AddPort(int port)
        {
            if (_ports.Contains(port))
                return;

            _ports.Add(port);
            UpdateFilter();
            _logger.LogInformation(
                "Added port {Port} to monitoring. Total ports: {Count}",
                port,
                _ports.Count
            );
        }

        public void RemovePort(int port)
        {
            if (!_ports.Remove(port))
                return;

            UpdateFilter();
            _logger.LogInformation(
                "Removed port {Port} from monitoring. Total ports: {Count}",
                port,
                _ports.Count
            );
        }

        public void ClearPorts()
        {
            _ports.Clear();
            UpdateFilter();
            _logger.LogInformation("Cleared all ports from monitoring");
        }

        public int[] GetPorts() => _ports.ToArray();

        private void UpdateFilter()
        {
            if (_ports.Count == 0)
            {
                _device.Filter = TelemetryDeviceConstants.Network.UdpFilter;
                return;
            }

            var portFilters = _ports.Select(p =>
                string.Format(TelemetryDeviceConstants.Network.DestinationPortFilter, p)
            );
            var combinedPortFilters = string.Join(
                TelemetryDeviceConstants.Network.FilterSeparator,
                portFilters
            );
            _device.Filter = string.Format(
                TelemetryDeviceConstants.Network.UdpPortFilter,
                combinedPortFilters
            );
            _logger.LogInformation("Updated filter to: {Filter}", _device.Filter);
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            var packet = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var udp = packet.Extract<UdpPacket>();

            if (udp == null)
                return;

            HandlePacket(udp);
        }

        private void HandlePacket(UdpPacket udp)
        {
            var ipPacket = udp.ParentPacket as IPPacket;
            var sourceIp =
                ipPacket?.SourceAddress?.ToString()
                ?? TelemetryDeviceConstants.PacketProcessing.UnknownAddress;
            var destIp =
                ipPacket?.DestinationAddress?.ToString()
                ?? TelemetryDeviceConstants.PacketProcessing.UnknownAddress;

            var payload = udp.PayloadData ?? [];
            var payloadLength = payload.Length;

            var hexPreview =
                payloadLength > TelemetryDeviceConstants.PacketProcessing.MaxHexPreviewLength
                    ? Convert.ToHexString(
                        payload[..TelemetryDeviceConstants.PacketProcessing.MaxHexPreviewLength]
                    ) + TelemetryDeviceConstants.PacketProcessing.HexPreviewSuffix
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
            _device.StopCapture();
            _device?.Close();
        }
    }
}
