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
        private int teamNumber;
        private int match;
        private PreformaceData automomous;
        private PreformaceData teleoporated;

        public class PreformaceData
        {
            private ScoringMethod highBoiler;
            private ScoringMethod lowBoiler;
            private ScoringMethod gears;
            private bool oneTimePoints;

            public class ScoringMethod
            {
                private int failedAttempts;
                private int successes;
            }
        }
    }
}