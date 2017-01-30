using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Bluetooth;
using System.Linq;
using System.Diagnostics;
using Android.Content;

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
            FindViewById<Button>(Resource.Id.buttonCollect).Click += ButtonCollect_Click;
            FindViewById<Button>(Resource.Id.buttonView).Click += ButtonView_Click;
            FindViewById<Button>(Resource.Id.button1).Click += button1_Click;
            cancelled = false;

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter= adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new Android.Content.IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(bluetoothReceiver, new Android.Content.IntentFilter(BluetoothDevice.ActionFound));

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
           
            ScanForDevices();
        }

        private bool cancelled;
        void ScanForDevices()
        {
            adapter.Clear();
            if (bluetoothAdapter.IsDiscovering)
            {
                bluetoothAdapter.CancelDiscovery();
                cancelled = true;
            }
            else
            {
                bluetoothAdapter.StartDiscovery();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ScanForDevices();
        }

        private void ButtonCollect_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(DataCollectionActivity)));
        }

        private void ButtonView_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(DataViewingActivity)));
        }

        private void DiscoveryFinishedCallback(List<string> devices)
        {
            if (!cancelled)
            {
                var paired = bluetoothAdapter.BondedDevices.Select(bt => "Paired: " + bt.Name).ToList();
                adapter.Add($"--- {paired.Count + devices.Count} Devices Found ---");
                adapter.AddAll(paired);
                adapter.AddAll(devices);
                adapter.NotifyDataSetChanged();
            }
            else
            {
                bluetoothAdapter.StartDiscovery();
                cancelled = false;
            }
            
        }

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

