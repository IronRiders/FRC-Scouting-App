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

namespace ScoutingFRC
{
    class BluetoothReceiver : BroadcastReceiver
    {
        private List<BluetoothDevice> devices;
        private Action<List<BluetoothDevice>> discoveryFinishedCallback;

        public BluetoothReceiver(Action<List<BluetoothDevice>> discoveryFinishedCallback)
        {
            this.discoveryFinishedCallback = discoveryFinishedCallback;
            devices = new List<BluetoothDevice>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            switch(intent.Action) {
                case BluetoothDevice.ActionFound: {
                        BluetoothDevice device = (BluetoothDevice) intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                        if (device.BondState != Bond.Bonded) {
                            devices.Add(device);
                        }

                        break;
                }
                case BluetoothAdapter.ActionDiscoveryFinished: {
                        discoveryFinishedCallback?.Invoke(devices);
                        devices.Clear();
                        break;
                }
            }
        }
    }
}