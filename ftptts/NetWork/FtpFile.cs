using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Threading;

namespace File_Exchange_Manager
{
    public class FtpFile : FileSupertype
    {
        public string ftpUri = "";
        public string ftpUsername = "";
        public string ftpPass = "";
        public int ftpPort = 80;
        public bool isPassive = true;
        public int ftpTimeout = 1000;
        public bool ftpUseBinary = true;
        public bool ftpKeepAlive = true;
        public string ftpProxyType = "None";

        public FtpFile()
        {
            this.statistics.conType = ConnectionType.FtpFile;
        }

        public FtpFile(FtpFile cln, int taksId)
        {
            this.statistics.conType = ConnectionType.FtpFile;
            this.ftpUri = cln.ftpUri;
            this.ftpUsername = cln.ftpUsername;
            this.ftpPass = cln.ftpPass;
            this.ftpPort = cln.ftpPort;
            this.isPassive = cln.isPassive;
            this.ftpTimeout = cln.ftpTimeout;
            this.ftpUseBinary = cln.ftpUseBinary;
            this.ftpKeepAlive = cln.ftpKeepAlive;
            this.ftpProxyType = cln.ftpProxyType;

            this.filePath = cln.filePath;            
            this.source_dest = cln.source_dest;
            this.addStrategy = new AddStrategyClass(cln.addStrategy);
            this.errorCount = cln.errorCount;
            this.sleepBeforeNext = cln.sleepBeforeNext;
        }

        //настраивает реквест для избавления от дублирования
        private FtpWebRequest FtpSet(FtpWebRequest req)
        {
            if ((!String.IsNullOrEmpty(this.ftpUsername)) && (!String.IsNullOrEmpty(this.ftpPass)))                
                req.Credentials = new NetworkCredential(this.ftpUsername, this.ftpPass);
            
            if (this.ftpProxyType == "Planner")
            {                   
                WebProxy wp = new WebProxy(this.prx.address, true, new string[] { this.prx.addressesToBypas });
                wp.Credentials = new NetworkCredential(this.prx.user, this.prx.pass);
                wp.BypassProxyOnLocal = true;
                req.Proxy = wp;
            }
            else if (this.ftpProxyType == "Default")
            {                
                req.Proxy = WebRequest.DefaultWebProxy;
                req.Proxy.Credentials = new NetworkCredential(this.prx.user, this.prx.pass);
            }
            else if (this.ftpProxyType == "None")
            {
                req.Proxy = null;
            }

            req.KeepAlive = this.ftpKeepAlive;
            req.UsePassive = this.isPassive;
            req.UseBinary = this.ftpUseBinary;
            req.Timeout = this.ftpTimeout;

            return req;
        }

        public override void ActionAsSource()
        {   
            string tmpDirName = SettingsClass.Instance.GetTmpDirName();
            ControllerOfOutput output = ControllerOfOutput.Instance;
            string ftpUriStr = "ftp://" + this.ftpUri + this.CalculatedFilePath;

            Dictionary<int, string> filesDict = new Dictionary<int, string>();
            Dictionary<int, DateTime> filesModifDict = new Dictionary<int, DateTime>();
            FtpWebResponse response = null;

            try
            {
                //здесь мы считываем список файлов, находящихся на ftp
                FtpWebRequest ftpReq = (FtpWebRequest)FtpWebRequest.Create(ftpUriStr);
                ftpReq = this.FtpSet(ftpReq);
                ftpReq.Method = WebRequestMethods.Ftp.ListDirectory;
                response = (FtpWebResponse)ftpReq.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                
                int ind = 0;
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    filesDict.Add(ind++, line);
                    line = streamReader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Ошибка при считывании списка файлов c Uri {0}. Текст ошибки: {1}", ftpUriStr, ex.Message);
                output.WriteErrors(errMsg);
                this.statistics.addError(errMsg);
                return;
            }            

            //если используется дата создания, сперва нужно опросить каждый файл о дате его последней модификации
            foreach (KeyValuePair<int, string> fileName in filesDict)
            {
                FtpWebRequest request = null;

                try
                {
                    request = (FtpWebRequest)WebRequest.Create(ftpUriStr + fileName.Value);
                    request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                    request = this.FtpSet(request);
                    response = (FtpWebResponse)request.GetResponse();
                    filesModifDict.Add(fileName.Key, response.LastModified.ToUniversalTime());

                    //проверка, подходит ли данный файл для передачи
                    if (!this.addStrategy.FileVerification(fileName.Value, filesModifDict[fileName.Key]))
                        continue;                    
                }
                catch (Exception ex)
                {
                    string errMsg = String.Format(
                        "Ошибка при определении даты последней модификации файла {0}. Текст ошибки: {1}", fileName, ex.Message);
                    output.WriteErrors(errMsg);
                    this.statistics.addError(errMsg);
                    return;
                }

                try
                {
                    //при успешной проверке мы передаем файл во временную папку
                    output.WriteAverageMessage(String.Format("Передаем файл {0} во временную папку",
                            fileName.Value));

                    //если передаваемый файл существует во временной папке
                    if (File.Exists(tmpDirName + fileName.Value))
                    {
                        string errMsg2 = String.Format("Файл {0} уже передавался из других источников.", fileName.Value);
                        output.WriteAverageMessage(errMsg2);
                        this.statistics.addMessage(errMsg2);
                        File.Delete(tmpDirName + fileName.Value);
                    }

                    FtpWebRequest request2 = (FtpWebRequest)WebRequest.Create(ftpUriStr + fileName.Value);
                    request2.Method = WebRequestMethods.Ftp.DownloadFile;
                    request = this.FtpSet(request2);
                    response = (FtpWebResponse)request2.GetResponse();
                    Stream stream = response.GetResponseStream();
                    List<byte> streamByteList = new List<byte>();                    
                    int b;
                    long allBytesCount = 0;
                    while ((b = stream.ReadByte()) != -1)
                    {
                        streamByteList.Add((byte)b);
                        allBytesCount += b;
                    }

                    File.WriteAllBytes(tmpDirName + fileName.Value, streamByteList.ToArray());

                    this.statistics.incrementFiles(1);
                    this.statistics.incrementBytesWithCut(new System.IO.FileInfo(tmpDirName + fileName.Value).Length);
                    this.statistics.addResult(WorkResult.Success);
                }
                //ToDo: А что если такой файл уже будет существовать?
                catch (Exception ex)
                {
                    //this.statistics.addResult(WorkResult.Error);
                    string errMsg = String.Format(
                        "Ошибка при передаче файла {0} во временную папку. Текст ошибки: {1}", fileName, ex.Message);
                    output.WriteErrors(errMsg);
                    this.statistics.addError(errMsg);
                    return;
                    //throw new FileExchangeException(errMsg);
                }                

                output.WriteAverageMessage(String.Format("Файл {0} успешно передан во временную папку {1}",
                        fileName.Value, tmpDirName + fileName.Value));
            }            

            Console.WriteLine();
        }

        public override void ActionAsDestination()
        {               
            ControllerOfOutput output = ControllerOfOutput.Instance;
            DirectoryInfo di = new DirectoryInfo(SettingsClass.Instance.GetTmpDirName());

            //int localErrorCount = 0;

            foreach (FileInfo file in di.GetFiles())
            {                
                string ftpUriStr = "";
                try
                {
                    ftpUriStr = @"ftp://" + this.ftpUri + "/" + file.Name;
                                        
                    FtpWebRequest request = WebRequest.Create(new Uri(string.Format(ftpUriStr))) as FtpWebRequest;
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request = this.FtpSet(request);
                    
                    //здесь непосредственно передача
                    int totalReadBytesCount = 0;
                    using (FileStream inputStream = File.OpenRead(file.FullName))
                    using (Stream outputStream = request.GetRequestStream())
                    {                        
                        byte[] buffer = new byte[16384];                        
                        int prevByteCount = 0;
                        int readBytesCount;                        
                        while ((readBytesCount = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {                            
                            outputStream.Write(buffer, 0, readBytesCount);
                            totalReadBytesCount += readBytesCount;
                            double progress = totalReadBytesCount * 100.0 / inputStream.Length;
                            double totalKBytes = Math.Round((double)totalReadBytesCount / 1000, 0);
                            double allKBytes = Math.Round((double)inputStream.Length / 1000, 0);                                                        

                            prevByteCount = totalReadBytesCount;
                            output.WriteProgress(String.Format("Progress: {0}% {1} Кб из {2} Кб.",
                                Math.Round(progress, 2).ToString("##0.00"), totalKBytes, allKBytes));
                        }
                    }

                    this.statistics.incrementFiles(1);
                    this.statistics.incrementBytesWithCut(file.Length);
                    this.statistics.addResult(WorkResult.Success);
                }
                catch (Exception ex)
                {
                    //сперва проверяем, не вылезла ли ошибка только потому, что такой файл уже существует
                    FtpWebRequest request = WebRequest.Create(new Uri(string.Format(ftpUriStr))) as FtpWebRequest;                    
                    request.Method = WebRequestMethods.Ftp.GetFileSize;
                    request = this.FtpSet(request);
                    try
                    {
                        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    }
                    catch (WebException wex)
                    {
                        FtpWebResponse response = (FtpWebResponse)wex.Response;
                        if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                        {
                            string errMsg2 = String.Format("Файл {0} уже существует на ftp-сервере {1}.", file.FullName, ftpUriStr);
                            output.WriteAverageMessage(errMsg2);
                            this.statistics.addMessage(errMsg2);
                            continue;
                        }
                    }

                    string errMsg = String.Format(
                            "Ошибка при передачи файла {0} На сервер {1}. Текст ошибки: {2}", file.Name, ftpUriStr, ex.Message);                    
                    output.WriteErrors(errMsg);
                    this.statistics.addError(errMsg);
                    return;
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

        public override ConnectionType GetConnectionType()
        {
            return ConnectionType.FtpFile;
        }
    }
}
