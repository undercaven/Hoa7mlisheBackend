using hoa7mlishe.API.Mail;
using hoa7mlishe.API.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace hoa7mlishe.API.Services
{
    public class MailService : IMailService
    {
        private SmtpClient _smtpClient;
        private MailAddress _mailAddress;
        public MailService()
        {
            _smtpClient = new("gmail.com");
            _mailAddress = new("admin@hoa7mlishe.com");
        }

        public async Task SendMessage(MailParameters mailParams)
        {
            var tasks = new List<Task>();
            foreach (var recipient in mailParams.Recipients)
            {
                var message = new MailMessage(_mailAddress, recipient)
                {
                    Subject = mailParams.Caption,
                    Body = mailParams.Message.ToString()
                };

                tasks.Add(_smtpClient.SendMailAsync(message));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
