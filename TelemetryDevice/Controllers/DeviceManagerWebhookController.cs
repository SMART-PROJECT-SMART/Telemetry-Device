using Microsoft.AspNetCore.Mvc;
using TelemetryDevices.Dto.DeviceManager;
using TelemetryDevices.Services.SleeveChangeHandlers;

namespace TelemetryDevices.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceManagerWebhookController : ControllerBase
    {
        private readonly ISleeveChangeHandlerFactory _handlerFactory;

        public DeviceManagerWebhookController(ISleeveChangeHandlerFactory handlerFactory)
        {
            _handlerFactory = handlerFactory;
        }

        [HttpPost("sleeve-changed")]
        public async Task<ActionResult> SleeveChanged([FromBody] SleeveChangedNotificationDto notification, CancellationToken cancellationToken)
        {
            ISleeveChangeHandler handler = _handlerFactory.CreateHandler(notification.Operation);
            await handler.HandleSleeveChangeAsync(notification.Id, cancellationToken);
            return Ok();
        }
    }
}
