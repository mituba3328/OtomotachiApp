using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        Console.WriteLine("Received message, sending back echo");
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}