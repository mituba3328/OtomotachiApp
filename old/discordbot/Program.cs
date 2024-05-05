using System.Configuration;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
public class Program
{
    public static DiscordSocketClient client;
    public static CommandService commands;
    public static IServiceProvider services;
    public static async Task Main(string[] args)
    {
        new Program().MainAsync().GetAwaiter().GetResult();
    }
    public Program()
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        };
        client = new DiscordSocketClient(config);
        client.Log += LogAsync;
        client.Ready += onReady;
    }
    public async Task MainAsync()
    {
        commands = new CommandService();
        services = new ServiceCollection().BuildServiceProvider();
        //client.Ready += Client_ready;
        await commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services);
        await client.LoginAsync(TokenType.Bot,"MTIwNjI0NDg3MjI4NjEwOTc1Ng.GxUUPj.LzjT3xxa9gH3moioK5_Cfy0t4jfhqyKVPXuBzQ");
        await client.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }
    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
    private async Task onReady()
    {
        Console.WriteLine($"{client.CurrentUser} is Running!!");
        while (true)
        {
            await Task.Delay(5000);
            string sleepStatus = File.ReadAllText(@"../sendMsgSleep.tmp");
            Console.WriteLine(sleepStatus);
            if (sleepStatus == "sleep"){
                await sendMsgGrass();
                File.WriteAllText(@"../sendMsgSleep.tmp", "working!");
            }
            string penStatus = File.ReadAllText(@"../sendMsgPen.tmp");
            Console.WriteLine(penStatus);
            if (penStatus == "sleep"){
                await sendMsgPen();
                File.WriteAllText(@"../sendMsgPen.tmp", "working!");
            }
            string workTime = File.ReadAllText(@"../sendMsgWork.tmp");
            Console.WriteLine(workTime);
            if (workTime != "sendcmp"){
                await sendMsgWorked(workTime);
                File.WriteAllText(@"../sendMsgWork.tmp", "sendcmp");
            }
        }
    }
    private async Task sendMsgGrass()
    {
        var OTOMOserver = client.GetGuild(1206245420553076776);
        var sendChan = OTOMOserver.GetTextChannel(1206245421467570188);
        await sendChan.SendMessageAsync("眠そうですね...疲れていませんか?");
    }
    private async Task sendMsgPen()
    {
    //     var OTOMOserver = client.GetGuild(1206245420553076776);
    //     var sendChan = OTOMOserver.GetTextChannel(1206245421467570188);
    //     await sendChan.SendMessageAsync("勉強サボっていませんか？");
    }
    private async Task sendMsgWorked(string time)
    {
        var OTOMOserver = client.GetGuild(1206245420553076776);
        var sendChan = OTOMOserver.GetTextChannel(1206245421467570188);
        sendChan.SendMessageAsync(time + "秒働きました！！ ドリンクチケットGET!!");
    }
}
