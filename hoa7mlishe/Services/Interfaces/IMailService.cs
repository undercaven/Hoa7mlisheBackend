using hoa7mlishe.API.Mail;

namespace hoa7mlishe.API.Services.Interfaces
{
    public interface IMailService
    {
        public Task SendMessage(MailParameters mailParams);
    }
}
