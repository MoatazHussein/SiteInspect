using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IEnumerable<Error> errors)
        : base("One or more request validation errors occurred.")
    {
        ArgumentNullException.ThrowIfNull(errors);

        Errors = errors.Distinct().ToArray();
        if (Errors.Count == 0)
        {
            throw new ArgumentException(
                "At least one validation error is required.",
                nameof(errors));
        }
    }

    public IReadOnlyCollection<Error> Errors { get; }
}
