using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;

namespace File_Exchange_Manager
{
    public class PlannerText
    {
        MyProxy prx = new MyProxy();
        public List<PlannerTask> tasks = new List<PlannerTask>();
        ControllerOfOutput outputs = null;
        private LogOfSuccession los = null;
        //private string plannerPath = "Planner.xml";
                
        /// Конструткор для тестов        
        public PlannerText(ControllerOfOutput _outputs, LogOfSuccession _los)
        {
            this.outputs = _outputs;
            this.los = _los;
        }

        public PlannerText() : this("Planner.xml", ControllerOfOutput.Instance, LogOfSuccession.Instance)
        {
            
        }

        /// <summary>
        /// Парсинг XML-файла
        /// </summary>
        /// <returns> </returns>
        public PlannerText(string _plannerPath, ControllerOfOutput _outputs, LogOfSuccession _los)
        {
            this.outputs = _outputs;
            this.los = _los;

            this.BackupPlannerXml();            
            
            PlannerTask tmpTask = new PlannerTask();
            FtpFile tmpFtpFile = new FtpFile();
            WinFSFile tmpWinFSFile = new WinFSFile();
            FileSupertype tmpFileSupertype = tmpFtpFile;
            AddStrategyClass tmpStrategy = new AddStrategyClass();
            CmdRoute tmpCmdRoute = new CmdRoute();
            ActionType source_dest = ActionType.Source;
            PrivateData pd = PrivateData.Instance;

            string nodeName = String.Empty;
            XmlTextReader reader = new XmlTextReader(_plannerPath);

            while (reader.Read())
            {
                string readerValPriv = pd.GetValueFromTemplate(reader.Value);

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // Узел является элементом.                        
                        nodeName = reader.Name;
                        switch (nodeName)
                        {
                            case "task":
                                tmpTask = new PlannerTask();
                                this.tasks.Add(tmpTask);
                                break;
                            case "route":
                                tmpCmdRoute = new CmdRoute();
                                tmpTask.taskRouts.Add(tmpCmdRoute);
                                break;
                            case "sourceFile":
                                source_dest = ActionType.Source;
                                break;
                            case "destinationFile":
                                source_dest = ActionType.Destination;
                                break;
                        }
                        break;
                    //ToDo: добавить отлавливание оршибок при ошибочном преобразовании
                    case XmlNodeType.Text: // Вывести текст в каждом элементе.  
                        switch (nodeName)
                        {
                            //исключительно Planner
                            case "proxyAddress":
                                this.prx.address = readerValPriv;
                                break;
                            case "proxyUser":
                                this.prx.user = readerValPriv;
                                break;
                            case "proxyPass":
                                this.prx.pass = readerValPriv;
                                break;
                            case "proxyAddressesToBypass":
                                this.prx.addressesToBypas = readerValPriv;
                                break;

                            //пошли задания                            
                            case "id":
                                tmpTask.id = int.Parse(readerValPriv);
                                break;
                            case "isFake":
                                tmpTask.isFake = bool.Parse(readerValPriv);
                                break;
                            case "taskDiscription":
                                tmpTask.taskDiscription = readerValPriv;
                                break;
                            case "deleteTmp":
                                tmpTask.deleteTmp = bool.Parse(readerValPriv);
                                break;
                            case "taskPeriod":
                                tmpTask.period = readerValPriv;
                                break;

                            //добавление IP route'ов
                            case "routeDestination":
                                tmpCmdRoute.destination = readerValPriv;
                                break;
                            case "routeSubnetmask":
                                tmpCmdRoute.subnetMask = readerValPriv;
                                break;
                            case "routeGateway":
                                tmpCmdRoute.gateway = readerValPriv;
                                break;

                            //стратегия добавления
                            case "addStrategyUseMask":
                                tmpStrategy.usemask = bool.Parse(readerValPriv);
                                break;
                            case "addStrategyFileMask":
                                tmpStrategy.fileMask = readerValPriv;
                                break;
                            case "addStrategyFileNameUsage":
                                tmpStrategy.fileNameUsage = readerValPriv;
                                break;
                            case "addStrategyWorkInterval":
                                tmpStrategy.workInterval = GlobalUtils.ParseWorkIntervalEnum(readerValPriv);
                                break;
                            case "addStrategyCheckDateCondition":
                                tmpStrategy.checkDateCondition = readerValPriv;
                                break;
                            case "addStrategyUseCreationDate":
                                tmpStrategy.useCreationDate = bool.Parse(readerValPriv);
                                break;
                            case "addStrategyDateRange":
                                tmpStrategy.dateRange = GlobalUtils.ParseDateRangeEnum(readerValPriv);
                                break;

                            //пошли файлы-источники и файлы-назначения
                            case "type":
                                if (readerValPriv == "FTP")
                                {
                                    tmpFtpFile = new FtpFile(tmpFtpFile, tmpTask.id);
                                    tmpFileSupertype = tmpFtpFile;
                                    tmpTask.uploadDownload.Add(tmpFileSupertype);
                                }
                                else
                                {
                                    tmpWinFSFile = new WinFSFile(tmpWinFSFile, tmpTask.id);
                                    tmpFileSupertype = tmpWinFSFile;
                                    tmpTask.uploadDownload.Add(tmpFileSupertype);
                                }
                                tmpStrategy = new AddStrategyClass(tmpStrategy);
                                tmpFileSupertype.addStrategy = tmpStrategy;
                                tmpFileSupertype.source_dest = source_dest;
                                break;
                            case "ftpUsername":
                                tmpFtpFile.ftpUsername = readerValPriv;
                                break;
                            case "ftpPass":
                                tmpFtpFile.ftpPass = readerValPriv;
                                break;
                            case "ftpUri":
                                tmpFtpFile.ftpUri = readerValPriv;
                                break;
                            case "ftpPort":
                                tmpFtpFile.ftpPort = int.Parse(readerValPriv);
                                break;
                            case "isPassive":
                                tmpFtpFile.isPassive = bool.Parse(readerValPriv);
                                break;
                            case "ftpTimeout":
                                tmpFtpFile.ftpTimeout = int.Parse(readerValPriv);
                                break;
                            case "ftpUseBinary":
                                tmpFtpFile.ftpUseBinary = bool.Parse(readerValPriv);
                                break;
                            case "ftpKeepAlive":
                                tmpFtpFile.ftpKeepAlive = bool.Parse(readerValPriv);
                                break;
                            case "ftpProxyType":
                                tmpFtpFile.ftpProxyType = readerValPriv;
                                break;

                            case "filePath":
                                tmpFileSupertype.filePath = readerValPriv;
                                break;
                            case "maxErrorCout":
                                tmpFileSupertype.errorCount = int.Parse(readerValPriv);
                                break;
                            case "sleepAfterError":
                                tmpFileSupertype.sleepBeforeNext = int.Parse(readerValPriv);
                                break;

                            default:
                                break;
                        }
                        break;

                    case XmlNodeType.EndElement:
                        /*
                        switch (reader.Name)
                        {
                            case "route":
                                tmpTask.taskRouts.Add(tmpCmdRoute);
                                break;
                        }*/
                        break;
                }
            }
            reader.Close();            
        }

        public string Privatisator()
        {
            return "";
        }

        //когда заканчивается парсинг xml-файла, необходимо выполнить все задания из него по очереди
        public ResultController StartExchange()
        {
            ResultController sessionRC = new ResultController("Выполнение сессии");
            
            foreach (PlannerTask oneTask in tasks)
            {
                //успешно ли выполнилось текуще задание
                bool taskRes = true;

                DateTime startDT = GlobalUtils.GetNowUtcDateTime();

                if (!oneTask.isFake)
                {
                    //сперва смотрим, нужно ли выполнять данный таск
                    if (this.los.CheckTaskExecute(oneTask.id, oneTask.period))
                    {
                        //сообщения отправляет класс LogOfSuccession
                        //это сделано потому, что сообщения, преденные системе вывода могут быть разными
                        //и в то же время этому коду нужно знать, продолжать выполнение задания или нет                        
                        continue;
                    }

                    ResultController tmpResContr = oneTask.TaskStartsExchange(this.prx, sessionRC);                    
                    taskRes = ActionResult.IsResultIsNotError(tmpResContr.globalWorkRes);
                    sessionRC.AddInnerController(tmpResContr);
                    this.los.SaveTaskResult(oneTask.id, taskRes, startDT);
                }
            }

            //даем команду сохранить все в логах проверки запуска
            this.los.WorkIsOver();
            this.outputs.WorkIsOver2(sessionRC);            

            return sessionRC;            
        }

        public void BackupPlannerXml()
        {
            string backupPath = "planner backup";
            //проверка, создана ли папка для бекапов
            if (!Directory.Exists(backupPath))
                Directory.CreateDirectory(backupPath);

            //заполняем списов файлов и дат их создания
            DirectoryInfo di = new DirectoryInfo(backupPath);
            Dictionary<DateTime, string> nameAndDate = new Dictionary<DateTime, string>();
            foreach (FileInfo file in di.GetFiles())
            {
                nameAndDate.Add(file.CreationTime, file.FullName);
            }

            //находим самый поздний бекап и удаляем его
            if (nameAndDate.Count > 10)
            {
                string minFile = nameAndDate[nameAndDate.Min(x => x.Key)];
                File.Delete(minFile);
            }

            //копируем планировщик
            File.Copy("Planner.xml", backupPath + "\\Planner " + GlobalUtils.GetNowUtcDateTime().ToString("yyyy-MM-dd hh-mm-ss") + ".xml");
        }
    }    

    public struct CmdRoute
    {
        public string destination;
        public string subnetMask;
        public string gateway;        
    }

    public struct MyProxy
    {
        public string address;
        public string user;
        public string pass;
        public string addressesToBypas;
    }

    public class FileExchangeException : Exception
    {
        public FileExchangeException(string msg) : base(msg)
        {
        }
    }
}
