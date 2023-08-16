namespace WebClient.Services;

public class SomeService : ISomeService
{
    public async Task DoSomethingAsync()
    {
        var nrOfTasks = new Random().Next(0, 10);

        for (var i = 0; i < nrOfTasks; i++)
        {
            //Console.WriteLine($"Processing task number: {i}");
            await Task.Delay(100);
        }
    }
}