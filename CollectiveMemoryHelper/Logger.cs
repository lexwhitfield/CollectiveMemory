using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectiveMemoryHelper
{

    public static class Logger
    {
        public static string LogFolder = "C:\\Databases\\CMLogs";

        public static void WriteLine(string logfile, string logtext, bool writeTimeStamp, bool throwNewLine)
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);

            string logFilePath = Path.Combine(LogFolder, logfile + " " + DateTime.Now.ToLongDateString() + ".txt");

            if (writeTimeStamp)
            logtext = DateTime.Now.ToLongTimeString() + " :: " + logtext;

            if (throwNewLine)
                File.AppendAllLines(logFilePath, new List<string> { logtext });
            else
                File.AppendAllText(logFilePath, logtext);
        }
    }
}
