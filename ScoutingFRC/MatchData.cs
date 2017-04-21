using System;
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

        /// <summary>
        /// Deserializes a byte array to an object.
        /// </summary>
        public static T Deserialize<T>(byte[] bytes) where T : class
        {
            using (MemoryStream stream = new MemoryStream(bytes)) {
                var binaryFormatter = new BinaryFormatter();
                return binaryFormatter.Deserialize(stream) as T;
            }
        }

        /// <summary>
        /// Serializes an object into a byte array.
        /// </summary>
        public static byte[] Serialize<T>(T matchData) where T : class
        {
            using (MemoryStream stream = new MemoryStream()) {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, matchData);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Returns a hash of the data, excluding teamNumber and match.
        /// </summary>
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

                public override int GetHashCode()
                {
                    return new { failedAttempts, successes }.GetHashCode();
                }

                /// <summary>
                /// Decreases number of attempts.
                /// </summary>
                public void DecrementAttempt(bool successful)
                {
                    if (successful)  {
                        successes--;
                    }
                    else {
                        failedAttempts--;
                    }
                }

                /// <summary>
                /// Increases number of attempts.
                /// </summary>
                public void IncrementAttempt(bool successful)
                {
                    if (successful) {
                        successes++;
                    }
                    else {
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