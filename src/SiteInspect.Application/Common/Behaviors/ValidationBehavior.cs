using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SiteInspect.Application.Common.Exceptions;
using SiteInspect.Application.Common.Validation;

namespace SiteInspect.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorList = validators as IValidator<TRequest>[] ?? validators.ToArray();
        if (validatorList.Length == 0)
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validatorList.Select(validator => validator.ValidateAsync(context, cancellationToken)));

        var validationResult = new ValidationResult(
            validationResults.SelectMany(result => result.Errors));
        var errors = validationResult.ToErrors();

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return await next(cancellationToken);
    }
}
