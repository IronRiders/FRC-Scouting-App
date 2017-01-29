using System.IO;
using System.Runtime.CompilerServices;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Lang;
using Java.Util;

namespace ScoutingFRC
{
    class BluetoothService
    {
        // Debugging
        private const string TAG = "BluetoothService";
        private const bool Debug = true;

        // Name for the SDP record when creating server socket
        public const string NAME = "BluetoothServiceName";

        // Unique UUID for this application
        public static UUID MY_UUID = UUID.FromString("97686312-1B47-4B52-8D08-6A072E2699A6");

        // Member fields
        public BluetoothAdapter _adapter;
        public Handler _handler;
        public AcceptThread acceptThread;
        public ConnectThread connectThread;
        public ConnectedThread connectedThread;
        public int _state;

        // Constants that indicate the current connection state
        // TODO: Convert to Enums
        public const int STATE_NONE = 0;       // we're doing nothing
        public const int STATE_LISTEN = 1;     // now listening for incoming connections
        public const int STATE_CONNECTING = 2; // now initiating an outgoing connection
        public const int STATE_CONNECTED = 3;  // now connected to a remote device

        public BluetoothService(Context context, Handler handler)
        {
            _adapter = BluetoothAdapter.DefaultAdapter;
            _state = STATE_NONE;
            _handler = handler;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetState(int state)
        {
            if (Debug)
                Log.Debug(TAG, "setState() " + _state + " -> " + state);

            _state = state;

            // Give the new state to the Handler so the UI Activity can update
            _handler.ObtainMessage(MainActivity.MESSAGE_STATE_CHANGE, state, -1).SendToTarget();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetState()
        {
            return _state;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Start()
        {
            if (Debug)
                Log.Debug(TAG, "start");

            // Cancel any thread attempting to make a connection
            if (connectThread != null) {
                connectThread.Cancel();
                connectThread = null;
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null) {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Start the thread to listen on a BluetoothServerSocket
            if (acceptThread == null) {
                acceptThread = new AcceptThread(this);
                acceptThread.Start();
            }

            SetState(STATE_LISTEN);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connect(BluetoothDevice device)
        {
            if (Debug)
                Log.Debug(TAG, "connect to: " + device);

            // Cancel any thread attempting to make a connection
            if (_state == STATE_CONNECTING) {
                if (connectThread != null) {
                    connectThread.Cancel();
                    connectThread = null;
                }
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null) {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Start the thread to connect with the given device
            connectThread = new ConnectThread(device, this);
            connectThread.Start();

            SetState(STATE_CONNECTING);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Connected(BluetoothSocket socket, BluetoothDevice device)
        {
            if (Debug)
                Log.Debug(TAG, "connected");

            // Cancel the thread that completed the connection
            if (connectThread != null) {
                connectThread.Cancel();
                connectThread = null;
            }

            // Cancel any thread currently running a connection
            if (connectedThread != null) {
                connectedThread.Cancel();
                connectedThread = null;
            }

            // Cancel the accept thread because we only want to connect to one device
            if (acceptThread != null) {
                acceptThread.Cancel();
                acceptThread = null;
            }

            // Start the thread to manage the connection and perform transmissions
            connectedThread = new ConnectedThread(socket, this);
            connectedThread.Start();

            // Send the name of the connected device back to the UI Activity
            var msg = _handler.ObtainMessage(MainActivity.MESSAGE_DEVICE_NAME);
            Bundle bundle = new Bundle();
            bundle.PutString(MainActivity.DEVICE_NAME, device.Name);
            msg.Data = bundle;
            _handler.SendMessage(msg);

            SetState(STATE_CONNECTED);
        }

        /// <summary>
        /// Stop all threads.
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (Debug)
                Log.Debug(TAG, "stop");

            if (connectThread != null) {
                connectThread.Cancel();
                connectThread = null;
            }

            if (connectedThread != null) {
                connectedThread.Cancel();
                connectedThread = null;
            }

            if (acceptThread != null) {
                acceptThread.Cancel();
                acceptThread = null;
            }

            SetState(STATE_NONE);
        }

        public void Write(byte[] @out)
        {
            // Create temporary object
            ConnectedThread r;
            // Synchronize a copy of the ConnectedThread
            lock (this) {
                if (_state != STATE_CONNECTED)
                    return;
                r = connectedThread;
            }
            // Perform the write unsynchronized
            r.Write(@out);
        }

        public void ConnectionFailed()
        {
            SetState(STATE_LISTEN);

            // Send a failure message back to the Activity
            var msg = _handler.ObtainMessage(MainActivity.MESSAGE_TOAST);
            Bundle bundle = new Bundle();
            bundle.PutString(MainActivity.TOAST, "Unable to connect device");
            msg.Data = bundle;
            _handler.SendMessage(msg);
        }

        public void ConnectionLost()
        {
            SetState(STATE_LISTEN);

            // Send a failure message back to the Activity
            var msg = _handler.ObtainMessage(MainActivity.MESSAGE_TOAST);
            Bundle bundle = new Bundle();
            bundle.PutString(MainActivity.TOAST, "Device connection was lost");
            msg.Data = bundle;
            _handler.SendMessage(msg);
        }
    }
}