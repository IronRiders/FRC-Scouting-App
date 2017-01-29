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
        private List<string> devices;
        private Action<List<string>> discoveryFinishedCallback;

        public BluetoothReceiver(Action<List<string>> discoveryFinishedCallback)
        {
            this.discoveryFinishedCallback = discoveryFinishedCallback;
            devices = new List<string>();
        }

        public override void OnReceive(Context context, Intent intent)
        {
            switch(intent.Action) {
                case BluetoothDevice.ActionFound: {
                        BluetoothDevice device = (BluetoothDevice) intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                        if (device.BondState != Bond.Bonded) {
                            devices.Add(device.Name);
                        }

                        break;
                }
                case BluetoothAdapter.ActionDiscoveryFinished: {
                        devices.Insert(0, string.Format("--- Test, {0} devices found ---", devices.Count));
                        discoveryFinishedCallback?.Invoke(devices);
                        break;
                }
            }
        }
    }
}