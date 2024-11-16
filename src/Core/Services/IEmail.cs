using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muzonia.Core.Common;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Muzonia.Core.Services;

public interface IEmail
{
    Task SendEmailAsync(string receiver, string subject, string message);
}

public sealed class NoopEmail : IEmail, ISingleton
{
    public Task SendEmailAsync(string receiver, string subject, string message)
    {
        return Task.CompletedTask;
    }
}

public sealed class SendgridEmail : IEmail, IScoped
{
    private readonly string apiKey;
    private readonly ILogger<SendgridEmail> _logger;
    private readonly ApiConfig config;

    public SendgridEmail(
        ILogger<SendgridEmail> logger,
        IConfiguration cfg,
        IOptions<ApiConfig> options
    )
    {
        _logger = logger;
        var sendgrid = cfg.GetConnectionString("sendgrid");
        ArgumentNullException.ThrowIfNull(sendgrid);
        apiKey = sendgrid;
        config = options.Value;
    }

    public async Task SendEmailAsync(
        string receiver,
        string subject,
        string message
    )
    {
        _logger.LogInformation(
            "Sending email to {receiver} with subject {subject}",
            receiver,
            subject
        );

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(
            config.SenderEmailAddress,
            "Muzonia Service"
        );

        var plaintext = message;
        var htmlContent = $"<strong>{message}</strong>";

        var to = new EmailAddress(receiver);
        var msg = MailHelper.CreateSingleEmail(
            from,
            to,
            subject,
            plaintext,
            htmlContent
        );
        var response = await client.SendEmailAsync(msg);
    }
}
