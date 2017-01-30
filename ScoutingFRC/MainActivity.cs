using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Bluetooth;
using System.Linq;
using System.Diagnostics;

namespace ScoutingFRC
{
    [Activity(Label = "ScoutingFRC", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter= adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new Android.Content.IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(bluetoothReceiver, new Android.Content.IntentFilter(BluetoothDevice.ActionFound));

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if(!bluetoothAdapter.StartDiscovery()) {
                Debugger.Break();
            }

            List<string> bondedDevices = bluetoothAdapter.BondedDevices.Select(bt => "Bonded: " + bt.Name).ToList();
            adapter.AddAll(bondedDevices);
        }

        private void DiscoveryFinishedCallback(List<string> devices)
        {
            adapter.AddAll(devices);
            adapter.NotifyDataSetChanged();
        }

        private BluetoothService btService;

        public const int MESSAGE_STATE_CHANGE = 1;
        public const int MESSAGE_READ = 2;
        public const int MESSAGE_WRITE = 3;
        public const int MESSAGE_DEVICE_NAME = 4;
        public const int MESSAGE_TOAST = 5;

        public const string DEVICE_NAME = "device_name";
        public const string TOAST = "toast";

        private ArrayAdapter<string> adapter;
        private BluetoothAdapter bluetoothAdapter;
    }
}

