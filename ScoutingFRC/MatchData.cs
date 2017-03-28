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
using Java.IO;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ScoutingFRC
{   
    [Serializable]
    class MatchData : TeamData
    {
        public int match;
        public PerformanceData automomous;
        public PerformanceData teleoperated;

        public MatchData()
        {
            automomous = new PerformanceData();
            teleoperated = new PerformanceData();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            MatchData m = obj as MatchData;
            return base.Equals(obj) && match == m.match && automomous.Equals(m.automomous) && teleoperated.Equals(m.teleoperated);
        }

        public static T Deserialize<T>(byte[] bytes) where T : class
        {
            using (MemoryStream stream = new MemoryStream(bytes)) {
                var binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(stream) as T;
            }
        }

        public static byte[] Serialize<T>(T matchData) where T : class
        {
            using (MemoryStream stream = new MemoryStream()) {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, matchData);
                return stream.ToArray();
            }
        }

        public int GetDataHash()
        {
            return new { automomous, teleoperated }.GetHashCode();
        }

        public override int GetHashCode()
        {
            return new { teamNumber, match, automomous, teleoperated }.GetHashCode();
        }

        [Serializable]
        public class PerformanceData
        {
            public ScoringMethod highBoiler;
            public ScoringMethod lowBoiler;
            public ScoringMethod gears;
            public bool oneTimePoints;

            public PerformanceData()
            {
                highBoiler = new ScoringMethod();
                lowBoiler  = new ScoringMethod();
                gears      = new ScoringMethod();

                oneTimePoints = false;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) {
                    return false;
                }

                PerformanceData p = obj as PerformanceData;
                return highBoiler.Equals(p.highBoiler) && lowBoiler.Equals(p.lowBoiler) && gears.Equals(p.gears) && oneTimePoints == p.oneTimePoints;
            }

            /*public PerformanceData Merge(PerformanceData other)
            {
                PerformanceData result = new PerformanceData();

                result.highBoiler = highBoiler.Merge(other.highBoiler);
                result.lowBoiler = lowBoiler.Merge(other.lowBoiler);
                result.gears = gears.Merge(other.gears);
                result.oneTimePoints = oneTimePoints || other.oneTimePoints;

                return result;
            }*/

            public override int GetHashCode()
            {
                return new { highBoiler, lowBoiler, gears, oneTimePoints }.GetHashCode();
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

                public override bool Equals(object obj)
                {
                    if (obj == null || GetType() != obj.GetType()) {
                        return false;
                    }

                    ScoringMethod s = obj as ScoringMethod;
                    return failedAttempts == s.failedAttempts && successes == s.successes;
                }

                /*public ScoringMethod Merge(ScoringMethod other)
                {
                    ScoringMethod result = new ScoringMethod();

                    result.failedAttempts = TeamData.Merge(failedAttempts, other.failedAttempts);
                    result.successes = TeamData.Merge(successes, other.successes);

                    return result;
                }*/

                public override int GetHashCode()
                {
                    return new { failedAttempts, successes }.GetHashCode();
                }

                public void DecrementAttempt(bool successful)
                {
                    if (successful) 
                    {
                        successes--;
                    }
                    else
                    {
                        failedAttempts--;
                    }
                }

                public void IncrementAttempt(bool successful)
                {
                    if (successful)
                    {
                        successes++;
                    }
                    else
                    {
                        failedAttempts++;
                    }
                }

                public override string ToString()
                {
                    return $"{successes}/{failedAttempts + successes}";
                }
            }
        }
    }
}