using Microsoft.Extensions.Options;
using System.Collections.Specialized;
using System.Net.Mail;

namespace Sponge.Common
{
    public class Email
    {
        private readonly IOptions<AppSettings> _settings;

        public Email(IOptions<AppSettings> settings)
        {
            _settings = settings;
        }
        public string GetMessageBody(string messageTemplate, NameValueCollection nvc)
        {
            messageTemplate = ReplacePlaceHolders(messageTemplate, nvc);
            return messageTemplate;

        }
        private string ReplacePlaceHolders(string text, NameValueCollection valueCollection)
        {
            if (valueCollection == null || valueCollection.Count <= 0)
            {
                throw new ArgumentException("Invalid NameValueCollection");
            }
            //string text=null;
            string result = text;
            string value;
            foreach (string key in valueCollection.AllKeys)
            {
                value = valueCollection[key];
                result = result.Replace(key, value);
            }
            return result;
        }
        public void SendMail(string filename, string subject, string mailbody, string MailID)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = _settings.Value.SMTPHost;

            MailMessage mailMessage = new MailMessage();
            mailMessage.Body = mailbody;
            mailMessage.Subject = subject;
            mailMessage.From = new MailAddress(_settings.Value.MailFrom);
            mailMessage.To.Add(new MailAddress(MailID));
            mailMessage.IsBodyHtml = true;
            if (!string.IsNullOrEmpty(filename))
            {
                mailMessage.Attachments.Add(new Attachment(filename));
            }

            smtpClient.Send(mailMessage);

        }
    }
}
