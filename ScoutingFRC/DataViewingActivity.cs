using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
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

            if (MatchList.Count > 0)
            {
                displayData(MatchList);
            }

        }

        private void displayData(List<TeamData> datas)
        {
            FindViewById<TextView>(Resource.Id.textViewTeamNumber).Text = datas[0].teamNumber.ToString();
            int count = datas.Count;
            string matches = "Matches: ";
            int[] gears = new int[4];
            int[] HighGoals = new int[4];
            int[] LowGoals = new int[4];
            foreach (var teamData in datas)
            {
                if (!(teamData is MatchData))
                {
                    continue;
                }
                MatchData matchData = teamData as MatchData;
                matches += matchData.match + ", ";
                addScoringMethod(matchData.automomous.gears, 0, gears);
                addScoringMethod(matchData.teleoperated.gears, 2, gears);
                addScoringMethod(matchData.automomous.highBoiler, 0, HighGoals);
                addScoringMethod(matchData.teleoperated.highBoiler, 2, HighGoals);
                addScoringMethod(matchData.automomous.lowBoiler, 0, LowGoals);
                addScoringMethod(matchData.teleoperated.lowBoiler, 2, LowGoals);
            }
            double[] high = divide(HighGoals, count);
            double[] low = divide(LowGoals, count);
            double[] gear = divide(gears, count);
            
            FindViewById<TextView>(Resource.Id.textView1).Text = matches.Substring(0,matches.Length-2);

            FindViewById<TextView>(Resource.Id.textViewAG).Text = String.Format("{0:#.###}/{1:#.##}",gear[0],gear[1]);
            FindViewById<TextView>(Resource.Id.textViewTG).Text = String.Format("{0:#.###}/{1:#.##}", gear[2], gear[3]);

            FindViewById<TextView>(Resource.Id.textViewAH).Text = String.Format("{0:#.###}/{1:#.##}", high[0], high[1]);
            FindViewById<TextView>(Resource.Id.textViewTH).Text = String.Format("{0:#.###}/{1:#.##}", high[2], high[3]);

            FindViewById<TextView>(Resource.Id.textViewAL).Text = String.Format("{0:#.###}/{1:#.##}", low[0], low[1]);
            FindViewById<TextView>(Resource.Id.textViewTL).Text = String.Format("{0:#.###}/{1:#.##}", low[2], low[3]);
        }

        private double[] divide(int[] ar, int a)
        {
            double[] ret = new double[ar.Length];
            for (int j = 0; j < ar.Length; j++)
            {
                ret[j] = ((double) ar[j])/a;
            }
            return ret;
        }

        private void addScoringMethod(MatchData.PerformanceData.ScoringMethod method, int start, int[] arr)
        {
            arr[start] += method.successes;
            arr[start + 1] += method.failedAttempts + method.successes;
        }
    }
}