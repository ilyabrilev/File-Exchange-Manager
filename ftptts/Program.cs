using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace File_Exchange_Manager
{
    class Program
    {
        static void Main(string[] args)
        {
            Init(args);
            //проверка файла ведения логов
            LogOfSuccession los = LogOfSuccession.Instance;
            if (!los.checkStatus)
            {
                ControllerOfOutput.Instance.WriteCritMessage("При попытке доступа к фалам логов произошла ошибка.");
                return;
            }
            else
            {
                PlannerText plnText = new PlannerText();
                plnText.StartExchange();
            }
            FadeOut();
        }

        static void Init(string[] args)
        {
            SettingsClass sc = SettingsClass.Instance;

            //парсинг командной строки
            foreach (string s in args)
            {
                Console.WriteLine(s);
                if (s == "-sc")
                    sc.__showConsole = true;
                else if (s == "-ra")
                    sc.__runAll = true;
            }

            ControllerOfOutput.Instance.WriteAverageMessage("Начинаем сессию от " + GlobalUtils.GetNowUtcDateTime().ToString());
        }

        static void FadeOut()
        {
            SettingsClass sc = SettingsClass.Instance;

            Thread.Sleep(2000);

            if (sc.__showConsole)
                Console.ReadLine();
        }
    }
}
