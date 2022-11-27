using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ordering.Application.Contracts.Infrastructure;
using Ordering.Application.Models;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        public EmailSettings EmailSettings { get; }
        public ILogger Logger { get; }

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger logger)
        {
            this.EmailSettings = emailSettings.Value;
            this.Logger        = logger;
        }

        public async Task<bool> SendEmail(Email email)
        {
            var client = new SendGridClient(this.EmailSettings.ApiKey);

            var subject   = email.Subject;
            var to        = new EmailAddress(email.To);
            var emailBody = email.Body;

            var from = new EmailAddress
            {
                Email = this.EmailSettings.FromAddress,
                Name  = this.EmailSettings.FromName
            };

            var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, emailBody, emailBody);

            var response = await client.SendEmailAsync(sendGridMessage);

            this.Logger.LogInformation("Email sent.");

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            this.Logger.LogError("Email sending failed.");

            return false;
        }
    }
}
