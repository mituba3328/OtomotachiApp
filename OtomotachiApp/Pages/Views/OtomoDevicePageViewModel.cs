using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OtomotachiApp.Pages.Views
{
    public partial class OtomoDevicePageViewModel:BaseViewModel
    {
        public BluetoothLEService BluetoothLEService { get; private set; }

        public IAsyncRelayCommand ConnectToDeviceCandidateAsyncCommand { get; }
        public IAsyncRelayCommand DisconnectFromDeviceAsyncCommand { get; }

        public Pen ConnecedDevice {  get; private set; }

        public IService OtomoDeviceService { get; private set; }
        public ICharacteristic OtomoPenIsUsingCharacteristic { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public OtomoDevicePageViewModel(BluetoothLEService bluetoothLEService)
        {
            Title = $"OTOMOデバイス";

            BluetoothLEService = bluetoothLEService;

            ConnectToDeviceCandidateAsyncCommand = new AsyncRelayCommand(ConnectToDeviceCandidateAsync);

            DisconnectFromDeviceAsyncCommand = new AsyncRelayCommand(DisconnectFromDeviceAsync);
        }
        [ObservableProperty]
        string isUsingText;

        [ObservableProperty]
        ushort isUsingValue;

        [ObservableProperty]
        DateTimeOffset timestamp;

        private async Task ConnectToDeviceCandidateAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (BluetoothLEService.NewDeviceCandidateFromHomePage.Id.Equals(Guid.Empty))
            {
                #region read device id from storage
                var device_name = await SecureStorage.Default.GetAsync("device_name");
                var device_id = await SecureStorage.Default.GetAsync("device_id");
                if (!string.IsNullOrEmpty(device_id))
                {
                    BluetoothLEService.NewDeviceCandidateFromHomePage.Name = device_name;
                    BluetoothLEService.NewDeviceCandidateFromHomePage.Id = Guid.Parse(device_id);
                }
                #endregion read device id from storage
                else
                {
                    await BluetoothLEService.ShowToastAsync($"Select a Bluetooth LE device first. Try again.");
                    return;
                }
            }

            if (!BluetoothLEService.BluetoothLE.IsOn)
            {
                await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                return;
            }

            if (BluetoothLEService.Adapter.IsScanning)
            {
                await BluetoothLEService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
                return;
            }

            try
            {
                IsBusy = true;

                if (BluetoothLEService.Device != null)
                {
                    if (BluetoothLEService.Device.State == DeviceState.Connected)
                    {
                        if (BluetoothLEService.Device.Id.Equals(BluetoothLEService.NewDeviceCandidateFromHomePage.Id))
                        {
                            await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} is already connected.");
                            return;
                        }

                        if (BluetoothLEService.NewDeviceCandidateFromHomePage != null)
                        {
                            #region another device
                            if (!BluetoothLEService.Device.Id.Equals(BluetoothLEService.NewDeviceCandidateFromHomePage.Id))
                            {
                                Title = $"{BluetoothLEService.NewDeviceCandidateFromHomePage.Name}";
                                await DisconnectFromDeviceAsync();
                                await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} has been disconnected.");
                            }
                            #endregion another device
                        }
                    }
                }

                BluetoothLEService.Device = await BluetoothLEService.Adapter.ConnectToKnownDeviceAsync(BluetoothLEService.NewDeviceCandidateFromHomePage.Id);

                if (BluetoothLEService.Device.State == DeviceState.Connected)
                {
                    OtomoDeviceService = await BluetoothLEService.Device.GetServiceAsync(OtomoDeviceUuids.OtomoPenServiceUuid);
                    if (OtomoDeviceService != null)
                    {
                        OtomoPenIsUsingCharacteristic = await OtomoDeviceService.GetCharacteristicAsync(OtomoDeviceUuids.OtomoPenIsUsingUuid);
                        if (OtomoPenIsUsingCharacteristic != null)
                        {
                            if (OtomoPenIsUsingCharacteristic.CanUpdate)
                            {
                                Title = $"{BluetoothLEService.Device.Name}";

                                #region save device id to storage
                                await SecureStorage.Default.SetAsync("device_name", $"{BluetoothLEService.Device.Name}");
                                await SecureStorage.Default.SetAsync("device_id", $"{BluetoothLEService.Device.Id}");
                                #endregion save device id to storage

                                ConnecedDevice = new Pen() {
                                    _name = BluetoothLEService.Device.Name
                                };
                                await ConnecedDevice.StatSignalR();
                                OtomoPenIsUsingCharacteristic.ValueUpdated += OtomoPenIsUsingCharacteristic_ValueUpdated;
                                await OtomoPenIsUsingCharacteristic.StartUpdatesAsync();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to connect to {BluetoothLEService.NewDeviceCandidateFromHomePage.Name} {BluetoothLEService.NewDeviceCandidateFromHomePage.Id}: {ex.Message}.");
                await Shell.Current.DisplayAlert($"{BluetoothLEService.NewDeviceCandidateFromHomePage.Name}", $"Unable to connect to {BluetoothLEService.NewDeviceCandidateFromHomePage.Name}.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OtomoPenIsUsingCharacteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            // var bytes = await characteristic.ReadAsync();
            var bytes = e.Characteristic.Value;
            
            IsUsingValue = bytes[0];

            if(IsUsingValue == 1) 
            {
                IsUsingText = "Pen Is Using !!";
                ConnecedDevice.Send("pen0:Using");
            }
            else
            {
                IsUsingText = "Pen Is not Using!!";
                ConnecedDevice.Send("pen0:NotUsing");
            }
            Timestamp = DateTimeOffset.Now.LocalDateTime;
            if (ConnecedDevice.CheerLed)
            {
                await OnCheerLED();
            }
            /* 
            var bytes = e.Characteristic.Value;
            const byte heartRateValueFormat = 0x01;

            byte flags = bytes[0];
            bool isHeartRateValueSizeLong = (flags & heartRateValueFormat) != 0;
            HeartRateValue = isHeartRateValueSizeLong ? BitConverter.ToUInt16(bytes, 1) : bytes[1];
            Timestamp = DateTimeOffset.Now.LocalDateTime;
             */
        }

        private async Task DisconnectFromDeviceAsync()
        {
            if (IsBusy)
            {
                return;
            }

            if (BluetoothLEService.Device == null)
            {
                await BluetoothLEService.ShowToastAsync($"Nothing to do.");
                return;
            }

            if (!BluetoothLEService.BluetoothLE.IsOn)
            {
                await Shell.Current.DisplayAlert($"Bluetooth is not on", $"Please turn Bluetooth on and try again.", "OK");
                return;
            }

            if (BluetoothLEService.Adapter.IsScanning)
            {
                await BluetoothLEService.ShowToastAsync($"Bluetooth adapter is scanning. Try again.");
                return;
            }

            if (BluetoothLEService.Device.State == DeviceState.Disconnected)
            {
                await BluetoothLEService.ShowToastAsync($"{BluetoothLEService.Device.Name} is already disconnected.");
                return;
            }

            try
            {
                IsBusy = true;

                await OtomoPenIsUsingCharacteristic.StopUpdatesAsync();

                await BluetoothLEService.Adapter.DisconnectDeviceAsync(BluetoothLEService.Device);

                OtomoPenIsUsingCharacteristic.ValueUpdated -= OtomoPenIsUsingCharacteristic_ValueUpdated;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to disconnect from {BluetoothLEService.Device.Name} {BluetoothLEService.Device.Id}: {ex.Message}.");
                await Shell.Current.DisplayAlert($"{BluetoothLEService.Device.Name}", $"Unable to disconnect from {BluetoothLEService.Device.Name}.", "OK");
            }
            finally
            {
                Title = "OtomoPen";
                IsUsingText = "未取得";
                Timestamp = DateTimeOffset.MinValue;
                IsBusy = false;
                BluetoothLEService.Device?.Dispose();
                BluetoothLEService.Device = null;
                await Shell.Current.GoToAsync("//DevicePage", true);
            }
        }
        private async Task OnCheerLED()
        {
            try 
            {
                var characteristic = await OtomoDeviceService.GetCharacteristicAsync(OtomoDeviceUuids.OtomoPencheerLEDUuid);
                byte[] bytes = [0x01];
                await characteristic.WriteAsync(bytes);
                ConnecedDevice.CheerLed = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to disconnect from {BluetoothLEService.Device.Name} {BluetoothLEService.Device.Id}: {ex.Message}.");
                await Shell.Current.DisplayAlert($"{BluetoothLEService.Device.Name}", $"Unable to disconnect from {BluetoothLEService.Device.Name}.", "OK");
                ConnecedDevice.CheerLed = false;
            }
        }
    }
}
