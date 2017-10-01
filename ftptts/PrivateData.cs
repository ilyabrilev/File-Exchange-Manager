using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using File_Exchange_Manager.Properties;
using System.Text.RegularExpressions;

namespace File_Exchange_Manager
{
    class PrivateData
    {
        private static PrivateData instance;
        private Dictionary<string, string> data = new Dictionary<string, string>();

        private PrivateData()
        {
            //считываем путь из настроек
            //конечно, плохо, что я обращаюсь к настройкам в обход SettingsClass
            //однако SettingsClass сам нуждается в данном классе
            string path = Settings.Default.PrivateDataPath;
            List<string> fileLines = new List<string>();

            //считываем содержимое файла
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    fileLines.Add(sr.ReadLine());
                }
            }

            //цикл по содержимому файла
            foreach (string line in fileLines)
            {
                //сплиттим каждую строку по ": "
                string[] splitted = line.Split(new string[] { " : " }, StringSplitOptions.RemoveEmptyEntries);
                if (splitted.Length >= 2)
                {
                    //сохраняем в data
                    data.Add(splitted[0].Trim(), splitted[1].Trim());
                }
            }            
        }

        public static PrivateData Instance
        {
            get { return instance ?? (instance = new PrivateData()); }
        }

        public string GetValueFromKey(string key) 
        {
            if (data.ContainsKey(key)) {
                return data[key];
            }
            else {
                return "NoSuchElement";
            }            
        }

        public string GetValueFromTemplate(string template)
        {
            string pattern = "%P_(.*)_P%";
            Regex regex = new Regex(pattern);

            Match valMatch = regex.Match(template);

            if (valMatch.Success)
            {
                return valMatch.Captures[0].Value;
            }
            else return template;
        }
    }
}
