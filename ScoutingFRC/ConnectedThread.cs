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
using System.IO;
using Java.Lang;

namespace ScoutingFRC
{
    class ConnectedThread : Thread
    {
        private BluetoothSocket mmSocket;
        private Stream mmInStream;
        private Stream mmOutStream;
        public BluetoothService _service;

        public ConnectedThread(BluetoothSocket socket, BluetoothService service)
        {
            Name = "ConnectedThread";

            mmSocket = socket;
            _service = service;
            Stream tmpIn = null;
            Stream tmpOut = null;

            tmpIn = socket.InputStream;
            tmpOut = socket.OutputStream;

            mmInStream = tmpIn;
            mmOutStream = tmpOut;
        }

        public override void Run()
        {
            byte[] buffer = new byte[1024];
            int bytes;

            while (true) {
                try {
                    bytes = mmInStream.Read(buffer, 0, buffer.Length);

                    _service._handler.ObtainMessage(MainActivity.MESSAGE_READ, bytes, -1, buffer).SendToTarget();
                }
                catch (Java.IO.IOException e) {
                    _service.ConnectionLost();
                    break;
                }
            }
        }

        public void Write(byte[] buffer)
        {
                mmOutStream.Write(buffer, 0, buffer.Length);
                _service._handler.ObtainMessage(MainActivity.MESSAGE_WRITE, -1, -1, buffer).SendToTarget();
        }

        public void Cancel()
        {
            mmSocket.Close();
        }
    }
}