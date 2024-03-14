using System.Net.WebSockets;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.SignalR.Client;

namespace OtomotachiApp.Models;

[INotifyPropertyChanged]
public partial class Pen : BaseModel
{
    [ObservableProperty]
    bool cheerLed = false;
    [ObservableProperty]
    bool isUsing = false;

    private HubConnection hubConnection;
    public async Task StatSignalR()
    {

        hubConnection = new HubConnectionBuilder()
            .WithUrl("http://192.168.10.102:32820/chatHub")
            .Build();

        /*hubConnection.Closed += async (error) =>
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await hubConnection.StartAsync();
        };*/

        await ConnectAsync();
    }
    private async Task ConnectAsync()
    {
        //���󂯎����API���烆�[�U�[���ƃ��b�Z�[�W�𔽉f���遫ChatHub�ō쐬����API�ƈꏏ�̖��O
        hubConnection.On<string>("ReceiveMessage", ( message) =>
        {
            //�󂯎�������b�Z�[�W�ɉ��s���������������̂��ǂ�ǂ񃉃x���ɒǉ����Ă���
           // Debug.WriteLine ($"{message}");
            if (message == "onLed")
            {
                cheerLed = true;
            }
        });
        try
        {
            await hubConnection.StartAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: {0}", e);
        }
        
    }

    public async void Send(string msg)
    {
        try
        {
            await hubConnection.InvokeAsync("SendMessage", msg);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception: {e}");
        }
    }
}