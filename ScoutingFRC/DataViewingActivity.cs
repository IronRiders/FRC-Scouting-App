using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace ScoutingFRC
{
    [Activity(Label = "Data Viewing", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DataViewingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.ViewData);
            var bytes = Intent.GetByteArrayExtra("MatchBytes");
            List<TeamData> MatchList = MatchData.Deserialize<List<TeamData>>(bytes);

            if (MatchList.Count > 0) {
                DisplayData(MatchList);
            }
        }
        
        /// <summary>  
        ///  Update user interface with Team and Match Data
        /// </summary> 
        private void DisplayData(List<TeamData> datas)
        {
            FindViewById<TextView>(Resource.Id.textViewTeamNumber).Text = datas[0].teamNumber.ToString();
            int count = datas.Count;
            string matches = "";
            int matchCount = 0;
            int[] gears = new int[4];
            int[] HighGoals = new int[4];
            int[] LowGoals = new int[4];
            int baseline = 0;
            double climbing =0;

            foreach (var teamData in datas) {
                if (!(teamData is MatchData)) {
                    continue;
                }
                matchCount++;
                MatchData matchData = teamData as MatchData;
                matches += matchData.match + ", ";
                if (matchData.automomous.oneTimePoints) {
                    baseline++;
                }
                if (matchData.teleoperated.oneTimePoints) {
                    climbing++;
                }
                AddScoringMethod(matchData.automomous.gears, 0, gears);
                AddScoringMethod(matchData.teleoperated.gears, 2, gears);
                AddScoringMethod(matchData.automomous.highBoiler, 0, HighGoals);
                AddScoringMethod(matchData.teleoperated.highBoiler, 2, HighGoals);
                AddScoringMethod(matchData.automomous.lowBoiler, 0, LowGoals);
                AddScoringMethod(matchData.teleoperated.lowBoiler, 2, LowGoals);
            }

            double[] high = DivideArray(HighGoals, matchCount);
            double[] low = DivideArray(LowGoals, matchCount);
            double[] gear = DivideArray(gears, matchCount);
            double baselinePercentage = (((double)baseline)/matchCount)*100;
            double climbingPercentage = (climbing / matchCount) * 100;

            UpdateTextView(Resource.Id.textViewBaseline, $"Baseline - {Math.Round(baselinePercentage,2)}%",(int)baselinePercentage);
            UpdateTextView(Resource.Id.textViewAutoGear, $"Gear - {Math.Round(gear[0]*100, 2)}%", gear[0]);
            UpdateTextView(Resource.Id.textViewAutoHG, $"High Goals - {Math.Round(high[0], 2)}", high[0]);
            UpdateTextView(Resource.Id.textViewAutoLG, $"Low Goals - {Math.Round(low[0], 2)}", low[0]);

            UpdateTextView(Resource.Id.textViewTeleGears, $"Gears - {Math.Round(gear[2], 2)}/{Math.Round(gear[3], 2)}", gear[3]);
            UpdateTextView(Resource.Id.textViewTeleHG, $"High Goals - {Math.Round(high[2], 2)}/{Math.Round(high[3], 2)}", high[3]);
            UpdateTextView(Resource.Id.textViewTeleLG, $"Low Goals - {Math.Round(low[2], 2)}/{Math.Round(low[3], 2)}", low[3]);
            UpdateTextView(Resource.Id.textViewClimbingView, $"Climbing - {Math.Round(climbingPercentage, 2)}%", climbingPercentage);

            if (matchCount > 0) {
                FindViewById<TextView>(Resource.Id.textView1).Text = ((matchCount>1)? "Mathces: " : "Match: ") + matches.Substring(0, matches.Length - 2);
                double autoPoints = (baselinePercentage/100)*5 + (gear[0])*60 + high[0] + low[0]/3;
                double telePoints = (climbingPercentage / 100) * 50 + (gear[2]) * 10 + high[0]/3 + low[0] / 9;
                FindViewById<TextView>(Resource.Id.textViewAutoPts).Text = Math.Round(autoPoints, 3) + " pts";
                FindViewById<TextView>(Resource.Id.textViewTelePts).Text = Math.Round(telePoints, 3) + " pts";

            }
            else {
                FindViewById<TextView>(Resource.Id.textView1).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.linearLayoutAuto).Visibility = ViewStates.Gone;
                FindViewById<LinearLayout>(Resource.Id.linearLayoutTele).Visibility = ViewStates.Gone;
            }

            LinearLayout.LayoutParams textViewLayout = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent);
            foreach (var teamData in datas)  {
                if (!string.IsNullOrEmpty(teamData.notes)) {
                    String note = ($"\"{teamData.notes}\" - {teamData.scoutName}");
                    TextView text = new TextView(this);
                    text.LayoutParameters = textViewLayout;
                    text.Text = note;
                    FindViewById<LinearLayout>(Resource.Id.linearLayoutListNotes).AddView(text);
                }
            }
        }

        /// <summary>  
        ///  given a string and an ID sets that TextView to that string
        /// </summary> 
        private void UpdateTextView(int id, String value, double visible)
        {
            using (TextView textView = FindViewById<TextView>(id)) {
                if (visible > 0) {
                    textView.Text = value;
                }
                else {
                    textView.Visibility = ViewStates.Gone;
                }
            }
        }

        /// <summary>  
        /// divide every element in an array by a given int.
        /// </summary> 
        private double[] DivideArray(int[] ar, int a)
        {
            double[] result = new double[ar.Length];
            for (int j = 0; j < ar.Length; j++) {
                result[j] = ((double) ar[j])/a;
            }
            return result;
        }
        
        /// <summary>  
        ///  add that successes and failure to a specifide index in an array
        /// </summary> 
        private void AddScoringMethod(MatchData.PerformanceData.ScoringMethod method, int start, int[] arr)
        {
            arr[start] += method.successes;
            arr[start + 1] += method.failedAttempts + method.successes;
        }
    }
}
