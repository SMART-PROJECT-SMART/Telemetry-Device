using Microsoft.AspNetCore.Mvc;
using TelemetryDevices.Common;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SniffersController : ControllerBase
    {
        private readonly PacketSniffer _packetSniffer;

        public SniffersController(PacketSniffer packetSniffer)
        {
            _packetSniffer = packetSniffer;
        }

        [HttpPost("add-port")]
        public IActionResult AddPort(int portNumber)
        {
            _packetSniffer.AddPort(portNumber);
            return Ok($"Port {portNumber} added successfully.");
        }

        [HttpPost("remove-port")]
        public IActionResult RemovePort(int portNumber)
        {
            _packetSniffer.RemovePort(portNumber);
            return Ok($"Port {portNumber} removed successfully.");
        }

        [HttpGet("run")]
        public IActionResult Run()
        {
            _packetSniffer.AddPort(TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1);
            _packetSniffer.AddPort(TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2);
            return Ok(
                $"Sniffer started with ports {TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_1} and {TelemetryDeviceConstants.DefaultValues.DEFAULT_PORT_2}."
            );
        }
    }
}
