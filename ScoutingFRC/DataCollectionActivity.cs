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
    [Activity(Label = "Data Collection")]
    public class DataCollectionActivity : Activity
    {
        private MatchData matchData;
        private bool autonomous = true;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DataCollection);
            matchData = new MatchData();
            FindViewById<Button>(Resource.Id.buttonSubmit).Click += ButtonSubmit_Click;
            FindViewById<Button>(Resource.Id.buttonGearGoal).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.gears.successes,ref matchData.teleoperated.gears.successes);
                };
            FindViewById<Button>(Resource.Id.buttonGearMiss).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.gears.failedAttempts, ref matchData.teleoperated.gears.failedAttempts);
                };
            FindViewById<Button>(Resource.Id.buttonHighGoal).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.highBoiler.successes, ref matchData.teleoperated.highBoiler.successes);
                };
            FindViewById<Button>(Resource.Id.buttonHighMiss).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.highBoiler.failedAttempts, ref matchData.teleoperated.highBoiler.failedAttempts);
                };
            FindViewById<Button>(Resource.Id.buttonLowGoal).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.lowBoiler.successes, ref matchData.teleoperated.lowBoiler.successes);
                };
            FindViewById<Button>(Resource.Id.buttonLowMiss).Click +=
                (sender, args) =>
                {
                    addAttempt(ref matchData.automomous.lowBoiler.failedAttempts, ref matchData.teleoperated.lowBoiler.failedAttempts);
                };
            FindViewById<Switch>(Resource.Id.switchAuto).CheckedChange += OnCheckedChange;
        }

        private void OnCheckedChange(object sender, CompoundButton.CheckedChangeEventArgs checkedChangeEventArgs)
        {
            autonomous = FindViewById<Switch>(Resource.Id.switchAuto).Checked;
            using (var line = FindViewById<CheckBox>(Resource.Id.checkBox1))
            using (var rope = FindViewById<CheckBox>(Resource.Id.checkBoxClimb))
            {
                line.Enabled = autonomous;
                rope.Enabled = !autonomous;
            }
        }


        private void addAttempt(ref int auto,ref int tele)
        {
            if (autonomous) {
                auto++;
            }
            else {
                tele++;
            }
        }


        private void ButtonSubmit_Click(object sender, EventArgs e)
        {
            matchData.teamNumber = int.Parse(FindViewById<TextView>(Resource.Id.editTextTeamNumber).Text);
            matchData.match = int.Parse(FindViewById<TextView>(Resource.Id.editTextMathcNumber).Text);
       
        }
    }
}