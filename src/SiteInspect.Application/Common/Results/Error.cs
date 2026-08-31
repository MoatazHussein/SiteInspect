namespace SiteInspect.Application.Common.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}
