using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    public class ActionResult
    {
        public WorkResult result = WorkResult.NothingHappens;
        public ActionType actType = ActionType.Other;
        public ConnectionType conType = ConnectionType.Other;
        public DateTime startDate = new DateTime();
        public DateTime stopDate = GlobalUtils.GetNowUtcDateTime();

        public int filesCount = 0;
        public long bytesTransfered = 0;        
        private List<string> errorsList = new List<string>();
        private List<string> messageList = new List<string>();
        private DateTime defaultDateTime = new DateTime();

        public ActionResult()
        {
        }

        public ActionResult(ActionType _actType, ConnectionType _conType)
        {
            this.actType = _actType;
            this.conType = _conType;
        }

        public static WorkResult CompareTwoResults(WorkResult retRes, WorkResult comparedRes)
        {
            if (comparedRes < retRes)
                return comparedRes;
            else return retRes;
        }

        public static bool IsResultIsNotError(WorkResult wr)
        {
            if (wr != WorkResult.Error)
                return true;
            else return false;
        }

        public string GenerateStatistic(ActionType type, bool hasInnerContr)
        {
            string ret = "";

            if (type == ActionType.Source)            
                ret += "Загружено файлов: " + this.filesCount;            
            else            
                if (type == ActionType.Destination)
                    ret += "Передано файлов: " + this.filesCount;            
            ret += ". Размер: " + this.bytesTransfered + " Кб.";

            int countMe = 1;
            if (this.messageList.Count > 0)
            {
                if (hasInnerContr)
                {
                    ret += "\n" + "Предупреждений в подзадачах:" + this.messageList.Count + ".";
                }
                else
                {
                    ret += "\nСписок предупреждений:";
                    foreach (string msg in this.messageList)
                    {
                        ret += "\n" + countMe++ + ". " + msg;
                    }
                }
            }

            countMe = 1;
            if (this.errorsList.Count > 0)
            {
                if (hasInnerContr)
                {
                    ret += "\n" + "Ошибок в подзадачах:" + this.errorsList.Count + ".";
                }
                else
                {
                    ret += "\nСписок критичных ошибок:";
                    foreach (string err in this.errorsList)
                    {
                        ret += "\n" + countMe++ + ". " + err;
                    }
                }
            }
            else
                ret += "\nКритичных ошибок не обнаружено.";

            return ret;
        }

        //генерируем сообщение об операции
        public string GenerateResultString()
        {
            string ret = "Запуск задачи " + this.startDate.ToString() +
                ". Выполнение задачи: " + this.stopDate.ToString() +
                "Результат выполнения задачи: \n";
            return ret;
        }

        //запускаем
        public void StartAction()
        {
            this.startDate = GlobalUtils.GetNowUtcDateTime();
        }

        //останавливаем
        public void StopAction()
        {
            this.stopDate = GlobalUtils.GetNowUtcDateTime();
        }

        public void incrementFiles(int _files)
        {
            this.filesCount += _files;
        }

        public void incrementBytes(long _bytes)
        {
            this.bytesTransfered += _bytes;
                //(long)Math.Round((double)_bytes / 1000, 0);
        }

        public void incrementBytesWithCut(long _bytes)
        {
            this.bytesTransfered += (long)Math.Round((double)_bytes / 1000, 0);
        }

        public void addError(string err)
        {
            this.errorsList.Add(err);
            this.result = WorkResult.Error;
        }

        public void addMessage(string err)
        {
            this.messageList.Add(err);            
        }

        public void addResult(WorkResult wr)
        {
            this.result = ActionResult.CompareTwoResults(this.result, wr);
        }

        //функция-слияние двух результатов выполнения задачи        
        public void MergeResults(ActionResult _res)
        {
            ActionResult current = this;

            current.incrementFiles(_res.filesCount);
            current.incrementBytes(_res.bytesTransfered);

            current.result = ActionResult.CompareTwoResults(current.result, _res.result);

            if (current.startDate == this.defaultDateTime)
                current.startDate = _res.startDate;

            if (current.stopDate < _res.stopDate)
                current.stopDate = _res.stopDate;

            //ToDo: надо бы понять, стоит ли клонировать сообщения об ошибках или нет
            foreach (var msg in _res.messageList)
            {
                current.messageList.Add(msg);
            }

            foreach (var err in _res.errorsList)
            {
                current.errorsList.Add(err);
            }
        }
    }
}
