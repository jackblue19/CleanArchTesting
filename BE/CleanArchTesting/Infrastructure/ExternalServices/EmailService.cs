// Infrastructure/ExternalServices/EmailService.cs
using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.ExternalServices;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    public EmailService(ILogger<EmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("Sending email to {To}: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
