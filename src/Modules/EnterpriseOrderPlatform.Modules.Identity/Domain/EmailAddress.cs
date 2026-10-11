using EnterpriseOrderPlatform.BuildingBlocks;

namespace EnterpriseOrderPlatform.Modules.Identity.Domain;

/// <summary>A normalized, minimally validated email address.</summary>
public sealed record EmailAddress
{
    public const int MaxLength = 254;

    private EmailAddress(string value) => Value = value;

    public string Value { get; }

    public static Result<EmailAddress> Create(string? input)
    {
        var trimmed = input?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return Result.Failure<EmailAddress>(new ResultError("email.empty", "Email is required."));
        }

        if (trimmed.Length > MaxLength)
        {
            return Result.Failure<EmailAddress>(new ResultError("email.too_long", "Email is too long."));
        }

        var at = trimmed.IndexOf('@');
        var dotAfterAt = at >= 0 ? trimmed.IndexOf('.', at + 1) : -1;

        var validShape = at > 0
            && at == trimmed.LastIndexOf('@')
            && at < trimmed.Length - 1
            && dotAfterAt > at + 1
            && !trimmed.EndsWith('.')
            && !trimmed.Contains(' ');

        return validShape
            ? Result.Success(new EmailAddress(trimmed.ToLowerInvariant()))
            : Result.Failure<EmailAddress>(new ResultError("email.invalid", "Email format is invalid."));
    }
}
