using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace File_Exchange_Manager
{
    public class NetTest
    {

        /// <summary>
        /// Тест сети
        /// </summary>
        /// <param name="logs">Файл логов</param>
        /// <param name="ftpIP">адрес сервера</param>
        /// <returns></returns>
        public static bool NetworkTest(ControllerOfOutput logs, string ftpIP)
        {
            string[] str = new string[] { ftpIP };
            logs.WriteAverageMessage("Пингуем FTP...");
            if (Pinging(logs, str) == false)
                logs.WriteCritMessage("FTP недоступен");

            str = new string[] { "8.8.8.8.", "google.com", "yandex.ru", "facebook.com", "twitter.com" };
            logs.WriteAverageMessage("Пингуем глобальную сеть...");
            if (Pinging(logs, str) == false)
                logs.WriteCritMessage("Ни один из адресов глобальной сети недоступен");

            str = new string[] { "10.10.0.100", "10.10.0.150", "10.10.0.151", "10.10.0.125" };
            logs.WriteAverageMessage("Пингуем локальную сеть...");
            if (Pinging(logs, str) == false)
                logs.WriteCritMessage("Ни один из адресов локальной сети недоступен");

            return true;
        }

        /// <summary>
        /// Выполняет команду ping
        /// </summary>
        /// <param name="logs">файл логов</param>
        /// <param name="str">адреса для пингования</param>
        /// <returns></returns>
        public static bool Pinging(ControllerOfOutput logs, string[] str)
        {
            System.Net.NetworkInformation.Ping ping = null;
            System.Net.NetworkInformation.PingReply pingReply = null;
            List<string> adresses = new List<string>(str);
            foreach (var item in adresses)
            {
                try
                {
                    ping = new System.Net.NetworkInformation.Ping();
                    pingReply = ping.Send(item);
                    if (pingReply.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        logs.WriteAverageMessage(String.Format("Адрес {0} успешно пропингован! RoundtripTime: {1}. Status: {2}",
                            item, pingReply.RoundtripTime, pingReply.Status));
                    }
                    else
                    {
                        logs.WriteCritMessage(String.Format("Ответ от адреса {0} отличается от успешного! RoundtripTime: {1}. Status: {2}",
                            item, pingReply.RoundtripTime, pingReply.Status));
                        continue;
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    if (pingReply != null)
                        logs.WriteCritMessage(String.Format("Адрес {0} недоступен. RoundtripTime: {1}. Status: {2}. Error: {3}",
                            item, pingReply.RoundtripTime, pingReply.Status, ex.Message));
                    else
                        logs.WriteCritMessage(String.Format("Во время получения ответа от адреса {0} произошла ошибка {1}",
                            item, ex.Message));
                    continue;
                }
            }
            return false;
        }

        public static void TestProxy()
        {
            WebProxy wp = new WebProxy("10.10.0.100:8080", true);
            wp.Credentials = new NetworkCredential("isbril", "310890");
            WebRequest wrq = WebRequest.Create("http://www.google.com");
            //wrq.Credentials = new NetworkCredential("isbril", "310890");
            wrq.Proxy = wp;            
            WebResponse wrs = wrq.GetResponse();
        }        

        public static void TestProxy2()
        {
            FtpWebResponse response = null;
            FtpWebRequest ftpReq = (FtpWebRequest)FtpWebRequest.Create("ftp://10.10.0.131");
            ftpReq.Credentials = new NetworkCredential("tts3ftp", "tts3receiver");
            ftpReq.Method = WebRequestMethods.Ftp.ListDirectory;
            WebProxy wp = new WebProxy("10.10.0.100:8080", true, new string[] { "10.10.0.*" });            
            wp.Credentials = new NetworkCredential("isbril", "310890");
            ftpReq.Proxy = wp;
            response = (FtpWebResponse)ftpReq.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());

            string bigLine = "";
            string line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                Console.WriteLine(line);
                bigLine += line;
                line = streamReader.ReadLine();
            }
            Console.ReadLine();
        }        
    }
}