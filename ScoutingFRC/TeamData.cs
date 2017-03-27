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
    class TeamData
    {
        public int teamNumber;
        public string scoutName;
        public string notes;

        public T Merge<T>(TeamData other) where T : TeamData, new()
        {
            T result = new T();

            result.teamNumber = teamNumber;
            result.scoutName = scoutName;
            result.notes = notes;

            return result;
        }

        public static int Merge(int a, int b)
        {
            bool a0 = a == 0;
            bool b0 = b == 0;

            if(!a0 && !b0) {
                return (a + b) / 2;
            }
            else if (a0 || b0) {
                return Math.Max(a, b);
            }

            return 0;
        }
    }
}