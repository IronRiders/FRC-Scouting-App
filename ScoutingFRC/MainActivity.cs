using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Bluetooth;
using System.Linq;
using System.Diagnostics;
using Android.Content;
using System.Text;

namespace ScoutingFRC
{
    [Activity(Label = "ScoutingFRC", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViewById<Button>(Resource.Id.buttonCollect).Click += ButtonCollect_Click;
            FindViewById<Button>(Resource.Id.buttonView).Click += ButtonView_Click;
            FindViewById<Button>(Resource.Id.button1).Click += button1_Click;
            FindViewById<ListView>(Resource.Id.listView1).ItemClick += MainActivity_ItemClick;
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter = adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothDevice.ActionFound));

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            bs = new BluetoothService(this);
            bluetoothDevices = new List<BluetoothDevice>();

            SearchForDevices();
        }

        long connectedId = -1;
        private void MainActivity_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if(connectedId < 0) {
                connectedId = e.Id;
                bs.Connect(bluetoothDevices[(int)(e.Id - 1)]);
            }
            else {
                bs.Write(Encoding.ASCII.GetBytes("Test Message"));
            } 
        }

        private bool cancelled = false;

        private void SearchForDevices()
        {
            if (bluetoothAdapter.IsDiscovering) {
                bluetoothAdapter.CancelDiscovery();
                cancelled = true;
            }
            
            if (!bluetoothAdapter.StartDiscovery()) {
                Debugger.Break();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            adapter.Clear();
            adapter.NotifyDataSetChanged();
            SearchForDevices();
        }

        private void ButtonCollect_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(DataCollectionActivity)));
        }

        private void ButtonView_Click(object sender, EventArgs e)
        {
            StartActivity(new Intent(Application.Context, typeof(DataViewingActivity)));
        }

        private void DiscoveryFinishedCallback(List<BluetoothDevice> devices)
        {
            if(!cancelled) {
                adapter.Add("---- Bluetooth Devices ---- ");
                bluetoothDevices.Clear();

                bluetoothDevices.AddRange(bluetoothAdapter.BondedDevices);
                bluetoothDevices.AddRange(devices);

                adapter.AddAll(bluetoothAdapter.BondedDevices.Select(bt => "Paired: " + bt.Name).ToList());
                adapter.AddAll(devices.Select(d => d.Name).ToList());
                adapter.NotifyDataSetChanged();
            }

            cancelled = false;
        }

        public const int MESSAGE_STATE_CHANGE = 1;
        public const int MESSAGE_READ = 2;
        public const int MESSAGE_WRITE = 3;
        public const int MESSAGE_DEVICE_NAME = 4;
        public const int MESSAGE_TOAST = 5;

        public const string DEVICE_NAME = "device_name";
        public const string TOAST = "toast";

        private ArrayAdapter<string> adapter;
        private List<BluetoothDevice> bluetoothDevices;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothService bs;
    }
}

