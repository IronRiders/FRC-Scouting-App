using System;
using System.Collections.Generic;
using Android.Content;
using Android.Bluetooth;

namespace ScoutingFRC
{
    class BluetoothReceiver : BroadcastReceiver
    {
        private List<BluetoothDevice> devices;
        private Action<List<BluetoothDevice>> discoveryFinishedCallback;
        private Action<BluetoothDevice> deviceFoundCallback;

        public BluetoothReceiver(Action<List<BluetoothDevice>> discoveryFinishedCallback, Action<BluetoothDevice> deviceFoundCallback)
        {
            this.discoveryFinishedCallback = discoveryFinishedCallback;
            this.deviceFoundCallback = deviceFoundCallback;

            devices = new List<BluetoothDevice>();
        }

        /// <summary>
        /// Callback for the BroadcastReceiver. Handles the case when a bluetooth devices is found
        /// and when the discovery finished.
        /// </summary>
        public override void OnReceive(Context context, Intent intent)
        {
            switch(intent.Action) {
                case BluetoothDevice.ActionFound: {
                        BluetoothDevice device = (BluetoothDevice) intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);

                        devices.Add(device);
                        deviceFoundCallback?.Invoke(device);

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