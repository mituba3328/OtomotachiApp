namespace OtomotachiApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        /*private void Connect_Btn(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                ConnectBtn.Text = $"Clicked {count} time";
            else
                ConnectBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(ConnectBtn.Text);
        }*/

        private async void ConnectBtn_Clicked(object sender, EventArgs e)
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            var state = ble.State;
            List<IDevice> deviceList = new List<IDevice> {};
            adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);
            await adapter.StartScanningForDevicesAsync();

        }


    }

}
