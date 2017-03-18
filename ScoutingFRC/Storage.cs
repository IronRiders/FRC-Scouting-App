using System;
using System.Collections.Generic;
using System.IO;

using System.Runtime.Serialization.Formatters.Binary;
using Android.Graphics;

using static System.IO.Path;

namespace ScoutingFRC
{
    class Storage
    {

        private static string GetPersonalFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        public static void Delete(string username)
        {
            string tsdPath = Combine(GetPersonalFolderPath(), username + ".tsd");

            if (File.Exists(tsdPath)) {
                File.Delete(tsdPath);
            }
        }

        public static void WriteToFile(string username, List<MatchData> data)
        {
            string tsdPath = Combine(GetPersonalFolderPath(), username + ".tsd");

            using (Stream stream = File.Open(tsdPath, FileMode.Create)) {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, data);
            }
          
        }

        public static List<MatchData> ReadFromFile(string username)
        {
            string tsdPath = Combine(GetPersonalFolderPath(), username + ".tsd");
         
            List<MatchData> result = new List<MatchData>();

            if (!File.Exists(tsdPath)) {
                return null;
            }

            try {
                using (Stream stream = File.Open(Combine(GetPersonalFolderPath(), tsdPath), FileMode.Open)) {
                    var binaryFormatter = new BinaryFormatter();
                    result = (List<MatchData>)binaryFormatter.Deserialize(stream);
                }
            }
            catch {
                File.Delete(tsdPath);
                return result;
            }

            return result;
        }
    }
}