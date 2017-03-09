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
    class BluetoothConnection
    {
        private Thread connectThread;
        private Thread connectionThread;

        public BluetoothDevice bluetoothDevice;
        private BluetoothSocket bluetoothSocket;

        private Action<Exception, BluetoothDevice> errorCallback;
        private Action<byte[], BluetoothDevice> dataCallback;

        private UUID uuid;

        public BluetoothConnection(BluetoothDevice bluetoothDevice, UUID uuid, Action<Exception, BluetoothDevice> errorCallback, Action<byte[], BluetoothDevice> dataCallback, BluetoothSocket bluetoothSocket = null)
        {
            this.bluetoothDevice = bluetoothDevice;

            this.uuid = uuid;

            this.errorCallback = errorCallback;
            this.dataCallback = dataCallback;

            connectThread = new Thread(ConnectInternal);
            connectionThread = new Thread(Connection);

            if(bluetoothSocket != null && bluetoothSocket.IsConnected) {
                this.bluetoothSocket = bluetoothSocket;
                connectionThread.Start();
            }
            else {
                this.bluetoothSocket = null;
                connectThread.Start();
            }
        }

        private bool Connect()
        {
            bool connected = IsConnected();

            if(connected) {
                connectThread.Start();
            }

            return connected;
        }

        public bool IsConnected()
        {
            return connectionThread.IsAlive;
        }

        public void ConnectInternal()
        {
            Looper.Prepare();

            bluetoothSocket = bluetoothDevice.CreateRfcommSocketToServiceRecord(uuid);

            try {
                bluetoothSocket.Connect();
                connectionThread.Start();
                Debug("Connected to " + bluetoothDevice.Name == null ? bluetoothDevice.Address : bluetoothDevice.Name);
            }
            catch (Exception ex) {
                errorCallback?.Invoke(ex, bluetoothDevice);
            }
        }

        public void Disconnect()
        {
            var copy = errorCallback;
            errorCallback = null;
            bluetoothSocket?.Close();
            bluetoothSocket = null;
            if(connectThread.IsAlive) {
                connectThread.Join();
            }
            if(connectionThread.IsAlive) {
                connectionThread.Join();
            }
            errorCallback = copy;
        }

        private void Connection()
        {
            Looper.Prepare();

            byte[] buffer = new byte[1024];
            int bytes;

            while (true) {
                try {
                    bytes = bluetoothSocket.InputStream.Read(buffer, 0, buffer.Length);
                    dataCallback(buffer.Take(bytes).ToArray(), bluetoothDevice);
                }
                catch (Exception ex) {
                    errorCallback?.Invoke(ex, bluetoothDevice);
                    return;
                }
            }
        }

        public void Write(byte[] data)
        {
            if(bluetoothSocket != null && bluetoothSocket.IsConnected) {
                try {
                    bluetoothSocket.OutputStream.Write(data, 0, data.Length);
                }
                catch(Exception ex) {
                    errorCallback?.Invoke(ex, bluetoothDevice);
                }       
            }
        }

        public void Debug(string s)
        {
            ((Activity)Application.Context).RunOnUiThread(() => Toast.MakeText(Application.Context, s, ToastLength.Long).Show());
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

        private Action<Exception, BluetoothDevice> errorCallback;
        private Action<byte[], BluetoothDevice> dataCallback;

        private Thread listenThread;

        public List<BluetoothConnection> connections;

        public readonly object connectionsLock = new object();

        public BluetoothService(Context context, Action<Exception, BluetoothDevice> errorCallback, Action<byte[], BluetoothDevice> dataCallback, BluetoothAdapter bluetoothAdapter = null, bool startListening = true)
        {
            this.context = context ?? Application.Context;
            this.bluetoothAdapter = bluetoothAdapter ?? BluetoothAdapter.DefaultAdapter;
            this.errorCallback = errorCallback;
            this.dataCallback = dataCallback;
            connections = new List<BluetoothConnection>();

            listenThread = new Thread(Listen);

            if(startListening) {
                StartListening();
            }
        }

        private void ErrorCallback(Exception ex, BluetoothDevice device)
        {
            connections.First(bc => bc.bluetoothDevice.Address == device.Address).Disconnect();

            errorCallback(ex, device);
        }

        public void Debug(string s)
        {
            ((Activity)context).RunOnUiThread(() => Toast.MakeText(context, s, ToastLength.Long).Show());
        }

        public void Connect(BluetoothDevice device)
        {
            lock (connectionsLock) {
                connections.Add(new BluetoothConnection(device, uuid, errorCallback, dataCallback));
            } 
        }

        public void Disconnect(BluetoothDevice device)
        {
            new Thread(new ParameterizedThreadStart(DisconnectThread)).Start();
        }

        public void DisconnectThread(object parameter)
        {
            BluetoothDevice device = (BluetoothDevice)parameter;

            lock (connectionsLock) {
                BluetoothConnection bc = connections.Find(_bc => _bc.bluetoothDevice.Address == device.Address);
                if (bc != null) {
                    bc.Disconnect();
                    connections.Remove(bc);
                }
            }
            
        }

        public void StartListening()
        {
            listen = true;
            listenThread.Start();
        }

        public bool IsListening()
        {
            return listenThread.IsAlive;
        }

        public void StopListening()
        {
            if(IsListening()) {
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
                        connections.Add(new BluetoothConnection(socket.RemoteDevice, uuid, errorCallback, dataCallback, socket));
                    }
                }

                attemptedClose = true;
                serverSocket.Close();
            }
            catch (Exception ex) {
                errorCallback(ex, null);

                if(!attemptedClose) {
                    try { serverSocket.Close(); } catch { errorCallback(ex, null); }
                }
            }
        }
    }

    /*class BluetoothService
    {
        private BluetoothAdapter bluetoothAdapter;

        private Thread incomingConnectionThread;
        private Thread connectionThread;

        private const string name = "BluetoothService";
        private static UUID uuid = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");

        private bool connected;

        private Context context;

        BluetoothSocket socket;

        public BluetoothService(Context activity)
        {
            this.context = activity;
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            incomingConnectionThread = new Thread(ListenForIncomingConnections);
            incomingConnectionThread.Start();

            connectionThread = new Thread(ConnectionThread);
        }

        public void ConnectionThread()
        {
            Looper.Prepare();

            byte[] buffer = new byte[1024];
            int bytes;

            while (true) {
                try {
                    bytes = socket.InputStream.Read(buffer, 0, buffer.Length);

                    string s;
                    unsafe
                    {
                        fixed (byte* bufferPtr = buffer) {
                            s = Encoding.ASCII.GetString(bufferPtr, bytes);
                        }
                    }

                    Debug("Msg: " + s);   
                }
                catch (Exception ex) {
                    //connection lost!
                    Debug(ex.Message + " CONNECTION LOST");
                    connected = false;
                    break;
                }
            }
        }

        public void Debug(string s)
        {
            ((Activity)context).RunOnUiThread(() => Toast.MakeText(context, s, ToastLength.Long).Show());
        }

        public void Write(byte[] bytes)
        {
            if(connected) {
                try {
                    socket.OutputStream.Write(bytes, 0, bytes.Length);
                }
                catch (Exception ex) {
                    Debug(ex.Message);
                    connected = false;
                    return;
                }
            }
        }

        BluetoothServerSocket serverSocket;
        public void ListenForIncomingConnections()
        {
            Looper.Prepare();

            serverSocket = bluetoothAdapter.ListenUsingInsecureRfcommWithServiceRecord(name, uuid);

            try {
                socket = serverSocket.Accept();
                connectionThread.Start();
                Debug("Accepted");
            }
            catch (Exception ex) {
                Debug(ex.Message);
                connected = false;
                return;
            }

            connected = true;
        }
 
        public void Connect(BluetoothDevice device)
        {
            try {
                socket = device.CreateRfcommSocketToServiceRecord(uuid);
               
                socket.Connect();

                connectionThread.Start();
                connected = true;
            }
            catch(Exception ex) {
                Debug(ex.Message);
                connected = false;
            }
        }
    }*/
}