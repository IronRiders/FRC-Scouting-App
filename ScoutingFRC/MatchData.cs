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
    [Serializable]
    public class MatchData
    {
        public int teamNumber;
        public int match;
        public PreformaceData automomous;
        public PreformaceData teleoperated;

        public MatchData()
        {
          automomous = new PreformaceData();
          teleoperated = new PreformaceData();
        }
        [Serializable]
        public class PreformaceData
        {
            public ScoringMethod highBoiler;
            public ScoringMethod lowBoiler;
            public ScoringMethod gears;
            public bool oneTimePoints;

            public PreformaceData()
            {
                highBoiler = new ScoringMethod();
                lowBoiler = new ScoringMethod();
                gears = new ScoringMethod();
            }
            [Serializable]
            public class ScoringMethod
            {
                public int failedAttempts;
                public int successes;

                public ScoringMethod()
                {
                    failedAttempts = 0;
                    successes = 0;
                }
            }
        }
    }
}