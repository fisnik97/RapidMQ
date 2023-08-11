namespace WebClient.Eventbus;

public static class EventBusFactory
{
    public static async Task<WebApplication> InstantiateEventBus(this WebApplication app)
    {

        

        await Task.Delay(100);

        return app;
    }
    
}