using Mailjet.Client;
using Mailjet.Client.Resources;
using System.Threading.Tasks;

namespace AspnetIdentityDemo.Service
{
    public interface IMailService
    {
        Task SendEmailAsync(string toEmail, string Subject, string Content);
    }

    public class MailJetMailService : IMailService
    {
        public async Task SendEmailAsync(string toEmail, string Subject, string Content)
        {
            MailjetClient client = new MailjetClient("e26c1abaa11f775dc2d47afdd35c5bf7", "ecd820c8df7d37baacd95423c76dd0e9");
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.FromEmail, "samson@mojec.com")
            .Property(Send.FromName, "Mojec International Limited")
            .Property(Send.To, toEmail)
            .Property(Send.Subject, Subject)
            .Property(Send.TextPart, Content);
            MailjetResponse response = await client.PostAsync(request);
        }
    }
}
