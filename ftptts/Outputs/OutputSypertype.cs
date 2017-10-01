using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    abstract public class OutputSupertype
    {
        abstract public void WriteLog(string msg, string title);
        abstract public void WorkIsOver(string msg, string title);
    }
}
