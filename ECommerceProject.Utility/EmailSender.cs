using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceProject.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailOptions _emailOptions;
        public EmailSender(IOptions<EmailOptions> options)
        {
            _emailOptions = options.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(_emailOptions.SendGridApiKey, subject, htmlMessage, email);
        }
        private Task Execute(string sendGridApiKey, string subject, string message, string email)
        {          
            var client = new SendGridClient(sendGridApiKey);
            var from = new EmailAddress("sanalmagaza479@gmail.com", "Sanal Mağaza");
            var to = new EmailAddress(email, "Kullanıcı");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, "", message);
            return client.SendEmailAsync(msg);
        }

    }
}

 
