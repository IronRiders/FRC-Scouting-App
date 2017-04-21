using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.Threading;
using Android.Bluetooth;
using Java.Util;

namespace ScoutingFRC
{
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

        /// <summary>
        /// Starts the connectthread unless the service is connected or disconnecting.
        /// </summary>
        private void Connect()
        {
            if (!IsConnected() && !IsDisconnecting()) {
                connectThread.Start();
            }
        }

        /// <summary>
        /// Thread that attempts to connect with the bluetooth device.
        /// On success it starts the connection thread.
        /// </summary>
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

        /// <summary>
        /// Returns whether we are connected to a bluetooth device.
        /// </summary>
        public bool IsConnected()
        {
            return connectionThread.IsAlive;
        }

        /// <summary>
        /// Returns whether we are disconnecting.
        /// </summary>
        public bool IsDisconnecting()
        {
            return disconnectThread.IsAlive;
        }

        /// <summary>
        /// Disconnects from the bluetooth device.
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected() && !IsDisconnecting()) {
                disconnectThread.Start();
            }
        }

        /// <summary>
        /// Thread that disconnects from the device.
        /// Waits until other threads are finished.
        /// </summary>
        private void DisconnectInternal()
        {
            //Don't throw any errors when disconnecting
            var copy = callbacks.error;
            callbacks.error = null;

            bool connected = IsConnected();

            //closing the socket will cause both threads to fail
            bluetoothSocket?.Close();
            bluetoothSocket = null;

            foreach (Thread writeThread in writeThreads) {
                if (writeThread.IsAlive) {
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

            if (!connected) {
                callbacks.disconnected?.Invoke(this);
            }

            callbacks.error = copy;
        }

        /// <summary>
        /// Thread that runs during a connection, waiting to receive data.
        /// When data is received the dataCallback is invoked.
        /// </summary>
        private void Connection()
        {
            Looper.Prepare();

            byte[] buffer = new byte[1024 * 1024];
            byte[] result = new byte[1024 * 1024];

            int totalBytes = -1;
            int bytesLeft = -1;

            while (true) {
                int startIndex = 0;
                try {
                    int bytes = bluetoothSocket.InputStream.Read(buffer, 0, buffer.Length);
                    if (totalBytes < 0) {
                        if (bytes > sizeof(int)) {
                            bytesLeft = totalBytes = BitConverter.ToInt32(buffer, 0);
                            Array.Copy(buffer, sizeof(int), result, 0, bytes - sizeof(int));
                            bytesLeft -= bytes - sizeof(int);
                            startIndex = 4;
                        }
                        else {
                            callbacks.error?.Invoke(this, new Exception("Initial data chunk too small."));
                            totalBytes = -1;
                            bytesLeft = -1;
                            continue;
                        }
                    }
                    else {
                        bytesLeft -= bytes;
                        if (bytesLeft < 0) {
                            callbacks.error?.Invoke(this, new Exception("Data chunk too big."));
                            totalBytes = -1;
                            bytesLeft = -1;
                            continue;
                        }
                    }

                    Array.Copy(buffer, startIndex, result, totalBytes - (bytes - startIndex) - bytesLeft, bytes - startIndex);

                    if (bytesLeft == 0) {
                        callbacks.dataReceived?.Invoke(this, result.Take(totalBytes).ToArray());
                        totalBytes = -1;
                        bytesLeft = -1;
                    }
                }
                catch (Exception ex) {
                    callbacks.error?.Invoke(this, ex);
                    callbacks.disconnected?.Invoke(this);
                    return;
                }
            }
        }

        /// <summary>
        /// Sends bytes to the other bluetooth device.
        /// </summary>
        public bool Write(byte[] data, ref int id)
        {
            if (IsConnected() && !IsDisconnecting()) {
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
}