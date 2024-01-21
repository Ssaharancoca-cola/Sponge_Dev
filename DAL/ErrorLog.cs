using Microsoft.AspNetCore.Http;
using System.Net.Mail;

namespace DAL.Common
{    
    public static class SentErrorMail
    {
        private static readonly string SMTPHost = "smtp.coca-cola.com";
        private static readonly string MailFrom = "noreplySponge@coca-cola.com";
        private static readonly string MailIds = "ssaharan@coca-cola.com";

        public static void SentEmailtoError(string message)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = SMTPHost;
            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = "Hi All,Please find the below Sponge  Application error message details:" + message + "";
            mailMessage.Subject = "Error On Sponge Application  --Do Not Reply---";
            mailMessage.From = new MailAddress(MailFrom);
            var Emailids = MailIds;
            mailMessage.To.Add(Emailids);
            smtpClient.Send(mailMessage);
        }
        public static void SentEmailtoBatchJobError(string message)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = SMTPHost;
            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = "Hi All,Please find the below Sponge Batch Job Application error message details:" + message + "";
            mailMessage.Subject = "Error On Sponge Batch Job  --Do Not Reply---";
            mailMessage.From = new MailAddress(MailFrom);
            mailMessage.To.Add(MailIds);
            smtpClient.Send(mailMessage);
        }
    }
    public class ErrorLog : IDisposable
    {
        public void LogErrorInTextFile(Exception ex)
        {
            string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
            StreamWriter swCSVWriter = null;
            string webRootPath = Directory.GetCurrentDirectory();
            string path = Path.Combine(webRootPath, "ErrorLog");
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("UserId: {0}", userName.Last().ToUpper());
            message += Environment.NewLine;
            //message += string.Format("URL: {0}", context.Current.Request.Url.ToString());
            message += Environment.NewLine;
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += string.Format("InnerException: {0}", ex.InnerException);
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            if (!File.Exists(path + "/ErrorLog.txt"))
            {
                using (StreamWriter writer = File.CreateText(path + "/ErrorLog.txt"))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
                swCSVWriter = File.CreateText("ErrorLog.txt");
                swCSVWriter.Write(message);
                swCSVWriter.Close();
                swCSVWriter = null;
            }
            else
            {
                StreamWriter writer = File.AppendText(path + "/ErrorLog.txt");
                writer.WriteLine(message);
                writer.Close();
                writer.Close();
            }
            SentErrorMail.SentEmailtoError(message);

        }

        public void BatchJobLogErrorInTextFile(Exception ex)
        {
            string[] userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new[] { "\\" }, StringSplitOptions.None);
            StreamWriter swCSVWriter = null;
            string path = Directory.GetCurrentDirectory();
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            
            message += string.Format("Message: {0}", ex.Message);
            message += Environment.NewLine;
            message += string.Format("StackTrace: {0}", ex.StackTrace);
            message += Environment.NewLine;
            message += string.Format("Source: {0}", ex.Source);
            message += Environment.NewLine;
            message += string.Format("TargetSite: {0}", ex.TargetSite.ToString());
            message += Environment.NewLine;
            message += string.Format("InnerException: {0}", ex.InnerException);
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            if (!File.Exists(path + "/ErrorLog.txt"))
            {
                using (StreamWriter writer = File.CreateText(path + "/ErrorLog.txt"))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
                swCSVWriter = File.CreateText("ErrorLog.txt");
                swCSVWriter.Write(message);
                swCSVWriter.Close();
                swCSVWriter = null;
            }
            else
            {
                StreamWriter writer = File.AppendText(path + "/ErrorLog.txt");
                writer.WriteLine(message);
                writer.Close();
                writer.Close();
            }
            SentErrorMail.SentEmailtoBatchJobError(message);

        }

        public void LogMessageInTextFile(String Message)
        {
            StreamWriter swCSVWriter = null;
            string webRootPath = Directory.GetCurrentDirectory();
            string path = Path.Combine(webRootPath, "ErrorLog");
            string message = string.Format("Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"));
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            message += string.Format("Message: {0}", Message);
            message += "-----------------------------------------------------------";
            message += Environment.NewLine;
            bool exists = System.IO.Directory.Exists(path);

            if (!exists)
                System.IO.Directory.CreateDirectory(path);
            if (!File.Exists(path + "/BatchJob.txt"))
            {
                using (StreamWriter writer = File.CreateText(path + "/BatchJob.txt"))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
                swCSVWriter = File.CreateText("BatchJob.txt");
                swCSVWriter.Write(message);
                swCSVWriter.Close();
                swCSVWriter = null;
            }
            else
            {
                StreamWriter writer = File.AppendText(path + "/BatchJob.txt");
                writer.WriteLine(message);
                writer.Close();
                writer.Close();
            }

        }

        void IDisposable.Dispose()
        {

        }

    }
}

