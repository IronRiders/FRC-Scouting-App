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

namespace ScoutingFRC
{
    [Activity(Label = "Sync Data")]
    public class SyncDataActivity : Activity
    {
        List<MatchData> currentData = new List<MatchData>();
        List<MatchData> newData = new List<MatchData>();

        private BluetoothCallbacks<BluetoothConnection> callbacks;

        private ArrayAdapter<string> adapter;
        private List<BluetoothDevice> bluetoothDevices;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothService bs;

        private class BluetoothDataTransfer
        {
            public BluetoothDataTransfer(BluetoothConnection connection = null, int id = -1, bool received = false, bool sent = false)
            {
                this.connection = connection;
                this.id = id;
                this.received = received;
                this.sent = sent;
            }

            public bool Done()
            {
                return received && sent;
            }

            public BluetoothConnection connection;
            public int id;
            public bool received;
            public bool sent;
        }

        private List<BluetoothDataTransfer> btDataTransfers;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SyncDevices);
            
            var bytes = Intent.GetByteArrayExtra("currentData");
            currentData = MatchData.Deserialize<List<MatchData>>(bytes);
            FindViewById<Button>(Resource.Id.buttonExchange).Click += ButtonExchange_Click;
            FindViewById<Button>(Resource.Id.buttonAdd).Click += ButtonAdd_Click;
            FindViewById<Button>(Resource.Id.buttonCancel).Click += ButtonCancel_Click;
            FindViewById<ListView>(Resource.Id.listViewDevices).ItemClick += SyncDataActivity_ItemClick;

            callbacks = new BluetoothCallbacks<BluetoothConnection>();
            callbacks.error = ErrorCallback;
            callbacks.dataReceived = DataCallback;
            callbacks.dataSent = DataSentCallback;
            callbacks.connected = ConnectedCallback;
            callbacks.disconnected = DisconnectedCallback;

            btDataTransfers = new List<BluetoothDataTransfer>();

            adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1);
            var listView = FindViewById<ListView>(Resource.Id.listViewDevices);
            listView.Adapter = adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));
            RegisterReceiver(bluetoothReceiver, new IntentFilter(BluetoothDevice.ActionFound));

            bluetoothDevices = new List<BluetoothDevice>();

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter != null && bluetoothAdapter.IsEnabled) {
                bs = new BluetoothService(this, callbacks, bluetoothAdapter);
                SearchForDevices();
            }
            else {
                Toast.MakeText(this, "Bluetooth is disabled", ToastLength.Long).Show();
            }
        }

        bool weStarted = false;
        private void SyncDataActivity_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            done = false;

            BluetoothDevice device = bluetoothDevices[(int)(e.Id)];

            lock (bs.connectionsLock) {
                if (bs.connections.Any(_bc => _bc.bluetoothDevice.Address == device.Address)) {
                    Toast.MakeText(this, "Already connected to this device. Disconnecting", ToastLength.Long).Show();
                    bs.Disconnect(device);
                }
                else {
                    weStarted = true;
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
                if(!done) {
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

        bool done = false;

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

                if (weStarted) {
                    SendData(bluetoothConnection);
                }
                else {
                    ChangeTextViews();
                    bluetoothConnection.Disconnect();
                    weStarted = false;
                    done = true;
                }
            });
        }

        void DataSentCallback(BluetoothConnection bluetoothConnection, int id)
        {
            RunOnUiThread(() => {
                if(weStarted) {
                    done = true;
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
                if(!weStarted) {
                    SendData(bluetoothConnection);
                }
            });
        }

        void DisconnectedCallback(BluetoothConnection bluetoothConnection)
        {
            RunOnUiThread(() => {
                if (weStarted && done) {
                    ChangeTextViews();
                    weStarted = false;
                }
            });
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

        private void ButtonExchange_Click(object sender, EventArgs eventArgs)
        {
            
        }

    }
}