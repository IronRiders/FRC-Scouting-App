using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Bluetooth;

namespace ScoutingFRC
{
    [Activity(Label = "ScoutingFRC", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
             SetContentView (Resource.Layout.Main);

            list = new List<string>();
            list.Add("Test");
            adapter = new ArrayAdapter<string>(this, Resource.Id.listView1, list);
            var listView = FindViewById<ListView>(Resource.Id.listView1);
            listView.Adapter = adapter;

            var bluetoothReceiver = new BluetoothReceiver(DiscoveryFinishedCallback);
            RegisterReceiver(bluetoothReceiver, new Android.Content.IntentFilter(BluetoothDevice.ActionFound));
            RegisterReceiver(bluetoothReceiver, new Android.Content.IntentFilter(BluetoothAdapter.ActionDiscoveryFinished));

            bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            bluetoothAdapter.StartDiscovery();
        }

        private void DiscoveryFinishedCallback(List<string> devices)
        {
            RunOnUiThread(() =>
            {
                //list.AddRange(devices);
                adapter.NotifyDataSetChanged();
            });
        }

        private ArrayAdapter<string> adapter;
        private List<string> list;
        private BluetoothAdapter bluetoothAdapter;
    }
}

