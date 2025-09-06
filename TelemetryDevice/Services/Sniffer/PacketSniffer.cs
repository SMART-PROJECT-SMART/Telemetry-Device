using Microsoft.Extensions.Options;
using PacketDotNet;
using SharpPcap;
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

            if (_devices.Count != 0)
                return;
            var fallbackCaptureDevice = availableDevices.First();
            InitializeDevice(fallbackCaptureDevice);
            _devices.Add(fallbackCaptureDevice);
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

        private void ApplyFilterToAllDevices()
        {
            var networkingConfig = _networkingConfig.Value;
            string baseProtocolFilter = FilterHandler.BuildProtocolFilter(
                networkingConfig.Protocols
            );
            string compositePortFilter = FilterHandler.BuildFilterFromPorts(
                _ports,
                baseProtocolFilter
            );

            foreach (var captureDevice in _devices)
            {
                ApplyFilterToDevice(captureDevice, compositePortFilter);
            }
        }

        private void ApplyFilterToDevice(ICaptureDevice captureDevice, string? deviceFilter = null)
        {
            if (deviceFilter == null)
            {
                var networkingConfig = _networkingConfig.Value;
                string baseProtocolFilter = FilterHandler.BuildProtocolFilter(
                    networkingConfig.Protocols
                );
                deviceFilter = FilterHandler.BuildFilterFromPorts(_ports, baseProtocolFilter);
            }

            captureDevice.Filter = deviceFilter;
        }

        private void OnPacketArrival(object sender, PacketCapture captureEventArgs)
        {
            RawCapture rawPacketData = captureEventArgs.GetPacket();
            Packet parsedPacket = Packet.ParsePacket(
                rawPacketData.LinkLayerType,
                rawPacketData.Data
            );
            _packetProcessor.ProcessPacket(parsedPacket, PacketReceived);
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
