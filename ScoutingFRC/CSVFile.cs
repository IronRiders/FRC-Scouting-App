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
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

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

        void WriteData(params object[] data)
        {
            for (int i = 0; i < data.Length; ++i) {
                sw.WriteLine(data[i] + (i != data.Length - 1 ? ", " : ""));
            }
        }

        //Not working yet, also not needed as of now.
        /*string[] ReadLine()
        {
            List<string> result = new List<string>();

            var line = sr.ReadLine();

            int tokenStart = 0;
            bool inQuotes = false;

            for (int i = 0; i < line.Length; ++i) {
                char c = line[i];
                if (c == ',') {
                    if(!inQuotes || i == line.Length - 1) {
                        result.Add(line.Substring(tokenStart, i - tokenStart));
                        tokenStart = i + 1;
                    }
                }
                else if (c == '\"') {
                    inQuotes = !inQuotes;
                }
            }

            return null;
        }

        string[][] ReadAllData()
        {
            List<string[]> result = new List<string[]>();

            long pos = s.Position;
            s.Position = 0;
            sr.DiscardBufferedData();

            string[] line;
            while ((line = ReadLine()) != null) {
                result.Add(line);
            }

            s.Position = pos;
            sr.DiscardBufferedData();

            return result.ToArray();
        }*/

        private Stream s;
        private StreamReader sr;
        private StreamWriter sw;

        private bool disposed;
    }
}