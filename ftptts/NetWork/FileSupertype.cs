using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    abstract public class FileSupertype
    {
        public int taskId = 0;
        public string filePath = "/";
        public ActionType source_dest = ActionType.Source;
        public AddStrategyClass addStrategy = new AddStrategyClass();
        public int errorCount = 1;
        public int sleepBeforeNext = 1000;
        protected MyProxy prx = new MyProxy();
        protected ActionResult statistics = new ActionResult();
                
        //protected DateTime runDT;
        //protected bool isSuccess;

        private string calculatedFilePath;
        public string CalculatedFilePath
        {
            get 
            {
                if (String.IsNullOrEmpty(this.calculatedFilePath))
                    this.calculatedFilePath = RegexpParsing.UTCDataInserting(filePath, this.addStrategy.GetWorkDate());
                return this.calculatedFilePath;
            }
        }        

        public static FileSupertype fileSupertipeFactory(string type)
        {
            switch (type)
            {
                case "FTP":
                    return new FtpFile();
                case "WinFS":
                    return new WinFSFile();
                default:
                    return new WinFSFile();
            }
        }

        /// <summary>
        /// Запускает выполнение подзадачи
        /// </summary>
        /// <param name="_prx">Настройки прокси</param>
        /// <returns>Возвращает статистику по проведенной операции</returns>
        public ActionResult StartFileAction(MyProxy _prx)
        {
            this.statistics.StartAction();
            this.prx = _prx;
            try
            {
                if (this.source_dest == ActionType.Source)
                {
                    this.statistics.actType = ActionType.Source;
                    this.ActionAsSource();                    
                }
                else
                {
                    this.statistics.actType = ActionType.Destination;
                    this.ActionAsDestination();
                }
                return this.statistics;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {                
                //i_resulter.AddAction(this.statistics);
            }
        }

        public abstract void ActionAsSource();        
        public abstract void ActionAsDestination();
        public abstract ConnectionType GetConnectionType();
    }    

    public struct FileStructure
    {
        
        public string fullpath;
        public string name;
        public DateTime creationTime;

        public FileStructure(string _fullpath, string _name, DateTime _creationTime)
        {
            this.fullpath = _fullpath;
            this.name = _name;
            this.creationTime = _creationTime;
        }
    }
}
