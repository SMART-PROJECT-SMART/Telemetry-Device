using Microsoft.AspNetCore.Mvc;
using TelemetryDevices.Common;
using TelemetryDevices.Dto;
using TelemetryDevices.Services.PortsManager;
using TelemetryDevices.Services.TelemetryDevicesManager;

namespace TelemetryDevices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryDeviceController : ControllerBase
    {
        private readonly ITelemetryDeviceManager _telemetryDeviceManager;
        private readonly IPortManager _portManager;

        public TelemetryDeviceController(
            ITelemetryDeviceManager telemetryDeviceManager,
            IPortManager portManager
        )
        {
            _telemetryDeviceManager = telemetryDeviceManager;
            _portManager = portManager;
        }

        [HttpPost("add-telemetry-device")]
        public IActionResult AddTelemetryDevice(CreateTelemetryDeviceDto deviceDto)
        {
            _ = _telemetryDeviceManager.AddTelemetryDeviceAsync(
                deviceDto.SleeveName,
                deviceDto.SleeveId,
                deviceDto.TailId,
                deviceDto.PortNumbers,
                deviceDto.Location
            );
            return Ok($"Telemetry device for sleeve '{deviceDto.SleeveName}' added successfully.");
        }

        [HttpPost("remove-telemetry-device")]
        public IActionResult RemoveTelemetryDevice(string sleeveName)
        {
            _telemetryDeviceManager.RemoveTelemetryDevice(sleeveName);
            return Ok($"Telemetry device for sleeve '{sleeveName}' removed successfully.");
        }

        [HttpGet("run")]
        public IActionResult Run()
        {
            string defaultSleeveName = "default-sleeve";
            List<int> defaultPortNumbers = new List<int>
            {
                TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1,
                TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2,
            };
            Core.Models.Location defaultLocation = new Core.Models.Location(
                TelemetryDeviceConstants.DefaultValues.DEFAULT_LOCATION_LAT,
                TelemetryDeviceConstants.DefaultValues.DEFAULT_LOCATION_LON,
                0.0
            );
            const int defaultSleeveId = 1;
            _ = _telemetryDeviceManager.AddTelemetryDeviceAsync(
                defaultSleeveName,
                defaultSleeveId,
                null,
                defaultPortNumbers,
                defaultLocation
            );
            return Ok(
                $"Telemetry device for sleeve '{defaultSleeveName}' started with ports {string.Join(", ", defaultPortNumbers)}."
            );
        }

        [HttpPost("switch-port")]
        public IActionResult SwitchPort(int sourcePort, int destinationPort)
        {
            _portManager.SwitchPorts(sourcePort, destinationPort);
            return Ok($"Switched ports {sourcePort} and {destinationPort} successfully.");
        }
    }
}
