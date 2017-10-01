using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Xml;
using File_Exchange_Manager.Properties;
using System.Diagnostics;

namespace File_Exchange_Manager
{
    /// <summary>
    /// Класс, абстрагирующий доступ к настройкам
    /// </summary>
    public class SettingsClass : ApplicationSettingsBase
    {
        public readonly string MailIP;
        public readonly string MailFromLogin;
        public readonly string MailFromPass;
        public readonly string MailToAddress;

        public bool __showConsole = false;
        public bool __runAll = false;

        private string DIRNAME = "";

        private static SettingsClass instance;
                
        private SettingsClass()
        {
            PrivateData pd = PrivateData.Instance;
            this.MailIP = pd.GetValueFromTemplate(Settings.Default.MailIP);
            this.MailFromLogin = pd.GetValueFromTemplate(Settings.Default.MailFromLogin);
            this.MailFromPass = pd.GetValueFromTemplate(Settings.Default.MailFromPass);
            this.MailToAddress = pd.GetValueFromTemplate(Settings.Default.MailToAddress);
        }

        public static SettingsClass Instance
        {
            get { return instance ?? (instance = new SettingsClass()); }
        }

        public string GetTmpDirName()
        {
            if (!String.IsNullOrEmpty(DIRNAME))
                return this.DIRNAME;

            this.DIRNAME = Directory.GetCurrentDirectory() +  "\\tmp\\";
            if (!Directory.Exists(this.DIRNAME))
            {
                Directory.CreateDirectory(this.DIRNAME);
            }
            return this.DIRNAME;
        }        
    }
}