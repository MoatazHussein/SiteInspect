namespace SiteInspect.Application.Common.Exceptions;

public sealed class PersistenceConcurrencyException(Exception innerException)
    : Exception("The persisted resource was changed by another operation.", innerException);
