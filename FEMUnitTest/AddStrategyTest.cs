using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace File_Exchange_Manager.UnitTest
{
    [TestFixture]
    class AddStrategyTest
    {
        [Test]
        public void FileVerification_SimpleFile_True()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            Assert.True(strat.FileVerification("abc.cba", DateTime.Now));
        }

        [Test]        
        public void FileVerification_MaskedFileWithoutCreationDate_True()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = new DateTime(2016, 03, 25);
            strat.useCreationDate = false;
            strat.fileMask = "%" + useDate.ToString("ddMMyyyy") + "%a.txt";            

            Assert.True(strat.FileVerification("25032016a.txt", useDate));
        }

        [Test]
        public void FileVerification_MaskedFileUseCreationDate_True()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = DateTime.Now;
            string dateStr = useDate.ToString("ddMMyyyy");
            strat.fileMask = "%" + dateStr + "%a.txt";

            Assert.True(strat.FileVerification(dateStr + "a.txt", useDate));
        }
        
        [Test]
        public void FileVerification_FailBecauseMaskWrongCDSkip_False()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = new DateTime(2016, 03, 25);

            string dateStr = useDate.ToString("dd MM yyyy");
            strat.fileMask = "%" + dateStr + "%a.txt";
            strat.useCreationDate = false;

            Assert.False(strat.FileVerification(dateStr + "1232a.txt", useDate));
        }

        [Test]
        public void FileVerification_FailBecauseCreationDateIsWrong_False()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = new DateTime(2015, 03, 25);
                        
            strat.fileMask = "a.txt";            

            Assert.False(strat.FileVerification("a.txt", useDate));
        }

        [Test]
        public void FileVerification_WorkIntervalDays_True()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = DateTime.Now;

            strat.fileMask = "a.txt";
            strat.workInterval = eWorkInterval.Days;
            strat.fileNameUsage = "";

            Assert.True(strat.FileVerification("a.txt", useDate));
        }

        [Test]
        public void FileVerification_WorkIntervalMonths_True()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = DateTime.Now;

            strat.fileMask = "a.txt";
            strat.workInterval = eWorkInterval.Months;
            strat.fileNameUsage = "";

            Assert.True(strat.FileVerification("a.txt", useDate));
        }

        [Test]
        public void FileVerification_WorkIntervalYears_True()
        {
            AddStrategyClass strat = this.GetTestStrategy();
            DateTime useDate = DateTime.Now;

            strat.fileMask = "a.txt";
            strat.workInterval = eWorkInterval.Years;
            strat.fileNameUsage = "";

            Assert.True(strat.FileVerification("a.txt", useDate));
        }

        //конструктор тестируемого объекта
        public AddStrategyClass GetTestStrategy()
        {
            AddStrategyClass ret = new AddStrategyClass();

            ret.dateRange = eDateRange.Now;
            ret.fileMask = "???.???";
            ret.fileNameUsage = "Masked";
            ret.useCreationDate = true;
            ret.workInterval = eWorkInterval.Days;
            ret.output = new ControllerOfOutput(true);

            return ret;
        }
    }
}
