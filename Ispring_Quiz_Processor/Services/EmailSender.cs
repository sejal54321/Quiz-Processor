using Ispring_Quiz_Processor.Model;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Ispring_Quiz_Processor.Services
{
    public class EmailSender 
    {
        private readonly EmailSetting _emailSettingConfig;
        public EmailSender(IOptions<EmailSetting> emailSettingConfig)
        {
            _emailSettingConfig = emailSettingConfig.Value;

        }
        public static bool SendEmailAsync(string email, string subject, string message)
        {
            return true;
        }

        public bool SendEmailAsyncWithBody(string to, string subject, string body, bool isHtml = false)
        {
            var mailMessage = new MailMessage { From = new MailAddress(_emailSettingConfig.email) };

            mailMessage.To.Add(new MailAddress(to));

            mailMessage.Subject = subject;

            mailMessage.Body = body;

            mailMessage.IsBodyHtml = isHtml;

            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Excel\\QuizExcel_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_');
            if(filepath != null)
            {
                Attachment att = new Attachment(filepath);
                mailMessage.Attachments.Add(att);
            }          

            var smtpClient = new SmtpClient
            {
                Host = _emailSettingConfig.HostName,
                Port = _emailSettingConfig.PortNumber,
                EnableSsl = true,
                Credentials = new NetworkCredential(_emailSettingConfig.email, _emailSettingConfig.password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                WriteToFile(ex.InnerException + " " + ex.Message);
                return false;
            }
        }

        public void WriteToFile(string Message)
        {
           
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\QuizLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            if (!System.IO.File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = System.IO.File.CreateText(filepath))
                {
                        
                        sw.WriteLine("Exception Message: " + Message);
                    
                }
            }
            else
            {
                using (StreamWriter sw = System.IO.File.AppendText(filepath))
                {
                  
                        sw.WriteLine();
                        sw.WriteLine("Exception Message: " + Message);
                    
                }
            }

        }
    }
}
