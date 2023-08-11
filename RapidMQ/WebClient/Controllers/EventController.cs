using Microsoft.AspNetCore.Mvc;
using WebClient.Eventbus;
using WebClient.Events;

namespace WebClient.Controllers;

[ApiController]
[Route("[controller]")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;

    private readonly IEventBus _eventBus;
    
    public EventController(ILogger<EventController> logger, IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    [HttpPost("alertReceived/publish")]
    public async Task<IActionResult> PublishEvent([FromBody] AlertReceivedEvent request)
    {
        _eventBus.PublishEvent("IoT", request);
        
        await Task.Delay(1000);
        
        return StatusCode(200);
    }
}