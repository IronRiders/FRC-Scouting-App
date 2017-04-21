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
    }
}
