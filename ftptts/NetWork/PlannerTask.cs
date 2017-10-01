using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace File_Exchange_Manager
{
    public class PlannerTask
    {
        public int id = 0;
        public string taskDiscription = "";
        public bool isFake = true;
        public bool deleteTmp = true;
        public string period = "";
        public List<CmdRoute> taskRouts = new List<CmdRoute>();

        public List<FileSupertype> uploadDownload = new List<FileSupertype>();

        public PlannerTask()
        {
        }

        public PlannerTask(bool _isFake)
        {
            this.isFake = _isFake;
        }

        //все задания могут содержать несколько источников и назначений
        //сначала на локальный компьютер передаются файлы из источников, а затем 
        //они передаются в назначения
        public ResultController TaskStartsExchange(MyProxy prx, ResultController sessionResultController)
        {
            ResultController taskResultController = new ResultController("Выполнение задачи " + this.id);            

            ControllerOfOutput outputs = ControllerOfOutput.Instance;
            //LogOfSuccession los = LogOfSuccession.Instance;
            //bool success = true;

            try
            {
                this.ClearTmpDir();

                //Если есть роуты, добавляем их
                foreach (CmdRoute rt in taskRouts)
                {
                    BatchRunner.AddRoute(rt.destination, rt.subnetMask, rt.gateway);
                }

                List<FileSupertype> downloadFiles = uploadDownload.FindAll(x => x.source_dest == ActionType.Source);
                List<FileSupertype> uploadFiles = uploadDownload.FindAll(x => x.source_dest == ActionType.Destination);
                
                foreach (FileSupertype dfile in downloadFiles)
                {
                    ActionResult tmpRes = dfile.StartFileAction(prx);
                    taskResultController.AddAction(tmpRes);
                }

                foreach (FileSupertype ufile in uploadFiles)
                {
                    ActionResult tmpRes = ufile.StartFileAction(prx);
                    taskResultController.AddAction(tmpRes);
                }
                this.ClearTmpDir();                
            }
            catch (Exception ex)
            {
                outputs.WriteErrors(ex.Message);                                
            }
            return taskResultController;
        }

        private void ClearTmpDir()
        {
            //перед выполнением и после выполнения очищаем временную папку
            DirectoryInfo di = new DirectoryInfo(SettingsClass.Instance.GetTmpDirName());
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }
    }
}
