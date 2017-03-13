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
            FindViewById<Button>(Resource.Id.button2).Click += button2_Click;
            FindViewById<ListView>(Resource.Id.listView1).ItemClick += MainActivity_ItemClick;
            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter = adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothDevice.ActionFound));

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            bs = new BluetoothService(this, ErrorCallback, DataCallback);

            bluetoothDevices = new List<BluetoothDevice>();

            SearchForDevices();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string text = FindViewById<EditText>(Resource.Id.editText1).Text;
            lock (bs.connectionsLock) {
                bs.connections.Where(bc => bc.IsConnected()).ToList().ForEach(bc => bc.Write(Encoding.ASCII.GetBytes(text)));
            }
        }

        void ErrorCallback(Exception ex, BluetoothDevice device)
        {
            RunOnUiThread( () => {
                Toast.MakeText(this, "Error from " + (device.Name == null ? device.Address : device.Name), ToastLength.Long).Show();
            });
        }

        void DataCallback(byte[] data, BluetoothDevice device)
        {
            RunOnUiThread(() => {
                Toast.MakeText(this, ("Data from " + device.Name == null ? device.Address : device.Name) + Encoding.ASCII.GetString(data), ToastLength.Long).Show();
            });
        }

        private void MainActivity_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            BluetoothDevice device = bluetoothDevices[(int)(e.Id - 1)];

            lock (bs.connectionsLock) {
                if (bs.connections.Any(_bc => _bc.bluetoothDevice.Address == device.Address)) {
                    Toast.MakeText(this, "Already connected to this device. Disconnecting", ToastLength.Long).Show();
                    bs.Disconnect(device);
                }
                else {
                    bs.Connect(device);

                }
            }
        }

        private bool cancelled = false;

        private void SearchForDevices()
        {
            if (bluetoothAdapter.IsDiscovering) {
                bluetoothAdapter.CancelDiscovery();
                cancelled = true;
            }
            
            if (bluetoothAdapter.StartDiscovery()) {
                adapter.Add("---- Bluetooth Devices ---- ");
                bluetoothDevices.Clear();

                bluetoothDevices.AddRange(bluetoothAdapter.BondedDevices);
                adapter.AddAll(bluetoothAdapter.BondedDevices.Select(bt => "Paired: " + ((bt.Name == null) ? "" : bt.Name) + " (" + bt.Address + ")").ToList());
            }
            else {
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
                bluetoothDevices.AddRange(devices);

                adapter.AddAll(devices.Select(bt => ((bt.Name == null) ? "" : bt.Name) + " (" + bt.Address + ")").ToList());
                adapter.NotifyDataSetChanged();
            }

            cancelled = false;
        }

        private ArrayAdapter<string> adapter;
        private List<BluetoothDevice> bluetoothDevices;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothService bs;
    }
}

