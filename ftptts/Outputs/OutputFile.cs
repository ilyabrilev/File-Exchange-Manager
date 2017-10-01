using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace File_Exchange_Manager
{
    public class OutputFile : OutputSupertype
    {
        public StreamWriter file;        
        public string path;

        public OutputFile(string filename) 
        {
            //filename = "\\log.txt";
            NewLogfile(Directory.GetCurrentDirectory() + filename);
        }

        public override void WriteLog(string msg, string title)
        {
            if (this.file != null)
                this.file.WriteLine(GlobalUtils.GetNowUtcDateTime().ToString("yyyy.MM.dd HH:mm:ss") + "\t" + msg);
            //Console.WriteLine(msg);
        }

        public void NewLogfile(string logPath)
        {
            if (!File.Exists(logPath))
            {
                File.Create(logPath);
                //Thread.Sleep(1000);
            }

            this.path = logPath;
            try
            {
                this.file = new StreamWriter(logPath, true);
            }
            catch (Exception)
            {
                this.file = null;
            } 
        }

        public override void WorkIsOver(string msg, string title)
        {
            this.WriteLog(msg, title);
            this.AddSep();
            this.CloseFile();
        }

        public void CloseFile()
        {
            if (this.file != null)
                this.file.Close();
            this.file.Close();
        }

        public void AddSep()
        {
            if (this.file != null)
                this.file.WriteLine("\n");
        }
    }
}
