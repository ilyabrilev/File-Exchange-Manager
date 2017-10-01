using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace File_Exchange_Manager
{
    public class WinFSFile : FileSupertype
    {
        public WinFSFile()
        {
            this.statistics.conType = ConnectionType.WinFSFile;
        }

        public WinFSFile(WinFSFile cln, int taksId)
        {
            this.statistics.conType = ConnectionType.WinFSFile;
            this.filePath = cln.filePath;
            this.source_dest = cln.source_dest;
            this.addStrategy = new AddStrategyClass(cln.addStrategy);
            this.errorCount = cln.errorCount;
            this.sleepBeforeNext = cln.sleepBeforeNext;
        }

        public override void ActionAsSource()
        {
            string tmpDirName = SettingsClass.Instance.GetTmpDirName();
            ControllerOfOutput output = ControllerOfOutput.Instance;
            //string[] filenames = Directory.GetFiles(this.CalculatedFilePath);            

            DirectoryInfo di = new DirectoryInfo(this.CalculatedFilePath);
            if (!this.checkDirectory(di))
                return;
           
            foreach (FileInfo file in di.GetFiles())
            { 
                try
                {
                    //проверка, подходит ли данный файл для передачи
                    if (!this.addStrategy.FileVerification(file.Name, File.GetLastWriteTime(file.FullName).ToUniversalTime()))
                        continue;

                    output.WriteAverageMessage(String.Format("Передаем файл {0} во временную папку {1}.",
                        file.FullName, tmpDirName + file.Name));

                    if (File.Exists(tmpDirName + file.Name))
                    {
                        string errMsg2 = String.Format("Файл {0} уже передавался из других источников.", file.Name);
                        output.WriteAverageMessage(errMsg2);
                        this.statistics.addMessage(errMsg2);
                        File.Delete(tmpDirName + file.Name);
                    }

                    File.Copy(file.FullName, tmpDirName + file.Name);

                    output.WriteAverageMessage(String.Format("Файл {0} успешно передан.", 
                        file.FullName, tmpDirName + file.Name));

                    this.statistics.incrementFiles(1);
                    this.statistics.incrementBytesWithCut(file.Length);
                    this.statistics.addResult(WorkResult.Success);
                }
                catch (Exception ex)
                {
                    string msg = String.Format(
                            "Ошибка при передаче файла {0} Во временную папку. Текст ошибки: {1}", file.Name, ex.Message);
                    output.WriteErrors(msg);
                    this.statistics.addError(msg);

                    //throw new FileExchangeException(String.Format("Ошибка при передачи файла {0} Во временную папку. Текст ошибки: {1}", file.Name, ex.Message));
                }
            }            
        }

        //копируем в нужную папку все файлы из временной директории
        public override void ActionAsDestination()
        {
            ControllerOfOutput output = ControllerOfOutput.Instance;

            DirectoryInfo di = new DirectoryInfo(SettingsClass.Instance.GetTmpDirName());
            if (!this.checkDirectory(di))
                return;

            foreach (FileInfo file in di.GetFiles())
            {                
                try
                {                
                    if (!Directory.Exists(this.CalculatedFilePath))
                        Directory.CreateDirectory(this.CalculatedFilePath);

                    if (File.Exists(this.CalculatedFilePath + file.Name))
                    {
                        string msg = String.Format("Файл {0} уже существует в папке {1}.",
                            file.FullName, this.CalculatedFilePath + file.Name);
                        output.WriteAverageMessage(msg);
                        this.statistics.addMessage(msg);
                        continue;
                    }

                    File.Copy(file.FullName, this.CalculatedFilePath + file.Name);
                    output.WriteAverageMessage(String.Format("Файл {0} успешно передан в папку-назначение {1}",
                        file.FullName, this.CalculatedFilePath + file.Name));
                    //throw new Exception();
                    this.statistics.incrementFiles(1);
                    this.statistics.incrementBytesWithCut(file.Length);
                    this.statistics.addResult(WorkResult.Success);
                }
                catch (Exception ex)
                {
                    string errMsg = String.Format(
                                    "Ошибка при передаче файла {0} В папку {1}. Текст ошибки: {2}",
                                    file.Name, this.CalculatedFilePath, ex.Message);                    
                    output.WriteErrors(errMsg);
                    this.statistics.addError(errMsg);
                    //пока не понятно нужен ли errorCount или нет
                    //localErrorCount++;
                    /*
                    if (localErrorCount > this.errorCount)
                    {
                        throw new FileExchangeException(errMsg);
                    }
                    Thread.Sleep(this.sleepBeforeNext);
                    */
                }
            }                        
        }

        public bool checkDirectory(DirectoryInfo di)
        {            
            ControllerOfOutput output = ControllerOfOutput.Instance;

            try
            {
                di.GetFiles();
            }
            catch (Exception ex)
            {

                string errMsg = String.Format(
                                    "Ошибка при подключении к директории {0}. Текст ошибки: {1}",
                                    di.FullName, ex.Message);
                output.WriteErrors(errMsg);
                this.statistics.addError(errMsg);
                return false;
            }

            return true;
        }

        public override ConnectionType GetConnectionType()
        {
            return ConnectionType.WinFSFile;
        }
    }
}
