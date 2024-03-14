namespace OtomotachiApp.Pages
{

	public partial class WorkTimePage : ContentPage
	{
		public WorkTimePage()
		{
			InitializeComponent();
		}
        private async void BLEScanBtn_Clicked(object sender, EventArgs e)
        {
            var ble = CrossBluetoothLE.Current;
            var adapter = CrossBluetoothLE.Current.Adapter;
            var state = ble.State;
            List<IDevice> deviceList = new List<IDevice> { };
            adapter.DeviceDiscovered += (s, a) => deviceList.Add(a.Device);
            await adapter.StartScanningForDevicesAsync();

        }
    }
}