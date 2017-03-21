using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Bluetooth;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Android.Content;
using System.Text;
using Android.Content.PM;
using System.ComponentModel;
using Android.Views;

namespace ScoutingFRC
{
    [Activity(Label = "FRC Scouting", Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private List<TeamData> teamDataList = new List<TeamData>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViewById<Button>(Resource.Id.buttonSync).Click += ButtonSync_Click;
            FindViewById<Button>(Resource.Id.buttonCollect).Click += ButtonCollect_Click;
            FindViewById<Button>(Resource.Id.buttonView).Click += ButtonView_Click;
            FindViewById<ListView>(Resource.Id.listView1).ItemClick += OnItemClick;

            teamDataList = Storage.ReadFromFile("test") ?? new List<TeamData>();

        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var item = FindViewById<ListView>(Resource.Id.listView1).Adapter.GetItem(itemClickEventArgs.Position);
            int num = int.Parse((string)item);
            DisplayTeamData(num);
        }

        protected override void OnResume()
        {
            base.OnResume();
            FindViewById<TextView>(Resource.Id.textView2).Text = ("Matches Scouted: " + teamDataList.Count);

            List<int> teamsList = new List<int>();
            foreach (var matchData in teamDataList)
            {
                if (!teamsList.Contains(matchData.teamNumber))
                {
                    teamsList.Add(matchData.teamNumber);
                }
            }
           FindViewById<TextView>(Resource.Id.textView3).Text = "Teams: "+teamsList.Count;
            var autocompleteTextView = FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
            teamsList.Sort();
            string[] autoCompleteOptions = teamsList.Select(i => i.ToString()).ToArray();
            ArrayAdapter autoCompleteAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, autoCompleteOptions);
            autocompleteTextView.Adapter = autoCompleteAdapter;

            var list = FindViewById<ListView>(Resource.Id.listView1);
            ArrayAdapter teamListAdapter  = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, autoCompleteOptions);
            list.Adapter = teamListAdapter;
        }

        Random r = new Random((int)(DateTime.Now.Ticks % int.MaxValue));

        MatchData RandomMatchData()
        {
            MatchData md = new MatchData();
             
            md.teamNumber = r.Next() % 5000;
            md.match = r.Next() % 5000;

            md.automomous.gears.failedAttempts = r.Next() % 5000;
            md.automomous.gears.successes = r.Next() % 5000;

            md.automomous.highBoiler.failedAttempts = r.Next() % 5000;
            md.automomous.highBoiler.successes = r.Next() % 5000;

            md.automomous.lowBoiler.failedAttempts = r.Next() % 5000;
            md.automomous.lowBoiler.successes = r.Next() % 5000;

            md.automomous.oneTimePoints = r.Next() % 2 == 1;

            md.teleoperated.gears.failedAttempts = r.Next() % 5000;
            md.teleoperated.gears.successes = r.Next() % 5000;

            md.teleoperated.highBoiler.failedAttempts = r.Next() % 5000;
            md.teleoperated.highBoiler.successes = r.Next() % 5000;

            md.teleoperated.lowBoiler.failedAttempts = r.Next() % 5000;
            md.teleoperated.lowBoiler.successes = r.Next() % 5000;

            md.teleoperated.oneTimePoints = r.Next() % 2 == 1;

            md.teleoperated.gears.failedAttempts = r.Next() % 5000;
            md.teleoperated.gears.successes = r.Next() % 5000;

            return md;
        }

        private void ButtonCollect_Click(object sender, EventArgs e)
        {
            var myIntent = new Intent(this, typeof(DataCollectionActivity));
            StartActivityForResult(myIntent, 0);
        }

        private void ButtonView_Click(object sender, EventArgs e)
        {
            int number;
            bool parsed = int.TryParse(FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1).Text, out number);
            if (!parsed)
            {
                return;
            }
            DisplayTeamData(number);
        }

        private void DisplayTeamData(int number)
        {
            List<TeamData> teamData = new List<TeamData>();
            foreach (var matchData in teamDataList) {
                if (matchData.teamNumber == number) teamData.Add(matchData);
            }
            if (teamData.Count <= 0) {
                return;
            }
            var viewActivity = new Intent(Application.Context, typeof(DataViewingActivity));
            byte[] bytes = MatchData.Serialize(teamData);
            viewActivity.PutExtra("MatchBytes", bytes);

            StartActivity(viewActivity);
        }

        private void ButtonSync_Click(object sender, EventArgs e)
        {
            var myIntent = new Intent(this, typeof(SyncDataActivity));
            byte[] bytes = MatchData.Serialize(teamDataList);
            myIntent.PutExtra("currentData", bytes);
            StartActivityForResult(myIntent, 1);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (resultCode == Result.Ok) {
                if (requestCode == 0)
                {
                    var bytes = data.GetByteArrayExtra("W");
                    var match = MatchData.Deserialize<MatchData>(bytes);
                    teamDataList.Add(match);
                }
                else if (requestCode == 1)
                {
                    var bytes = data.GetByteArrayExtra("newMatches");
                    var matches = MatchData.Deserialize<List<TeamData>>(bytes);
                    teamDataList.AddRange(matches);
                }
                Storage.Delete("test");
                Storage.WriteToFile("test",teamDataList);
            }
        }
    }
}

