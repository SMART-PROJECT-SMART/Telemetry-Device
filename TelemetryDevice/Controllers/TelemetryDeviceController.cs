using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult AddTelemetryDevice(CreateTelemetryDeviceDto dto)
        {
            _telemetryDeviceManager.AddTelemetryDevice(dto.TailId, dto.PortNumbers, dto.Location);
            return Ok($"Telemetry device with tail ID {dto.TailId} added successfully.");
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
            int tailId = 1;
            List<int> portNumbers = new List<int> { 8000, 8001 };
            Location location = new Location(0, 0);
            _telemetryDeviceManager.AddTelemetryDevice(tailId, portNumbers, location);
            return Ok(
                $"Telemetry device with tail ID {tailId} started with ports {string.Join(", ", portNumbers)}."
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
