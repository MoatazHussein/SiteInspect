namespace SiteInspect.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "Messaging:RabbitMq";

    public string Host { get; init; } = "rabbitmq://localhost";

    public string Username { get; init; } = "guest";

    public string Password { get; init; } = string.Empty;
}
