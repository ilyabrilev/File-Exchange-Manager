using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    public class OutputConsole : OutputSupertype
    {
        public override void WriteLog(string msg, string title)
        {
            Console.WriteLine(msg);
        }

        public override void WorkIsOver(string msg, string title)
        {
            Console.WriteLine(msg);
        }
    }
}
