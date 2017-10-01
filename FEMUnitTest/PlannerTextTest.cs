using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace File_Exchange_Manager.UnitTest
{
    [TestFixture]
    class PlannerTextTest
    {
        [Test]
        public void StartExchange_ComplicatedSession_ErrorRes()
        {
            PlannerText pln = this.GetPlannerText();

            PlannerTask tmpTask1 = new PlannerTask(false);
            tmpTask1.id = 1;
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Success, false, ActionType.Source));
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Success, false, ActionType.Destination));
            pln.tasks.Add(tmpTask1);

            PlannerTask tmpTask2 = new PlannerTask(false);
            tmpTask2.id = 2;
            tmpTask2.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Error, false, ActionType.Source));
            tmpTask2.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Error, false, ActionType.Destination));
            pln.tasks.Add(tmpTask2);

            ResultController rc = pln.StartExchange();

            Assert.AreEqual(rc.globalWorkRes, WorkResult.Error);
            Assert.AreEqual(rc.GetAllFiles(ActionType.Source), 2);
            Assert.AreEqual(rc.GetAllFiles(ActionType.Destination), 4);
            Assert.AreEqual(rc.GetAllBytes(ActionType.Source), 6);
            Assert.AreEqual(rc.GetAllBytes(ActionType.Destination), 8);
        }

        [Test]
        public void StartExchange_SuccessSession_Success()
        {
            PlannerText pln = this.GetPlannerText();

            PlannerTask tmpTask1 = new PlannerTask(false);
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.Success, false, ActionType.Source));
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.Success, false, ActionType.Destination));
            pln.tasks.Add(tmpTask1);

            ResultController rc = pln.StartExchange();

            Assert.AreEqual(rc.globalWorkRes, WorkResult.Success);
            Assert.AreEqual(rc.GetAllFiles(ActionType.Source), 1);
            Assert.AreEqual(rc.GetAllFiles(ActionType.Destination), 1);
            Assert.AreEqual(rc.GetAllBytes(ActionType.Source), 2);
            Assert.AreEqual(rc.GetAllBytes(ActionType.Destination), 2);
        }

        [Test]
        public void StartExchange_FakeTask_NothingHappens()
        {
            PlannerText pln = this.GetPlannerText();

            PlannerTask tmpTask1 = new PlannerTask(true);
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.Success, false, ActionType.Source));
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.Success, false, ActionType.Destination));
            pln.tasks.Add(tmpTask1);

            ResultController rc = pln.StartExchange();
                        
            Assert.AreEqual(rc.globalWorkRes, WorkResult.NothingHappens);
        }

        [Test]
        public void StartExchange_NothingHappensFiles_NothingHappens()
        {
            PlannerText pln = this.GetPlannerText();

            PlannerTask tmpTask1 = new PlannerTask(false);
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.NothingHappens, false, ActionType.Source));
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.NothingHappens, false, ActionType.Destination));
            pln.tasks.Add(tmpTask1);

            ResultController rc = pln.StartExchange();

            Assert.AreEqual(rc.globalWorkRes, WorkResult.NothingHappens);
        }

        [Test]
        public void StartExchange_TestLos_10110()
        {
            ControllerOfOutput co = new ControllerOfOutput(true);
            FakeLogOfSuccession los = new FakeLogOfSuccession(false);
            PlannerText pln = new PlannerText(co, los);
            
            PlannerTask tmpTask1 = new PlannerTask(false);
            tmpTask1.id = 1;
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.Success, false, ActionType.Source));
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 2, WorkResult.Success, false, ActionType.Destination));
            pln.tasks.Add(tmpTask1);
            
            PlannerTask tmpTask2 = new PlannerTask(false);
            tmpTask2.id = 2;
            tmpTask2.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Error, false, ActionType.Source));
            tmpTask2.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Error, false, ActionType.Destination));
            pln.tasks.Add(tmpTask2);

            PlannerTask tmpTask3 = new PlannerTask(false);
            tmpTask3.id = 3;
            tmpTask3.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Other, false, ActionType.Source));
            tmpTask3.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Other, false, ActionType.Destination));
            pln.tasks.Add(tmpTask3);

            PlannerTask tmpTask4 = new PlannerTask(false);
            tmpTask4.id = 4;
            tmpTask4.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.NothingHappens, false, ActionType.Source));
            tmpTask4.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.NothingHappens, false, ActionType.Destination));
            pln.tasks.Add(tmpTask4);

            PlannerTask tmpTask5 = new PlannerTask(false);
            tmpTask5.id = 5;
            tmpTask5.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Success, false, ActionType.Source));
            tmpTask5.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Success, false, ActionType.Source));
            tmpTask5.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Error, false, ActionType.Source));
            tmpTask5.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Success, false, ActionType.Destination));
            tmpTask5.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Success, false, ActionType.Destination));
            tmpTask5.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Other, false, ActionType.Destination));
            pln.tasks.Add(tmpTask5);

            ResultController rc = pln.StartExchange();

            Assert.True(los.boolResultSaved[0]);
            Assert.False(los.boolResultSaved[1]);
            Assert.True(los.boolResultSaved[2]);
            Assert.True(los.boolResultSaved[3]);
            Assert.False(los.boolResultSaved[4]);
        }

        public void StartExchange_IsThereCanBeOnlySourceTasks()
        {
            PlannerText pln = this.GetPlannerText();

            PlannerTask tmpTask1 = new PlannerTask(false);
            tmpTask1.id = 1;
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Success, false, ActionType.Source));
            tmpTask1.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Success, false, ActionType.Source));
            pln.tasks.Add(tmpTask1);

            PlannerTask tmpTask2 = new PlannerTask(false);
            tmpTask2.id = 2;
            tmpTask2.uploadDownload.Add(new FileSupertypeFake(1, 3, WorkResult.Error, false, ActionType.Source));
            tmpTask2.uploadDownload.Add(new FileSupertypeFake(2, 4, WorkResult.Error, false, ActionType.Destination));
            pln.tasks.Add(tmpTask2);

            ResultController rc = pln.StartExchange();

            Assert.AreEqual(rc.globalWorkRes, WorkResult.Error);
            Assert.AreEqual(rc.GetAllFiles(ActionType.Source), 2);
            Assert.AreEqual(rc.GetAllFiles(ActionType.Destination), 4);
            Assert.AreEqual(rc.GetAllBytes(ActionType.Source), 6);
            Assert.AreEqual(rc.GetAllBytes(ActionType.Destination), 8);
        }

        public PlannerText GetPlannerText()
        {
            ControllerOfOutput co = new ControllerOfOutput(true);
            FakeLogOfSuccession los = new FakeLogOfSuccession(false);
            PlannerText ret = new PlannerText(co, los);    
            return ret;
        }
    }
}
