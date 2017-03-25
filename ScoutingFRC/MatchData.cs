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
            timeCollected = DateTime.Now;
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
                lowBoiler = new ScoringMethod();
                gears = new ScoringMethod();

                oneTimePoints = false;
            }

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