using System;
using System.IO;

namespace PSP.Models
{
    public class LogWriter : IDisposable
    {
        public void Dispose() { GC.SuppressFinalize(this); }

        public static void Write(string pMessage)
        {
            string strDirectory = "C:\\Payroll Files\\Logs\\";
            string strFilename = strDirectory + "PYCTB_Logs.txt";

            if (!Directory.Exists(strDirectory)) { Directory.CreateDirectory(strDirectory); }

            using (FileStream fs = new FileStream(strFilename, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine(pMessage);
                    sw.Close();
                }
                fs.Close();
            }
        }

    }
}
