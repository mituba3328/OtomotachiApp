namespace OtomotachiApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(DevicePage), typeof(DevicePage));
            Routing.RegisterRoute(nameof(OtomoDevicePage), typeof(OtomoDevicePage));
        }
    }
}
