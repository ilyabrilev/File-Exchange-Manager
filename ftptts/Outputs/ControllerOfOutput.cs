using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    public class ControllerOfOutput
    {
        List<OutputSupertype> critMessages = new List<OutputSupertype>();
        List<OutputSupertype> averageMessages = new List<OutputSupertype>();
        List<OutputSupertype> progressMessages = new List<OutputSupertype>();
        List<OutputSupertype> errorMessages = new List<OutputSupertype>();
        List<OutputSupertype> emptyMessages = new List<OutputSupertype>();
        private DateTime SESSIONDATE;
        bool testmode;
        
        private static ControllerOfOutput instance;
        public static ControllerOfOutput Instance
        {
            get { return instance ?? (instance = new ControllerOfOutput(false)); }
        }

        public ControllerOfOutput(bool _testmode)
        {
            this.testmode = _testmode;
            SESSIONDATE = GlobalUtils.GetNowUtcDateTime();

            if (!this.testmode)
            {
                OutputSupertype email = new OutputEmail();
                OutputSupertype allLogFile = new OutputFile("\\alllog.txt");
                OutputSupertype errorLogFile = new OutputFile("\\errorlog.txt");
                OutputSupertype console = new OutputConsole();

                critMessages.Add(email);
                critMessages.Add(allLogFile);
                critMessages.Add(console);

                averageMessages.Add(allLogFile);
                averageMessages.Add(console);

                progressMessages.Add(console);

                errorMessages.Add(errorLogFile);
                errorMessages.Add(console);
                errorMessages.Add(allLogFile);
                errorMessages.Add(email);
            }
            else
            {
                OutputSupertype console = new OutputConsole();
                critMessages.Add(console);
                averageMessages.Add(console);
                progressMessages.Add(console);
                errorMessages.Add(console);
            }
        }

        public void WriteCritMessage(string msg) 
        {
            WriteMessage(msg, "Ошибка приложения", this.critMessages);
        }

        public void WriteAverageMessage(string msg)
        {
            WriteMessage(msg, "", this.averageMessages);
        }

        public void WriteProgress(string msg)
        {
            WriteMessage(msg, "", this.progressMessages);
        }

        public void WriteErrors(string msg)
        {
            WriteMessage(msg, "", this.errorMessages);
        }

        private void WriteMessage(string msg, string title, List<OutputSupertype> MessagesManager)
        {
            foreach (OutputSupertype output in MessagesManager)
            {
                output.WriteLog(msg, title);
            }
        }
        
        public void WorkIsOver2(ResultController rc)
        {
            string msg = rc.GenerateMessage();
            string title = rc.GenereateTitle();

            List<OutputSupertype> ooo = null;
            if (rc.globalWorkRes == WorkResult.NothingHappens)
            {
                //ooo = this.critMessages;
                ooo = this.averageMessages;
            }
            else
            {
                ooo = this.critMessages;
            }
                        
            if (this.testmode)
                ooo = this.emptyMessages;            

            foreach (OutputSupertype output in ooo)
            {
                output.WorkIsOver(msg, title);
            }
        }
    }
}
