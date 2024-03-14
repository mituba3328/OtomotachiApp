using Microsoft.Extensions.Logging;
using OtomotachiApp.Pages.Views;
using OtomotachiApp.Services;

namespace OtomotachiApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.UseMauiApp<App>().UseMauiCommunityToolkit();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<BluetoothLEService>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<DevicePageViewModel>();
            builder.Services.AddSingleton<DevicePage>();
            builder.Services.AddSingleton<OtomoDevicePage>();
            builder.Services.AddSingleton<OtomoDevicePageViewModel>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
