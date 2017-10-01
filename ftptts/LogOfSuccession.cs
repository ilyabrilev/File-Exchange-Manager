using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace File_Exchange_Manager
{
    public class LogOfSuccession
    {
        private string allLogPath = Directory.GetCurrentDirectory() + "\\allexec.txt";
        private string lastLogPath = Directory.GetCurrentDirectory() + "\\lastexec.txt";
        public bool checkStatus = false;

        private static LogOfSuccession instance;
        public static LogOfSuccession Instance
        {
            get { return instance ?? (instance = new LogOfSuccession()); }
        }

        public List<SavedTaskResults> prevRes = new List<SavedTaskResults>();
        //задачи сохранются в эту переменную, а затем из этой переменной сохраняются в файл
        public List<SavedTaskResults> resToSave = new List<SavedTaskResults>();

        public LogOfSuccession()
        {
            this.CheckFileCreate(this.lastLogPath);
            this.CheckFileCreate(this.allLogPath);

            this.Init();
        }

        private void Init()
        {
            string[] sepFields = new string[] { " " };
            string[] sepDate = new string[] { "." };
            string[] sepTime = new string[] { ":" };
                        
            StreamReader myStream = null;
            try
            {
                myStream = new StreamReader(this.lastLogPath);
            }
            catch (Exception ex)
            {
                this.checkStatus = false;
                ControllerOfOutput coo = ControllerOfOutput.Instance;
                coo.WriteErrors("Ошибка при попытке открыть файл логов работы. Текст ошибки" + ex.Message);
                return;
            }
                
            while (!myStream.EndOfStream)
            {
                string strToParse = myStream.ReadLine();
                string[] parsingStr = strToParse.Split(sepFields, StringSplitOptions.RemoveEmptyEntries);
                    
                try
                {
                    List<int> parsingStrDate = StrArrToIntArr(parsingStr[1].Split(sepDate, StringSplitOptions.RemoveEmptyEntries));
                    List<int> parsingStrTime = StrArrToIntArr(parsingStr[2].Split(sepTime, StringSplitOptions.RemoveEmptyEntries));

                    SavedTaskResults tmpTsk = new SavedTaskResults();
                    tmpTsk.id = int.Parse(parsingStr[0]);
                    tmpTsk.lastRun = new DateTime(parsingStrDate[2], parsingStrDate[1], parsingStrDate[0], parsingStrTime[0], parsingStrTime[1], parsingStrTime[2]);
                    tmpTsk.isSuccess = bool.Parse(parsingStr[3]);
                    this.prevRes.Add(tmpTsk);
                }
                catch (Exception)
                {
                    continue;
                }
            }
            myStream.Close();
            this.checkStatus = true;
        }

        private List<int> StrArrToIntArr(string[] enterStr)
        {
            List<int> ret = new List<int>();
            foreach (string item in enterStr)
            {
                ret.Add(int.Parse(item));
            }
            return ret;
        }
                        
        //Проверка задания на тему того, надо ли запускать ее еще раз
        //false, если задачу надо запустить еще раз
        public virtual bool CheckTaskExecute(int taskID, string taskPeriod)
        {
            ControllerOfOutput output = ControllerOfOutput.Instance;
            //находим нужное задание по id
            SavedTaskResults workRes = prevRes.Find(x => x.id == taskID);
            if (workRes == null)
            {
                output.WriteAverageMessage("Задание " + taskID + " отсутствует в списке. Начинаем выполнение.");
                return false;
            }
            //исключительная ситуация, требующая дополнительного внимания: когда задание выполнилось,
            //затем пропало из планировщика,  затем снова появилось в планировщике.
            //оно будет выполнено еще раз, как и задания с изменившимся id
            //добавление этого таска нужно чтобы переносить уже сделанные задачи на следующую проверку
            resToSave.Add(workRes);
            if (!workRes.isSuccess)
            {
                output.WriteAverageMessage("Предыдущее выполнение задания " + taskID + 
                    " завершилось ошибкой. Начинаем выполнение.");
                return false;
            }
            bool periodRes = PeriodFitting(workRes, taskPeriod);

            if (periodRes)
            {
                output.WriteAverageMessage("Задача " + taskID +
                    " уже успешно выполнялась в периоде " + taskPeriod +
                    ". Последнее время выполнения: " + workRes.lastRun.ToString() + ".");
            }
            else
            {
                output.WriteAverageMessage("Задача " + taskID + " будет выполнена.");
            }
            return periodRes;
        }

        //проверяет, нужно ли запускать задачу исходя из времени последнего запуска и периодичности 
        private bool PeriodFitting(SavedTaskResults task, string taskPeriod)
        {
            DateTime taskCutDT = this.CutDateTime(task.lastRun, taskPeriod);
            DateTime nowCutDT = this.CutDateTime(GlobalUtils.GetNowUtcDateTime(), taskPeriod);
            return (taskCutDT == nowCutDT);
        }

        //избыточный метод, который обрезает ненужную часть даты
        private DateTime CutDateTime(DateTime toCut, string taskPeriod)
        {
            switch (taskPeriod)
            {
                case "Hour":
                    return new DateTime(toCut.Year, toCut.Month, toCut.Day, toCut.Hour, toCut.Minute, toCut.Day);
                case "Day":
                    return new DateTime(toCut.Year, toCut.Month, toCut.Day);
                case "Month":
                    return new DateTime(toCut.Year, toCut.Month, 1);
                case "Year":
                    return new DateTime(toCut.Year, 1, 1);                
                default:
                    return toCut;
            }
        }

        //сохраняем результаты текущей задачи в списке результатов
        public virtual void SaveTaskResult(int _id, bool _isSuccess, DateTime _whenItStarted)
        {            
            SavedTaskResults workRes = prevRes.Find(x => x.id == _id);
            //если в предыдущей строке задача нашлась, она затерается новой
            if (workRes != null)
                resToSave.Remove(workRes);
            workRes = new SavedTaskResults();
            workRes.id = _id;
            workRes.isSuccess = _isSuccess;
            workRes.lastRun = _whenItStarted;
            resToSave.Add(workRes);
        }

        //когда алгоритм заканчивает работу, записываются результаты всех обработок
        public virtual bool WorkIsOver()
        {
            try
            {
                StreamWriter lastLogFile = new StreamWriter(this.lastLogPath, false);
                StreamWriter allLogFile = new StreamWriter(this.allLogPath, true);

                foreach (SavedTaskResults tsk in this.resToSave)
                {
                    string saveStr = tsk.id + " " + tsk.lastRun.ToString() + " " + tsk.isSuccess.ToString();
                    lastLogFile.WriteLine(saveStr);
                    allLogFile.WriteLine(saveStr);
                }                
                allLogFile.WriteLine("<--------------------->");

                lastLogFile.Close();
                allLogFile.Close();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        private bool CheckFileCreate(string path)
        {
            if (!File.Exists(path))
            {
                try
                {
                    File.Create(path);
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class SavedTaskResults
    {
        public int id;
        public DateTime lastRun;
        public bool isSuccess;
    }

    public class FakeLogOfSuccession : LogOfSuccession
    {
        public bool checkRes = false;
        public List<string> resultsSaved = new List<string>();
        public List<bool> boolResultSaved = new List<bool>();

        public FakeLogOfSuccession(bool _checkRes)
        {
            this.checkRes = _checkRes;
        }

        public FakeLogOfSuccession()
        {
        }

        public override bool WorkIsOver()
        {
            return true;
        }

        public override void SaveTaskResult(int _id, bool _isSuccess, DateTime _whenItStarted)
        {
            this.resultsSaved.Add(_id.ToString() + " " + _isSuccess.ToString() + " " + _whenItStarted.ToString());
            this.boolResultSaved.Add(_isSuccess);
        }

        public override bool CheckTaskExecute(int taskID, string taskPeriod)
        {
            return this.checkRes;
        }
    }
}