namespace SiteInspect.Application.Common.Results;

public sealed class Result<T> : Result
{
    private readonly T? value;

    private Result(T value)
        : base(true, Array.Empty<Error>())
    {
        this.value = value;
    }

    private Result(IReadOnlyCollection<Error> errors)
        : base(false, errors)
    {
    }

    public T Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("A failed result does not have a value.");

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Result<T>(value);
    }

    public new static Result<T> Failure(params Error[] errors) => Failure((IEnumerable<Error>)errors);

    public new static Result<T> Failure(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        return new Result<T>(errors.ToArray());
    }
}
