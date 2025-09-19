using Microsoft.Extensions.Options;
using PacketDotNet;
using SharpPcap;
using TelemetryDevices.Common;
using TelemetryDevices.Config;
using TelemetryDevices.Services.Helpers;
using TelemetryDevices.Services.PacketProcessing;

namespace TelemetryDevices.Services.Sniffer
{
    public class PacketSniffer : IDisposable, IPacketSniffer
    {
        private readonly IOptions<NetworkingConfiguration> _networkingConfig;
        private readonly List<ICaptureDevice> _devices = new();
        private readonly HashSet<int> _ports = new();
        private readonly IPacketProcessor _packetProcessor;
        public event Action<byte[], int> PacketReceived;

        public PacketSniffer(
            IOptions<NetworkingConfiguration> networkingConfig,
            IPacketProcessor packetProcessor
        )
        {
            _networkingConfig = networkingConfig;
            _packetProcessor = packetProcessor;
            var availableDevices = CaptureDeviceList.Instance;
            InitializeDevices(availableDevices);
        }

        private void InitializeDevices(CaptureDeviceList availableDevices)
        {
            if (availableDevices.Count == 0)
            {
                throw new InvalidOperationException("No network capture devices available");
            }

            var networkingConfig = _networkingConfig.Value;

            foreach (
                var matchedCaptureDevice in networkingConfig
                    .Interfaces.Select(configuredInterfaceName =>
                        GetCaptureDevice(availableDevices, configuredInterfaceName)
                    )
                    .OfType<ICaptureDevice>()
            )
            {
                InitializeDevice(matchedCaptureDevice);
                _devices.Add(matchedCaptureDevice);
            }

            if (_devices.Count == 0)
            {
                var fallbackCaptureDevice = availableDevices.First();
                InitializeDevice(fallbackCaptureDevice);
                _devices.Add(fallbackCaptureDevice);
            }
        }

        private ICaptureDevice? GetCaptureDevice(
            CaptureDeviceList availableDevices,
            string configuredInterfaceName
        )
        {
            return availableDevices.FirstOrDefault(captureDevice =>
                captureDevice.Description.Contains(
                    configuredInterfaceName,
                    StringComparison.OrdinalIgnoreCase
                )
                || captureDevice.Name.Contains(
                    configuredInterfaceName,
                    StringComparison.OrdinalIgnoreCase
                )
            );
        }

        private void InitializeDevice(ICaptureDevice captureDevice)
        {
            captureDevice.Open();
            captureDevice.OnPacketArrival += OnPacketArrival;
            ApplyFilterToDevice(captureDevice);
            captureDevice.StartCapture();
        }

        public void AddPort(int port)
        {
            _ports.Add(port);
            ApplyFilterToAllDevices();
        }

        public void RemovePort(int port)
        {
            _ports.Remove(port);
            ApplyFilterToAllDevices();
        }

        public List<int> GetPorts()
        {
            return _ports.ToList();
        }

        private void ApplyFilterToDevice(ICaptureDevice captureDevice, string? deviceFilter = null)
        {
            deviceFilter ??= BuildCurrentFilter();
            captureDevice.Filter = deviceFilter;
        }

        private string BuildCurrentFilter()
        {
            var networkingConfig = _networkingConfig.Value;
            string baseProtocolFilter = FilterHandler.BuildProtocolFilter(
                networkingConfig.Protocols
            );
            return FilterHandler.BuildFilterFromPorts(_ports, baseProtocolFilter);
        }

        private void ApplyFilterToAllDevices()
        {
            string compositePortFilter = BuildCurrentFilter();

            foreach (var captureDevice in _devices)
            {
                ApplyFilterToDevice(captureDevice, compositePortFilter);
            }
        }

        private void OnPacketArrival(object sender, PacketCapture captureArgs)
        {
            TransportPacket? transportPacket = ExtractTransportPacket(captureArgs);
            if (transportPacket == null) return;

            PacketReceived?.Invoke(transportPacket.PayloadData, transportPacket.DestinationPort);
        }

        private TransportPacket? ExtractTransportPacket(PacketCapture captureArgs)
        {
            RawCapture rawCapture = captureArgs.GetPacket();
            Packet parsedPacket = Packet.ParsePacket(rawCapture.LinkLayerType, rawCapture.Data);

            if (parsedPacket?.PayloadPacket is not IPv4Packet ipv4Packet) return null;
            if (ipv4Packet.PayloadPacket is not TransportPacket transportPacket) return null;
            if (transportPacket.PayloadData?.Length <= TelemetryDeviceConstants.PacketProcessing.MINIMUM_PAYLOAD_LENGTH) return null;

            return transportPacket;
        }

        public void Dispose()
        {
            foreach (var captureDevice in _devices)
            {
                captureDevice.OnPacketArrival -= OnPacketArrival;
                captureDevice.StopCapture();
                captureDevice.Close();
            }
            _devices.Clear();
        }
    }
}
