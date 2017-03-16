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

namespace ScoutingFRC
{
    [Activity(Label = "Sync Data")]
    public class SyncDataActivity : Activity
    {
        List<MatchData> CurrentData = new List<MatchData>();
        List<MatchData> NewData = new List<MatchData>();
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SyncDevices);
            
            var bytes = Intent.GetByteArrayExtra("currentData");
            CurrentData = MatchData.Deserialize<List<MatchData>>(bytes);
            FindViewById<Button>(Resource.Id.buttonExchange).Click += ButtonExchange_Click;
            FindViewById<Button>(Resource.Id.buttonAdd).Click += ButtonAdd_Click;
            FindViewById<Button>(Resource.Id.buttonCancel).Click += ButtonCancel_Click;
        }

        private void ButtonCancel_Click(object sender, EventArgs eventArgs)
        {
            Intent myIntent = new Intent(this, typeof(MainActivity));
            SetResult(Result.Canceled, myIntent);
            Finish();
        }

        private void ButtonAdd_Click(object sender, EventArgs eventArgs)
        {
            Intent myIntent = new Intent(this, typeof(MainActivity));
            var bytes = MatchData.Serialize(NewData);
            myIntent.PutExtra("newMatches", bytes);
            SetResult(Result.Ok, myIntent);
            Finish();
        }

        private void ButtonExchange_Click(object sender, EventArgs eventArgs)
        {
            
        }

    }
}