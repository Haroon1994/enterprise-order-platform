namespace EnterpriseOrderPlatform.Modules.Notifications.Domain;

public sealed class NotificationLog
{
    private NotificationLog() { }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static NotificationLog Create(Guid userId, string type, string message) => new()
    {
        Id = Guid.CreateVersion7(),
        UserId = userId,
        Type = type,
        Message = message,
        CreatedAt = DateTimeOffset.UtcNow,
    };
}
