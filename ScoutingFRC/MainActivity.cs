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

namespace ScoutingFRC
{
    [Activity(Label = "FRC Scouting", Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private List<MatchData> matchDataList = new List<MatchData>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            FindViewById<Button>(Resource.Id.buttonSync).Click += ButtonSync_Click;
            FindViewById<Button>(Resource.Id.buttonCollect).Click += ButtonCollect_Click;
            FindViewById<Button>(Resource.Id.buttonView).Click += ButtonView_Click;

            //Some testing
            //List<MatchData> md = new List<MatchData> { RandomMatchData(), RandomMatchData(), RandomMatchData(), RandomMatchData(), RandomMatchData(), RandomMatchData() };

            //byte[] test = MatchData.Serialize(md);

            //List<MatchData> md2 = MatchData.Deserialize<List<MatchData>>(test);
            //
            matchDataList = new List<MatchData>();
            matchDataList.Add(RandomMatchData());
            matchDataList.Add(RandomMatchData());
            matchDataList.Add(RandomMatchData());
            matchDataList.Add(RandomMatchData());
            matchDataList.Add(RandomMatchData());

        }

        protected override void OnResume()
        {
            base.OnResume();
            FindViewById<TextView>(Resource.Id.textView2).Text = ("Matches Scouted: " + matchDataList.Count);
            String teams = "Teams: ";

            var autocompleteTextView = FindViewById<AutoCompleteTextView>(Resource.Id.autoCompleteTextView1);
            List<int> numbers = new List<int>();
            foreach (var matchData in matchDataList)
            {
                if (!numbers.Contains(matchData.teamNumber))
                {
                    numbers.Add(matchData.teamNumber);
                    teams += (matchData.teamNumber + ", ");
                }
            }
            FindViewById<TextView>(Resource.Id.textView3).Text = teams.Substring(0, teams.Length - 2);

           string[] autoCompleteOptions = numbers.Select(i => i.ToString()).ToArray();
            ArrayAdapter autoCompleteAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleDropDownItem1Line, autoCompleteOptions);
            autocompleteTextView.Adapter = autoCompleteAdapter;
        }
        Random r = new Random();

        MatchData RandomMatchData()
        {
            MatchData md = new MatchData();
             
            md.teamNumber = r.Next();
            md.match = r.Next();

            md.automomous.gears.failedAttempts = r.Next();
            md.automomous.gears.successes = r.Next();

            md.automomous.highBoiler.failedAttempts = r.Next();
            md.automomous.highBoiler.successes = r.Next();

            md.automomous.lowBoiler.failedAttempts = r.Next();
            md.automomous.lowBoiler.successes = r.Next();

            md.automomous.oneTimePoints = r.Next() % 2 == 1;

            md.teleoperated.gears.failedAttempts = r.Next();
            md.teleoperated.gears.successes = r.Next();

            md.teleoperated.highBoiler.failedAttempts = r.Next();
            md.teleoperated.highBoiler.successes = r.Next();

            md.teleoperated.lowBoiler.failedAttempts = r.Next();
            md.teleoperated.lowBoiler.successes = r.Next();

            md.teleoperated.oneTimePoints = r.Next() % 2 == 1;

            md.teleoperated.gears.failedAttempts = r.Next();
            md.teleoperated.gears.successes = r.Next();

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
            List<MatchData> goodData = new List<MatchData>();
            foreach (var matchData in matchDataList)
            {
                if(matchData.teamNumber == number) goodData.Add(matchData);
            }
            if (goodData.Count <= 0)
            {
                return;
            }
            var viewActivity = new Intent(Application.Context, typeof(DataViewingActivity));
            byte[] bytes = MatchData.Serialize(goodData);
            viewActivity.PutExtra("MatchBytes", bytes);
            
            StartActivity(viewActivity);
        }

        private void ButtonSync_Click(object sender, EventArgs e)
        {
            var myIntent = new Intent(this, typeof(SyncDataActivity));
            byte[] bytes = MatchData.Serialize(matchDataList);
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
                    matchDataList.Add(match);
                }
                else if (requestCode == 1)
                {
                    var bytes = data.GetByteArrayExtra("newMatches");
                    var matches = MatchData.Deserialize<List<MatchData>>(bytes);
                    matchDataList.AddRange(matches);
                }
            }
        }
    }
}

