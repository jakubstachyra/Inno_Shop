using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using UserManagement.Application.Interfaces;

namespace UserManagement.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpClient = new SmtpClient(_configuration["Email:Smtp:Host"])
        {
            Port = int.Parse(_configuration["Email:Smtp:Port"]!),
            Credentials = new NetworkCredential(
                _configuration["Email:Smtp:Username"],
                _configuration["Email:Smtp:Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Email:From"]!),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };

        mailMessage.To.Add(to);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send email.", ex);
        }
    }
}
