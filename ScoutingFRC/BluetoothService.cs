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

namespace ScoutingFRC
{
    class BluetoothService
    {
        private BluetoothAdapter bluetoothAdapter;

        private Thread incomingConnectionThread;

        private const string name = "BluetoothService";
        private static UUID uuid = UUID.FromString("fa87c0d0-afac-11de-8a39-0800200c9a66");

        private bool connected;

        public BluetoothService()
        {
            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;

            incomingConnectionThread = new Thread(ListenForIncomingConnections);
            incomingConnectionThread.Start();
        }

        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            serverSocket.Close();
        }

        BluetoothServerSocket serverSocket;
        public void ListenForIncomingConnections()
        {
            serverSocket = bluetoothAdapter.ListenUsingInsecureRfcommWithServiceRecord(name, uuid);
            BluetoothSocket socket;

            try {
                socket = serverSocket.Accept();
            }
            catch {
                connected = false;
                return;
            }

            connected = true;
        }

        public void Connect(BluetoothDevice device)
        {
            BluetoothSocket socket = device.CreateRfcommSocketToServiceRecord(uuid);
           
            socket.Connect();

            connected = true;
        }
    }
}