using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Threading;
using Android.Bluetooth;
using Java.Util;

namespace ScoutingFRC
{
    class BluetoothCallbacks<T>
    {
        public Action<T, Exception> error;
        public Action<T, byte[]> dataReceived;
        public Action<T, int> dataSent;
        public Action<T> connected;
        public Action<T> disconnected;
    }

    class BluetoothService
    {
        private const string name = "BluetoothService";
        private static UUID uuid = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");

        private Context context;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothServerSocket serverSocket;

        private Thread listenThread;

        private bool stopping = false;

        public List<BluetoothConnection> connections;

        public readonly object connectionsLock = new object();

        private BluetoothCallbacks<BluetoothConnection> userCallbacks;
        private BluetoothCallbacks<BluetoothConnection> serviceCallbacks;

        public BluetoothService(Context context, BluetoothCallbacks<BluetoothConnection> callbacks, BluetoothAdapter bluetoothAdapter = null, bool startListening = true)
        {
            this.context = context ?? Application.Context;
            this.bluetoothAdapter = bluetoothAdapter ?? BluetoothAdapter.DefaultAdapter;

            userCallbacks = callbacks ?? new BluetoothCallbacks<BluetoothConnection>();
            serviceCallbacks = new BluetoothCallbacks<BluetoothConnection>
            {
                error = Error,
                dataReceived = DataReceived,
                dataSent = DataSent,
                connected = Connected,
                disconnected = Disconnected
            };


            connections = new List<BluetoothConnection>();

            listenThread = new Thread(Listen);

            if (startListening) {
                StartListening();
            }
        }

        /// <summary>
        /// Callback for when an error occurs in a bluetooth connection.
        /// </summary>
        private void Error(BluetoothConnection bluetoothConnection, Exception ex)
        {
            bluetoothConnection?.Disconnect();

            userCallbacks.error?.Invoke(bluetoothConnection, ex);
        }

        /// <summary>
        /// Callback for when data is received in a bluetooth connection.
        /// </summary>
        private void DataReceived(BluetoothConnection bluetoothConnection, byte[] data)
        {
            userCallbacks.dataReceived?.Invoke(bluetoothConnection, data);
        }

        /// <summary>
        /// Callback for when data is sent in a bluetooth connection.
        /// </summary>
        private void DataSent(BluetoothConnection bluetoothConnection, int id)
        {
            userCallbacks.dataSent?.Invoke(bluetoothConnection, id);
        }

        /// <summary>
        /// Callback for when a connection with another device has been established.
        /// </summary>
        private void Connected(BluetoothConnection bluetoothConnection)
        {
            lock(connectionsLock) {
                connections.Add(bluetoothConnection);
            }
            
            userCallbacks.connected?.Invoke(bluetoothConnection);
        }

        /// <summary>
        /// Callback for when we disconnected from a device.
        /// </summary>
        private void Disconnected(BluetoothConnection bluetoothConnection)
        {
            lock (connectionsLock) {
                connections.Remove(bluetoothConnection);
            }

            userCallbacks.disconnected?.Invoke(bluetoothConnection);
        }

        /// <summary>
        /// Shows a toast on the UI thread
        /// </summary>
        public void DebugString(string s)
        {
            (context as Activity).RunOnUiThread(() => Toast.MakeText(context, s, ToastLength.Long).Show());
        }

        /// <summary>
        /// Starts a connection with another device.
        /// </summary>
        public void Connect(BluetoothDevice device)
        {
            new BluetoothConnection(context, device, uuid, serviceCallbacks);
        }

        /// <summary>
        /// Disconnects from a device.
        /// </summary>
        public void Disconnect(BluetoothDevice device)
        {
            lock (connectionsLock) {
                BluetoothConnection bc = connections.Find(_bc => _bc.bluetoothDevice.Address == device.Address);
                bc?.Disconnect();
            }
        }

        /// <summary>
        /// Starts listening for incoming bluetooth connections.
        /// </summary>
        public void StartListening()
        {
            if(!IsListening()) {
                listenThread.Start();
            }
        }

        /// <summary>
        /// Returns whether the bluetooth service is listening for connections.
        /// </summary>
        public bool IsListening()
        {
            return listenThread.IsAlive;
        }

        /// <summary>
        /// Stops listening for bluetooth connections.
        /// </summary>
        public void StopListening()
        {
            if (IsListening()) {
                stopping = true;
                serverSocket.Close();
                listenThread.Join();
            }
        }


        /// <summary>
        /// Listens for incoming connections and starts a connection with them.
        /// </summary>
        public void Listen()
        {
            Looper.Prepare();

            serverSocket = bluetoothAdapter.ListenUsingRfcommWithServiceRecord(name, uuid);

            try {
                while (true) {
                    BluetoothSocket socket = serverSocket.Accept();
                    lock (connectionsLock) {
                        new BluetoothConnection(context, socket.RemoteDevice, uuid, serviceCallbacks, socket);
                    }
                }
            }
            catch (Exception ex) {
                if(!stopping) {
                    Error(null, ex);
                }
                stopping = false;
            }
        }
    }
}