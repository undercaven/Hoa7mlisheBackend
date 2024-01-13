using System.Collections.ObjectModel;
using System.Net.Mail;
using System.Text;

namespace hoa7mlishe.API.Mail
{
    public class MailParameters
    {
        public MailParameters(string caption, string message)
        {
            Recipients = new();
            Attachments = new();
            Message.Append(message);
            Caption = caption;
        }

        public string Caption { get; set; }
        public StringBuilder Message { get; set; }
        public MailAddressCollection Recipients { get; private set; }
        public Collection<Attachment> Attachments { get; private set; }

        public void AddAttachment(Attachment attachment) => Attachments.Add(attachment);

        public void AddRecipients(string[] recipients)
        {
            foreach (string recipient in recipients)
            {
                Recipients.Add(recipient);
            }
        }

        public async void AddAttachment(IFormFile attachment)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await attachment.CopyToAsync(ms);
                AddAttachment(new Attachment(ms, attachment.FileName, attachment.ContentType));
            }
        }

        public void AddAttachments(IFormFile[] attachments)
        {
            var tasks = new List<Task>();
            foreach (IFormFile attachment in attachments)
                tasks.Add(Task.Run(() => AddAttachment(attachment)));

            Task.WaitAll(tasks.ToArray());
        }
    }
}
