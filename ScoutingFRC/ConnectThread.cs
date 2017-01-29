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
using Java.Lang;
using Android.Bluetooth;

namespace ScoutingFRC
{
    class ConnectThread : Thread
    {
        private BluetoothSocket mmSocket;
        private BluetoothDevice mmDevice;
        private BluetoothService _service;

        public ConnectThread(BluetoothDevice device, BluetoothService service)
        {
            Name = "ConnectThread";

            mmDevice = device;
            _service = service;
            BluetoothSocket tmp = null;

           tmp = device.CreateRfcommSocketToServiceRecord(BluetoothService.MY_UUID);
           mmSocket = tmp;
        }

        public override void Run()
        {
            _service._adapter.CancelDiscovery();

            try {
                mmSocket.Connect();
            }
            catch (Java.IO.IOException e) {
                _service.ConnectionFailed();
                mmSocket.Close();
                _service.Start();
                return;
            }

            lock (this) {
                _service.connectThread = null;
            }

            _service.Connected(mmSocket, mmDevice);
        }

        public void Cancel()
        {
            mmSocket.Close();
        }
    }
}