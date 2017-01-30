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
using Java.Lang;

namespace ScoutingFRC
{
    class AcceptThread : Thread
    {
        private BluetoothServerSocket mmServerSocket;
        private BluetoothService2 _service;

        public AcceptThread(BluetoothService2 service)
        {
            Name = "AcceptThread";

            _service = service;
            BluetoothServerSocket tmp = null;

            tmp = _service._adapter.ListenUsingRfcommWithServiceRecord(BluetoothService2.NAME, BluetoothService2.MY_UUID);

            mmServerSocket = tmp;
        }

        public override void Run()
        {
            BluetoothSocket socket = null;

            while (_service._state != BluetoothService2.STATE_CONNECTED) {
                socket = mmServerSocket.Accept();

                if (socket != null) {
                    lock (this) {
                        switch (_service._state) {
                            case BluetoothService2.STATE_LISTEN:
                            case BluetoothService2.STATE_CONNECTING:
                                _service.Connected(socket, socket.RemoteDevice);
                                break;
                            case BluetoothService2.STATE_NONE:
                            case BluetoothService2.STATE_CONNECTED:
                                socket.Close();
                                break;
                        }
                    }
                }
            }

        }

        public void Cancel()
        {
            mmServerSocket.Close();
        }
    }
}