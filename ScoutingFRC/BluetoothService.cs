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
using System.Threading;
using Android.Bluetooth;
using Java.Util;
using System.Diagnostics;

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

    class BluetoothConnection
    {
        static int dataId;

        private Thread connectThread;
        private Thread connectionThread;

        private List<Thread> writeThreads;

        private Thread disconnectThread;

        public BluetoothDevice bluetoothDevice;
        private BluetoothSocket bluetoothSocket;

        private BluetoothCallbacks<BluetoothConnection> callbacks;

        private UUID uuid;

        Context context;

        public BluetoothConnection(Context context, BluetoothDevice bluetoothDevice, UUID uuid, BluetoothCallbacks<BluetoothConnection> callbacks, BluetoothSocket bluetoothSocket = null)
        {
            this.bluetoothDevice = bluetoothDevice;

            this.uuid = uuid;

            this.context = context;

            this.callbacks = callbacks ?? new BluetoothCallbacks<BluetoothConnection>();

            connectThread = new Thread(ConnectInternal);
            connectionThread = new Thread(Connection);

            writeThreads = new List<Thread>();

            disconnectThread = new Thread(DisconnectInternal);

            dataId = 0;

            if (bluetoothSocket != null && bluetoothSocket.IsConnected) {
                this.bluetoothSocket = bluetoothSocket;
                connectionThread.Start();
                callbacks.connected?.Invoke(this);
            }
            else {
                this.bluetoothSocket = null;
                connectThread.Start();
            }
        }

        private void Connect()
        {
            if (!IsConnected() && !IsDisconnecting()) {
                connectThread.Start();
            }
        }

        private void ConnectInternal()
        {
            Looper.Prepare();

            bluetoothSocket = bluetoothDevice.CreateRfcommSocketToServiceRecord(uuid);

            try {
                bluetoothSocket.Connect();
                connectionThread.Start();

                callbacks.connected?.Invoke(this);
            }
            catch (Exception ex) {
                callbacks.error?.Invoke(this, ex);
            }
        }

        public bool IsConnected()
        {
            return connectionThread.IsAlive;
        }

        public bool IsDisconnecting()
        {
            return disconnectThread.IsAlive;
        }

        public void Disconnect()
        {
            if (IsConnected()) {
                disconnectThread.Start();
            }
        }

        private void DisconnectInternal()
        {
            //Don't throw any errors when disconnecting
            var copy = callbacks.error;
            callbacks.error = null;

            //closing the socket will cause both threads to fail
            bluetoothSocket?.Close();
            bluetoothSocket = null;

            foreach(Thread writeThread in writeThreads) {
                if(writeThread.IsAlive) {
                    writeThread.Join();
                }
            }

            writeThreads.Clear();

            if (connectThread.IsAlive) {
                connectThread.Join();
            }
            if (connectionThread.IsAlive) {
                connectionThread.Join();
            }

            callbacks.error = copy;

            callbacks.disconnected?.Invoke(this);
        }

        private void Connection()
        {
            Looper.Prepare();

            byte[] buffer = new byte[4096];
            int bytes;

            while (true) {
                try {
                    bytes = bluetoothSocket.InputStream.Read(buffer, 0, buffer.Length);
                    callbacks.dataReceived?.Invoke(this, buffer.Take(bytes).ToArray());
                }
                catch (Exception ex) {
                    callbacks.error?.Invoke(this, ex);
                    return;
                }
            }
        }

        public bool Write(byte[] data, ref int id)
        {
            if(IsConnected() && !IsDisconnecting()) {
                int _id = dataId++;
                Thread thread = new Thread((() =>
                {
                    if (bluetoothSocket != null && bluetoothSocket.IsConnected) {
                        try {
                            bluetoothSocket.OutputStream.Write(data, 0, data.Length);
                            callbacks.dataSent?.Invoke(this, _id);
                        }
                        catch (Exception ex) {
                            callbacks.error?.Invoke(this, ex);
                        }
                    }

                    writeThreads.Remove(Thread.CurrentThread);
                }));

                id = _id;

                writeThreads.Add(thread);
                thread.Start();

                return true;
            }

            return false;
        }

        public void Debug(string s)
        {
            (context as Activity).RunOnUiThread(() => Toast.MakeText(Application.Context, s, ToastLength.Long).Show());
        }
    }

    class BluetoothService
    {
        private const string name = "BluetoothService";
        private static UUID uuid = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");

        private Context context;
        private BluetoothAdapter bluetoothAdapter;
        private BluetoothServerSocket serverSocket;

        private bool listen = false;

        private Thread listenThread;

        public List<BluetoothConnection> connections;

        public readonly object connectionsLock = new object();

        private BluetoothCallbacks<BluetoothConnection> userCallbacks;
        private BluetoothCallbacks<BluetoothConnection> serviceCallbacks;

        public BluetoothService(Context context, BluetoothCallbacks<BluetoothConnection> callbacks, BluetoothAdapter bluetoothAdapter = null, bool startListening = true)
        {
            this.context = context ?? Application.Context;
            this.bluetoothAdapter = bluetoothAdapter ?? BluetoothAdapter.DefaultAdapter;

            userCallbacks = callbacks ?? new BluetoothCallbacks<BluetoothConnection>();
            serviceCallbacks = new BluetoothCallbacks<BluetoothConnection>();

            serviceCallbacks.error = Error;
            serviceCallbacks.dataReceived = DataReceived;
            serviceCallbacks.dataSent = DataSent;
            serviceCallbacks.connected = Connected;
            serviceCallbacks.disconnected = Disconnected;

            connections = new List<BluetoothConnection>();

            listenThread = new Thread(Listen);

            if (startListening) {
                StartListening();
            }
        }

        private void Error(BluetoothConnection bluetoothConnection, Exception ex)
        {
            if (bluetoothConnection != null) {
                bluetoothConnection.Disconnect();
            }

            userCallbacks.error?.Invoke(bluetoothConnection, ex);
        }

        private void DataReceived(BluetoothConnection bluetoothConnection, byte[] data)
        {
            userCallbacks.dataReceived?.Invoke(bluetoothConnection, data);
        }

        private void DataSent(BluetoothConnection bluetoothConnection, int id)
        {
            userCallbacks.dataSent?.Invoke(bluetoothConnection, id);
        }

        private void Connected(BluetoothConnection bluetoothConnection)
        {
            connections.Add(bluetoothConnection);
            userCallbacks.connected?.Invoke(bluetoothConnection);
        }

        private void Disconnected(BluetoothConnection bluetoothConnection)
        {
            connections.Remove(bluetoothConnection);
            userCallbacks.disconnected?.Invoke(bluetoothConnection);
        }

        public void Debug(string s)
        {
            (context as Activity).RunOnUiThread(() => Toast.MakeText(context, s, ToastLength.Long).Show());
        }

        public void Connect(BluetoothDevice device)
        {
            lock (connectionsLock) {
                connections.Add(new BluetoothConnection(context, device, uuid, serviceCallbacks));
            }
        }

        public void Disconnect(BluetoothDevice device)
        {
            BluetoothConnection bc = connections.Find(_bc => _bc.bluetoothDevice.Address == device.Address);
            if (bc != null) {
                bc.Disconnect();
                connections.Remove(bc);
            }
        }

        public void StartListening()
        {
            if(!IsListening()) {
                listen = true;
                listenThread.Start();
            }
        }

        public bool IsListening()
        {
            return listenThread.IsAlive;
        }

        public void StopListening()
        {
            if (IsListening()) {
                listen = false;
                listenThread.Join();
            }
        }

        public void Listen()
        {
            Looper.Prepare();

            serverSocket = bluetoothAdapter.ListenUsingRfcommWithServiceRecord(name, uuid);

            bool attemptedClose = false;

            try {
                while (listen) {
                    BluetoothSocket socket = serverSocket.Accept();
                    lock (connectionsLock) {
                        new BluetoothConnection(context, socket.RemoteDevice, uuid, serviceCallbacks, socket);
                    }
                }

                attemptedClose = true;
                serverSocket.Close();
            }
            catch (Exception ex) {
                Error(null, ex);

                if (!attemptedClose) {
                    try { serverSocket.Close(); } catch { Error(null, ex); }
                }
            }

            listen = false;
        }
    }
}