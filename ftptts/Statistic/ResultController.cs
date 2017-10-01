using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    public class ResultController : IResulter
    {
        //В этой переменной - строка типа "Выполнение сессии" или "Выполнение задачи N"
        public string discription = "";

        public WorkResult globalWorkRes = WorkResult.NothingHappens;
        Dictionary<ActionType, ActionResult> resultsByTypes = new Dictionary<ActionType, ActionResult>();
        public List<ActionResult> innerResults = new List<ActionResult>();
        public List<ResultController> innerControllers = new List<ResultController>();

        public ResultController(string _desc)
        {
            this.discription = _desc;
            //создаем объект результатов запуска для каждого типа
            foreach (ActionType act in Enum.GetValues(typeof(ActionType)))
            {
                ActionResult ar = new ActionResult();                
                resultsByTypes.Add(act, ar);
            }
        }

        public void AddInnerController(ResultController rc)
        {
            this.innerControllers.Add(rc);
            this.globalWorkRes = ActionResult.CompareTwoResults(this.globalWorkRes, rc.globalWorkRes);
            foreach (KeyValuePair<ActionType, ActionResult> resTypes in rc.resultsByTypes)
            {
                this.resultsByTypes[resTypes.Key].MergeResults(resTypes.Value);
            }
        }

        //завершаем операцию, записываем дату завершения и добавляем итоги операции в массив
        public void AddAction(ActionResult ar)
        {
            ar.StopAction();
            innerResults.Add(ar);
            this.resultsByTypes[ar.actType].MergeResults(ar);

            this.globalWorkRes = ActionResult.CompareTwoResults(this.globalWorkRes, ar.result);
        }

        //генерируем длинное сообщение об итогах
        public string GenerateMessage()
        {
            string message = "\n\n" + this.discription + "\n"; 

            if (this.globalWorkRes == WorkResult.NothingHappens)
            {
                message += "Ни одна из задач не запускалась";
            }
            if (this.globalWorkRes == WorkResult.Success)
            {
                message += "Успешное выполнение " +
                    this.resultsByTypes[ActionType.Source].startDate.ToString("yyyy.MM.dd HH.mm.ss") + ".\n";
                message += this.GenerateStatistic();
            }
            if (this.globalWorkRes == WorkResult.Error)
            {
                message += "Ошибка выполнения " +
                    this.resultsByTypes[ActionType.Source].startDate.ToString("yyyy.MM.dd HH.mm.ss") + ".\n";
                message += this.GenerateStatistic();
            }

            return message;
        }

        public string GenerateStatistic()
        {
            bool ifItHasInnerControllers = innerControllers.Count > 0;

            string message = "";
            message += "Статистика загрузки: \n";
            message += this.resultsByTypes[ActionType.Source].GenerateStatistic(ActionType.Source, ifItHasInnerControllers);
            message += "\nСтатистика отправки: \n";
            message += this.resultsByTypes[ActionType.Destination].GenerateStatistic(ActionType.Destination, ifItHasInnerControllers);
            foreach (ResultController rc in innerControllers)
            {
                message += rc.GenerateMessage();
            }
            return message;
        }

        //генерируем заголовок
        public string GenereateTitle()
        {
            string title = "";

            if (this.globalWorkRes == WorkResult.NothingHappens)
                title = "Никакие задачи не запускались";
            if (this.globalWorkRes == WorkResult.Success)
                title = "Успешное выполнение";
            if (this.globalWorkRes == WorkResult.Error)
                title = "Ошибка выполнения";

            return title;
        }

        public int GetAllFiles(ActionType _type)
        {            
            return resultsByTypes[_type].filesCount;
        }

        public long GetAllBytes(ActionType _type)
        {
            return resultsByTypes[_type].bytesTransfered;
        }
    }

    //чем закончилась работа задачи
    public enum WorkResult { Error = 0, Success = 1, NothingHappens = 2, Other = 3 };
    //тип задачи
    public enum ActionType { Source, Destination, Other };
    //тип связи 
    public enum ConnectionType { WinFSFile, FtpFile, Other };

    public interface IResulter
    {
        void AddAction(ActionResult ar);
        string GenerateMessage();
        string GenereateTitle();
    }
}
