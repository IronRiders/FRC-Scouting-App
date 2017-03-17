using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Bluetooth;
using System.Diagnostics;
using Android.Content.PM;

namespace ScoutingFRC
{
    [Activity(Label = "Sync Data", ScreenOrientation = ScreenOrientation.Portrait)]
    public class SyncDataActivity : Activity
    {
        List<MatchData> currentData;
        List<MatchData> newData;

        private BluetoothCallbacks<BluetoothConnection> callbacks;

        private ArrayAdapter<string> adapter;
        private List<BluetoothDevice> bluetoothDevices;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothService bs;

        private class BluetoothDataTransfer
        {
            public BluetoothDataTransfer(BluetoothConnection connection = null, BluetoothDevice device = null, bool weStarted = false)
            {
                this.device = device;
                this.connection = connection;
                this.weStarted = weStarted;
                this.done = false;
            }

            public BluetoothDevice device;
            public BluetoothConnection connection;
            public bool weStarted;
            public bool done;
        }

        private List<BluetoothDataTransfer> btDataTransfers;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SyncDevices);

            var bytes = Intent.GetByteArrayExtra("currentData");
            currentData = MatchData.Deserialize<List<MatchData>>(bytes);
            FindViewById<Button>(Resource.Id.buttonAdd).Click += ButtonAdd_Click;
            FindViewById<Button>(Resource.Id.buttonCancel).Click += ButtonCancel_Click;
            FindViewById<ListView>(Resource.Id.listViewDevices).ItemClick += SyncDataActivity_ItemClick;

            callbacks = new BluetoothCallbacks<BluetoothConnection>();
            callbacks.error = ErrorCallback;
            callbacks.dataReceived = DataCallback;
            callbacks.dataSent = DataSentCallback;
            callbacks.connected = ConnectedCallback;
            callbacks.disconnected = DisconnectedCallback;

            newData = new List<MatchData>();

            btDataTransfers = new List<BluetoothDataTransfer>();

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSelectableListItem);
            var listView = FindViewById<ListView>(Resource.Id.listViewDevices);
            listView.Adapter = adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothDevice.ActionFound));

            bluetoothDevices = new List<BluetoothDevice>();

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter != null) {
                if (!bluetoothAdapter.IsEnabled) {
                    bluetoothAdapter.Enable();

                    for (int i = 0; i < 100 && !bluetoothAdapter.IsEnabled; ++i) {
                        System.Threading.Thread.Sleep(10);
                    }

                    if(!bluetoothAdapter.IsEnabled) {
                        Toast.MakeText(this, "Bluetooth is disabled and can't be automatically enabled.", ToastLength.Long).Show();
                        return;
                    }
                }
                 
                if (bluetoothAdapter.ScanMode != ScanMode.ConnectableDiscoverable) {
                    Intent discoverableIntent = new Intent(BluetoothAdapter.ActionRequestDiscoverable);
                    discoverableIntent.PutExtra(BluetoothAdapter.ExtraDiscoverableDuration, 60);
                    StartActivity(discoverableIntent);
                    bluetoothAdapter.Enable();
                }

                bs = new BluetoothService(this, callbacks, bluetoothAdapter);
                SearchForDevices();
            }
            else {
                Toast.MakeText(this, "Bluetooth not supported on this device.", ToastLength.Long).Show();
            }
        }

        private void SyncDataActivity_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            BluetoothDevice device = bluetoothDevices[(int)(e.Id)];

            lock (bs.connectionsLock) {
                if (bs.connections.Any(_bc => _bc.bluetoothDevice.Address == device.Address)) {
                    Toast.MakeText(this, "Already connected to this device. Disconnecting", ToastLength.Long).Show();
                    bs.Disconnect(device);
                }
                else if (bs.connections.Count > 0) {
                    Toast.MakeText(this, "Already connected to a device.", ToastLength.Long).Show();
                }
                else {
                    btDataTransfers.Add(new BluetoothDataTransfer(null, device, true));
                    bs.Connect(device);  
                }
            }
        }

        private void DiscoveryFinishedCallback(List<BluetoothDevice> devices)
        {
            if (!cancelled) {
                bluetoothDevices.AddRange(devices);

                adapter.AddAll(devices.Select(bt => ((bt.Name == null) ? "" : bt.Name) + " (" + bt.Address + ")").ToList());
                adapter.NotifyDataSetChanged();
            }

            cancelled = false;
        }



        private bool cancelled = false;
        private void SearchForDevices()
        {
            if (bluetoothAdapter != null) {
                if (bluetoothAdapter.IsDiscovering) {
                    bluetoothAdapter.CancelDiscovery();
                    cancelled = true;
                }

                if (bluetoothAdapter.StartDiscovery()) {
                    bluetoothDevices.Clear();

                    bluetoothDevices.AddRange(bluetoothAdapter.BondedDevices);
                    adapter.AddAll(bluetoothAdapter.BondedDevices.Select(bt => "Paired: " + ((bt.Name == null) ? "" : bt.Name) + " (" + bt.Address + ")").ToList());
                }
                else {
                    Debugger.Break();
                }
            }
        }

        void ErrorCallback(BluetoothConnection bluetoothConnection, Exception ex)
        {
            RunOnUiThread(() => {
                var btd = btDataTransfers.FirstOrDefault(bt => bt.connection == bluetoothConnection);
                if (btd == null || !btd.done) {
                    Toast.MakeText(this, "Error from " + (bluetoothConnection.bluetoothDevice.Name == null ? bluetoothConnection.bluetoothDevice.Address : bluetoothConnection.bluetoothDevice.Name) + ": " + ex.Message, ToastLength.Long).Show();

                }
            });
        }

        void ChangeTextViews()
        {   
            FindViewById<TextView>(Resource.Id.textViewReceived).Text = "Matches Received: " + newData.Count;
            FindViewById<TextView>(Resource.Id.textViewSent).Text = "Matches Sent: " + currentData.Count;

            Toast.MakeText(this, "Done", ToastLength.Long).Show();
        }

        void DataCallback(BluetoothConnection bluetoothConnection, byte[] data)
        {
            RunOnUiThread(() => {
                List<MatchData> newMatchData = MatchData.Deserialize<List<MatchData>>(data);

                foreach (var md in newMatchData) {
                    var duplicate = currentData.Find(_md => _md.teamNumber == md.teamNumber && _md.match == md.match);
                    if (duplicate == null) {
                        newData.Add(md);
                    }
                }

                var btd = btDataTransfers.FirstOrDefault(bt => bt.connection == bluetoothConnection);

                if(btd != null) {
                    if (btd.weStarted) {
                        SendData(bluetoothConnection);
                    }
                    else {
                        ChangeTextViews();
                        bluetoothConnection.Disconnect();
                        btd.weStarted = false;
                        btd.done = true;
                    }
                }
            });
        }

        void DataSentCallback(BluetoothConnection bluetoothConnection, int id)
        {
            RunOnUiThread(() => {
                var btd = btDataTransfers.FirstOrDefault(bt => bt.connection == bluetoothConnection);
                if (btd != null && btd.weStarted) {
                    btd.done = true;
                }
            });
        }

        void SendData(BluetoothConnection bluetoothConnection)
        {
            var serialized = MatchData.Serialize(currentData);
            byte[] data = new byte[sizeof(int) + serialized.Length];
            BitConverter.GetBytes(serialized.Length).CopyTo(data, 0);
            serialized.CopyTo(data, sizeof(int));
            int id = 0;
            bluetoothConnection.Write(data, ref id);
        }

        void ConnectedCallback(BluetoothConnection bluetoothConnection)
        {
           RunOnUiThread(() => {
               newData.Clear();

                var btd = btDataTransfers.FirstOrDefault(bt => bt.device == bluetoothConnection.bluetoothDevice);
                if (btd == null) {
                    btd = new BluetoothDataTransfer(bluetoothConnection, bluetoothConnection.bluetoothDevice, false);
                    btDataTransfers.Add(btd);
                }
                else if (btd.connection == null) {
                    btd.connection = bluetoothConnection;
                }

                if(!btd.weStarted) {
                    SendData(bluetoothConnection);
                }
            });
        }

        void DisconnectedCallback(BluetoothConnection bluetoothConnection)
        {
            RunOnUiThread(() => {
                var btd = btDataTransfers.FirstOrDefault(bt => bt.connection == bluetoothConnection);
                if(btd != null) {
                    if (btd.weStarted && btd.done) {
                        ChangeTextViews();
                    }
                    else if(!btd.done){
                        Toast.MakeText(this, "Connection was interrupted", ToastLength.Long).Show();
                    }

                    btDataTransfers.Remove(btd);
                }

            });
        }

        protected override void OnDestroy()
        {
            bs.StopListening();
            base.OnDestroy();
        }

        private void ButtonCancel_Click(object sender, EventArgs eventArgs)
        {
            Intent myIntent = new Intent(this, typeof(MainActivity));
            SetResult(Result.Canceled, myIntent);
            Finish();
        }

        private void ButtonAdd_Click(object sender, EventArgs eventArgs)
        {
            Intent myIntent = new Intent(this, typeof(MainActivity));
            var bytes = MatchData.Serialize(newData);
            myIntent.PutExtra("newMatches", bytes);
            SetResult(Result.Ok, myIntent);
            Finish();
        }
    }
}