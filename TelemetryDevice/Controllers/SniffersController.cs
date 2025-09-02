using Microsoft.AspNetCore.Mvc;
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
            _packetSniffer.AddPort(8000);
            _packetSniffer.AddPort(8001);
            return Ok("Sniffer started with ports 8000 and 8001.");
        }
    }
}
