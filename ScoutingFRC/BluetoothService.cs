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
    class BluetoothService
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
    }
}