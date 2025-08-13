using Microsoft.AspNetCore.Mvc;
using TelemetryDevices.Services.Sniffer;

namespace TelemetryDevices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SniffersController : ControllerBase
    {
        private readonly PacketSniffer _sniffer;
        public SniffersController(PacketSniffer sniffer)
        {
            _sniffer = sniffer;
        }

        [HttpPost("add-port")]
        public void AddPort(int portNumber)
        {
            _sniffer.AddPort(portNumber);
        }

        [HttpPost("remove-port")]
        public void RemovePort(int portNumber)
        {
            _sniffer.RemovePort(portNumber);
        }
    }
}
