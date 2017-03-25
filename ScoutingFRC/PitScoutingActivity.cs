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
    [Activity(Label = "Pit Scouting")]
    public class PitScoutingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PitScouting);
            FindViewById<Button>(Resource.Id.buttonSubmitPit).Click += SubmitPitData;
            var name = Intent.GetStringExtra("name");
            if (name != null) {
                FindViewById<TextView>(Resource.Id.editTextNameP).Text = name;
            }
        }

        private void SubmitPitData(object sender, EventArgs eventArgs)
        {
            var teamData = new TeamData();
            try {
                teamData.teamNumber = int.Parse(FindViewById<TextView>(Resource.Id.editTextTeamNumP).Text);
            }
            catch (Exception) {
                ComplainAboutFeild("a team number");
                return;
            }

            string name = FindViewById<TextView>(Resource.Id.editTextNameP).Text;
            if (string.IsNullOrEmpty(name)) {
                ComplainAboutFeild("your name");
                return;
            }
            teamData.scoutName = name;
            string notes = FindViewById<TextView>(Resource.Id.editTextNotesP).Text;
            if (string.IsNullOrEmpty(name)) {
                ComplainAboutFeild("taking any notes");
                return;
            }
            teamData.notes = notes;

            Intent myIntent = new Intent(this, typeof(MainActivity));

            var bytes = MatchData.Serialize(teamData);
            myIntent.PutExtra("newPitData", bytes);
            SetResult(Result.Ok, myIntent);
            Finish();

        }
        private void ComplainAboutFeild(string missing)
        {
            var builder = new AlertDialog.Builder(this)
                 .SetTitle("Cannot Submit Pit Scouting Data!")
                 .SetMessage($"You cannot submit pit scouting data without {missing}")
                 .SetPositiveButton("Ok", (sender, args) => { });
            builder.Create().Show();
        }
    }
}