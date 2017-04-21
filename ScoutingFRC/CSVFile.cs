using System;
using System.IO;

namespace ScoutingFRC
{
    class CSVFile : IDisposable
    {
        public CSVFile(string path, FileMode mode)
        {
            s  = File.Open(path, mode, FileAccess.ReadWrite);

            sr = new StreamReader(s);
            sw = new StreamWriter(s);
            
            disposed = false;
        }

        /// <summary>
        /// Closes and diposes all streams
        /// </summary>
        public void Dispose()
        {
            if (!disposed) {
                sw.Close();
                sr.Close();
                s.Close();
                sw.Dispose();
                sr.Dispose();
                s.Dispose();
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Writes the objects in data seperated by commas to the stream
        /// </summary>
        void WriteData(params object[] data)
        {
            for (int i = 0; i < data.Length; ++i) {
                sw.WriteLine(data[i] + (i != data.Length - 1 ? ", " : ""));
            }
        }

        private Stream s;
        private StreamReader sr;
        private StreamWriter sw;

        private bool disposed;
    }
}