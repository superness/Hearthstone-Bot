using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HearthstoneBot
{
    public class FileLogger : Singleton<FileLogger>
    {
        public String LogPath
        {
            get;
            set;
        }

        public FileLogger()
        {
            this.LogPath = "HSBot.log";
        }

        public void LogLine(String line)
        {

            // This text is added only once to the file. 
            if (!File.Exists(this.LogPath))
            {
                // Create a file to write to. 
                using (StreamWriter sw = File.CreateText(this.LogPath))
                {
                    sw.WriteLine("~~~~~Hearthstone Bot Log~~~~~");
                }
            }

            // This text is always added, making the file longer over time 
            // if it is not deleted. 
            using (StreamWriter sw = File.AppendText(this.LogPath))
            {
                sw.WriteLine(line);
            }	
        }
    }
}
