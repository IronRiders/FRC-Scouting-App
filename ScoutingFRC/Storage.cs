using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using static System.IO.Path;

namespace ScoutingFRC
{
    class Storage
    {

        private static string GetPersonalFolderPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }

        public static void Delete(string fileName)
        {
            var tsdPath = Combine(GetPersonalFolderPath(), fileName + ".frc");

            if (File.Exists(tsdPath)) {
                File.Delete(tsdPath);
            }
        }

        public static void WriteToFile(string fileName, List<TeamData> data)
        {
            var tsdPath = Combine(GetPersonalFolderPath(), fileName + ".frc");

            using (Stream stream = File.Open(tsdPath, FileMode.Create)) {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, data);
            }
          
        }

        public static List<TeamData> ReadFromFile(string fileName)
        {
            var tsdPath = Combine(GetPersonalFolderPath(), fileName + ".frc");
         
            var result = new List<TeamData>();

            if (!File.Exists(tsdPath)) {
                return null;
            }

            try {
                using (Stream stream = File.Open(Combine(GetPersonalFolderPath(), tsdPath), FileMode.Open)) {
                    var binaryFormatter = new BinaryFormatter();
                    result = (List<TeamData>)binaryFormatter.Deserialize(stream);
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