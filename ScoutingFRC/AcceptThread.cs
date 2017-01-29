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
        private BluetoothService _service;

        public AcceptThread(BluetoothService service)
        {
            Name = "AcceptThread";

            _service = service;
            BluetoothServerSocket tmp = null;

            tmp = _service._adapter.ListenUsingRfcommWithServiceRecord(BluetoothService.NAME, BluetoothService.MY_UUID);

            mmServerSocket = tmp;
        }

        public override void Run()
        {
            BluetoothSocket socket = null;

            while (_service._state != BluetoothService.STATE_CONNECTED) {
                socket = mmServerSocket.Accept();

                if (socket != null) {
                    lock (this) {
                        switch (_service._state) {
                            case BluetoothService.STATE_LISTEN:
                            case BluetoothService.STATE_CONNECTING:
                                _service.Connected(socket, socket.RemoteDevice);
                                break;
                            case BluetoothService.STATE_NONE:
                            case BluetoothService.STATE_CONNECTED:
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