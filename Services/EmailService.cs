using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace FliesProject.Services
{
    public class EmailService : IEmailService
    {
        public void SendEmail(string to, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Flies Learning", "quanqh12042000@gmail.com"));
            message.To.Add(new MailboxAddress("User", to));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = htmlMessage
            };

            using var smtp = new SmtpClient();
            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtp.Authenticate("quanqh12042000@gmail.com", "ilue ujis erip cmgm");
            smtp.Send(message);
            smtp.Disconnect(true);
        }
    }
}
