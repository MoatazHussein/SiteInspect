using FluentValidation.Results;
using SiteInspect.Application.Common.Errors;
using SiteInspect.Application.Common.Results;

namespace SiteInspect.Application.Common.Validation;

public static class ValidationResultExtensions
{
    public static IReadOnlyCollection<Error> ToErrors(this ValidationResult validationResult)
    {
        ArgumentNullException.ThrowIfNull(validationResult);

        return validationResult.Errors
            .Select(failure => failure.CustomState as Error ?? CommonErrors.Validation)
            .Distinct()
            .ToArray();
    }
}
