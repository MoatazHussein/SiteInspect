namespace SiteInspect.Application.Common.Exceptions;

public sealed class StaleRowVersionException : Exception
{
    public StaleRowVersionException()
        : base("The record was changed by another request.")
    {
    }
}
