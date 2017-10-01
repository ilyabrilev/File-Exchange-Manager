using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

namespace File_Exchange_Manager
{
    public class OutputEmail : OutputSupertype
    {
        public void SenTestEmail()
        {
            PrivateData pd = PrivateData.Instance;
            SendMail("smtp.yandex.ru", pd.GetValueFromKey("TestMailFromLogin"), pd.GetValueFromKey("TestMailFromPass"),
                pd.GetValueFromKey("TestMailToLogin"), "Тело письма", "Тема письма", "C:\\txt.txt");
        }

        public override void WriteLog(string msg, string title) 
        {
            this.SendWorkEmail(msg, title, null);
        }

        public void SendWorkEmail(string mBody, string mHeader, string attached)
        {
            SettingsClass set = SettingsClass.Instance;
            SendMail(set.MailIP, set.MailFromLogin, set.MailFromPass, set.MailToAddress, mBody, mHeader, attached);
        }

        /// <summary>
        /// Отправка письма на почтовый ящик C# mail send
        /// </summary>
        /// <param name="smtpServer">Имя SMTP-сервера</param>
        /// <param name="from">Адрес отправителя</param>
        /// <param name="password">пароль к почтовому ящику отправителя</param>
        /// <param name="mailto">Адрес получателя</param>
        /// <param name="caption">Тема письма</param>
        /// <param name="message">Сообщение</param>
        /// <param name="attachFile">Присоединенный файл</param>
        public void SendMail(string smtpServer, string from, string password,
            string mailto, string message, string caption = "Ошибка выполнения программы", string attachFile = null)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(new MailAddress(mailto));
                mail.Subject = caption;
                mail.Body = message;
                if (!string.IsNullOrEmpty(attachFile))
                    mail.Attachments.Add(new Attachment(attachFile));
                SmtpClient client = new SmtpClient();
                client.Host = smtpServer;
                client.Port = 25;
                client.EnableSsl = false;
                client.Credentials = new NetworkCredential(from.Split('@')[0], password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Send(mail);
                mail.Dispose();
            }
            catch (Exception)
            {
                //throw new Exception("Mail.Send: " + e.Message);
            }
        }

        public override void WorkIsOver(string msg, string title)
        {
            this.WriteLog(msg, title);
        }
    }
}
