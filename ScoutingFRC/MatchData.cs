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
    class MatchData
    {
        public int teamNumber;
        public int match;
        public PerformanceData automomous;
        public PerformanceData teleoperated;

        public MatchData()
        {
          automomous = new PerformanceData();
          teleoperated = new PerformanceData();
        }

        public int GetDataHash()
        {
            return new { automomous, teleoperated }.GetHashCode();
        }

        public override int GetHashCode()
        {
            return new { teamNumber, match, automomous, teleoperated }.GetHashCode();
        }

        public class PerformanceData
        {
            public ScoringMethod highBoiler;
            public ScoringMethod lowBoiler;
            public ScoringMethod gears;
            public bool oneTimePoints;

            public PerformanceData()
            {
                highBoiler = new ScoringMethod();
                lowBoiler = new ScoringMethod();
                gears = new ScoringMethod();
            }

            public override int GetHashCode()
            {
                return new { highBoiler, lowBoiler, gears, oneTimePoints }.GetHashCode();
            }

            public class ScoringMethod
            {
                public int failedAttempts;
                public int successes;

                public ScoringMethod()
                {
                    failedAttempts = 0;
                    successes = 0;
                }

                public override int GetHashCode()
                {
                    return new { failedAttempts, successes }.GetHashCode();
                }
            }
        }
    }
}