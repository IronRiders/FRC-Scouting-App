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

        public TeamData Merge(TeamData o)
        {
            MatchData other = o as MatchData;
            MatchData result = Merge<MatchData>(o);
            
            result.match = match;
            result.automomous = automomous.Merge(other.automomous);
            result.teleoperated = teleoperated.Merge(other.teleoperated);

            return result;
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

            public PerformanceData Merge(PerformanceData other)
            {
                PerformanceData result = new PerformanceData();

                result.highBoiler = highBoiler.Merge(other.highBoiler);
                result.lowBoiler = lowBoiler.Merge(other.lowBoiler);
                result.gears = gears.Merge(other.gears);
                result.oneTimePoints = oneTimePoints || other.oneTimePoints;

                return result;
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

                public ScoringMethod Merge(ScoringMethod other)
                {
                    ScoringMethod result = new ScoringMethod();

                    result.failedAttempts = TeamData.Merge(failedAttempts, other.failedAttempts);
                    result.successes = TeamData.Merge(successes, other.successes);

                    return result;
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