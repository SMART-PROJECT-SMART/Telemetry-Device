using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Common.Enums;
using TelemetryDevices.Dto;
using TelemetryDevices.Models;
using TelemetryDevices.Services;
using TelemetryDevices.Services.PortsManager;

namespace TelemetryDevices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TelemetryDeviceController : ControllerBase
    {
        private readonly TelemetryDeviceManager _telemetryDeviceManager;
        private readonly IPortManager _portManager;

        public TelemetryDeviceController(
            TelemetryDeviceManager telemetryDeviceManager,
            IPortManager portManager
        )
        {
            _telemetryDeviceManager = telemetryDeviceManager;
            _portManager = portManager;
        }

        [HttpPost("add-telemetry-device")]
        public IActionResult AddTelemetryDevice(CreateTelemetryDeviceDto deviceDto)
        {
            _telemetryDeviceManager.AddTelemetryDevice(deviceDto.TailId, deviceDto.PortNumbers, deviceDto.Location);
            return Ok($"Telemetry device with tail ID {deviceDto.TailId} added successfully.");
        }

        [HttpPost("remove-telemetry-device")]
        public IActionResult RemoveTelemetryDevice(int tailId)
        {
            _telemetryDeviceManager.RemoveTelemetryDevice(tailId);
            return Ok($"Telemetry device with tail ID {tailId} removed successfully.");
        }

        [HttpGet("run")]
        public IActionResult Run()
        {
            int defaultTailId = 1;
            var defaultPortNumbers = new List<int> { 8000, 8001 };
            Location defaultLocation = new Location(0, 0);
            _telemetryDeviceManager.AddTelemetryDevice(defaultTailId, defaultPortNumbers, defaultLocation);
            return Ok(
                $"Telemetry device with tail ID {defaultTailId} started with ports {string.Join(", ", defaultPortNumbers)}."
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
