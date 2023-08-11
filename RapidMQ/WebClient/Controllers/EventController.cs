using Microsoft.AspNetCore.Mvc;

namespace WebClient.Controllers;

[ApiController]
[Route("[controller]")]
public class PushEventController : ControllerBase
{
    private readonly ILogger<PushEventController> _logger;

    public PushEventController(ILogger<PushEventController> logger)
    {
        _logger = logger;
    }
    
}