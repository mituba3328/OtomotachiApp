using Microsoft.AspNetCore.SignalR.Client;

namespace Client
{
    public class Program
    {
        private static HubConnection _connection;
        static bool pen1;
        static bool pen0;
        static bool penoff;
        static int sleep_time=0;
        public static async Task Main(string[] args)
        {            
            _connection = new HubConnectionBuilder()
                .WithUrl("http://192.168.0.70:32820/chatHub")                
                .Build();

            _connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await _connection.StartAsync();
            };

            await ConnectAsync();        
            GetLoacl();
        }

        public static void GetLoacl()
        {
            while (true)
            {
                Send(Console.ReadLine());
            }
        }
        private static async Task ConnectAsync()
        {
            _connection.On<string>("ReceiveMessage", async (message) =>
            {
                Console.WriteLine("Received message");
                Console.WriteLine($"message: {message}");
                string[] msg = message.Split(':');
                switch (msg[0])
                {
                    case "pen0":
                    if (msg[1]=="Using") {
                        pen0=true;
                        sleep_time=0;
                    }
                    else{
                        pen0=false;
                        sleep_time++;
                    }
                    break;
                    case "pen1":
                    if (msg[1]=="Using") {pen1=true;}
                    else{pen1=false;}
                    break;
                }
                if(pen0 && pen1)
                {
                    if (penoff)
                    {
                        Send("onLed");
                        penoff = false;
                    }
                }
                if (sleep_time == 3)
                {
                    Send("sleep");
                    penoff=true;
                    await Task.Delay(1000);
                }
                
            });
            try
            {
                await _connection.StartAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        private static async void Send( string msg)
        {
            try
            {
                await _connection.InvokeAsync("SendMessage", msg);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
        }
    }
}