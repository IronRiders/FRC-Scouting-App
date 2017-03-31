using System;

namespace ScoutingFRC
{
    [Serializable]
    class TeamData
    {
        public int teamNumber;
        public string scoutName;
        public string notes;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            TeamData t = obj as TeamData;
            return teamNumber == t.teamNumber && scoutName == t.scoutName && notes == t.notes;
        }

        /*public T Merge<T>(TeamData other) where T : TeamData, new()
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
        }*/
    }
}