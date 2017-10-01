using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace File_Exchange_Manager
{
    public class AddStrategyClass
    {
        /// <summary>
        /// Использовать ли маску файла при верификации
        /// </summary>
        public bool usemask = true;

        /// <summary>
        /// Wildcard файла
        /// </summary>
        public string fileMask = "";

        /// <summary>
        /// Days/Months/Years - участвует в подстановке даты в путь и имя файла, а также при выборе через дату создания 
        /// </summary>
        public eWorkInterval workInterval = eWorkInterval._Other;

        /// <summary>
        /// Masked/Other - использовать ли маску при верификации
        /// </summary>
        public string fileNameUsage = "";

        /// <summary>
        /// Now/Prev/Percise Определяет, какая дата будет использоваться для верификации и подстановки имени
        /// </summary>
        public eDateRange dateRange = eDateRange._Other;

        /// <summary>
        /// Если dateRange равен Percise, то в качестве раюочего интервала используетися эта дата
        /// </summary>
        public DateTime perciseEnterDateTime = new DateTime();
    
        /// <summary>
        /// пока не используется...
        /// </summary>
        public string checkDateCondition = "";

        /// <summary>
        /// Использвать ли дату создания при 
        /// </summary>
        public bool useCreationDate = false;

        public ControllerOfOutput output = null;

        private string calculatedFileMask;
        public string CalculatedFileMask
        {
            get
            {
                if (String.IsNullOrEmpty(this.calculatedFileMask))
                    this.calculatedFileMask = RegexpParsing.UTCDataInserting(this.fileMask, this.GetWorkDate());
                return this.calculatedFileMask;
            }
        }

        public AddStrategyClass(ControllerOfOutput _output)
        {
            this.output = _output;
        }

        //toDo Синхронизировать с наполнением класса
        public AddStrategyClass(AddStrategyClass cln)
            : this(ControllerOfOutput.Instance)
        {
            this.usemask = cln.usemask;
            this.workInterval = cln.workInterval;
            this.fileMask = cln.fileMask;            
            this.fileNameUsage = cln.fileNameUsage;
            this.dateRange = cln.dateRange;
            //this.workDateTime = cln.workDateTime;
            this.checkDateCondition = cln.checkDateCondition;
            this.useCreationDate = cln.useCreationDate;
        }

        public AddStrategyClass()
            : this(ControllerOfOutput.Instance)
        {
        }

        //workDateTime участвует в подстановке даты в путь и имя файла, а также при выборе через дату создания
        private DateTime workDateTime;
        private bool isWorkDTComputed = false;
        public DateTime GetWorkDate()
        {
            if (this.isWorkDTComputed)
                return this.workDateTime;

            //сначала вычислим рабочую дату
            switch (this.dateRange)
            {
                case eDateRange.Now:
                    this.workDateTime = GlobalUtils.GetNowUtcDateTime();
                    break;
                case eDateRange.Prev:
                    this.workDateTime = this.ComputePrevDate(this.workInterval);
                    break;
                default:
                    this.workDateTime = new DateTime();
                    break;
            }

            this.isWorkDTComputed = true;
            return this.workDateTime;
        }

        public DateTime GetUtcWorkDate()
        {
            return this.GetWorkDate().ToUniversalTime();
        }        
        
        /// <summary>
        /// Проверка, подходит ли файл для передачи с помощью текущей стратегии добавления
        /// </summary>
        /// <param name="filename">Имя файла</param>
        /// <param name="creationTime">Дата создания файла</param>
        /// <returns>true - подходит для передачи, false - не подходит</returns>
        public bool FileVerification(string filename, DateTime creationTime)
        {
            this.output = ControllerOfOutput.Instance;

            if (this.useCreationDate)
            {
                DateTime cCreatTime = this.CutDateTime(creationTime.ToUniversalTime());
                DateTime cWorkDateTime = this.CutDateTime(this.GetUtcWorkDate());
                if (cCreatTime != cWorkDateTime)
                {
                    output.WriteProgress(
                        String.Format("Файл {0} не передавался. Не подходящая дата создания {1}. Искали с датой {2}",
                        filename, cCreatTime.ToString("yyyy:MM:dd"), cWorkDateTime.ToString("yyyy:MM:dd")));
                    return false;
                }
            }            

            if (this.fileNameUsage == "Masked")
            {
                bool regRes = RegexpParsing.FilenameVerification(filename, this.CalculatedFileMask);
                if (!regRes)
                {
                    output.WriteAverageMessage(String.Format("Файл {0} не подходит под маску {1}.",
                        filename, this.CalculatedFileMask));
                }
                return regRes;
            }

            return true;
        }

        /// <summary>
        /// Удаляет несущественную часть даты в зависимости от значения параметра workInterval
        /// </summary>
        /// <param name="toCut"></param>
        /// <returns></returns>
        private DateTime CutDateTime(DateTime toCut)
        {
            switch (this.workInterval)
            {
                case eWorkInterval.Days:
                    return new DateTime(toCut.Year, toCut.Month, toCut.Day);
                case eWorkInterval.Months:
                    return new DateTime(toCut.Year, toCut.Month, 1);
                case eWorkInterval.Years:
                    return new DateTime(toCut.Year, 1, 1);                    
                default:
                    return toCut;
            }
        }

        private DateTime ComputePrevDate(eWorkInterval _int)
        {
            switch (this.workInterval)
            {
                case eWorkInterval.Days:
                    return GlobalUtils.GetNowUtcDateTime().AddDays(-1);                    
                case eWorkInterval.Months:
                    return GlobalUtils.GetNowUtcDateTime().AddMonths(-1);                    
                case eWorkInterval.Years:
                    return GlobalUtils.GetNowUtcDateTime().AddYears(-1);                    
                default:
                    return new DateTime();
            }
        }
    }

    public enum eWorkInterval { Days, Months, Years, _Other };
    public enum eDateRange { Now, Prev, Percise, _Other };
}
