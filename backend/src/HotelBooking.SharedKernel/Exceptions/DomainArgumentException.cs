namespace HotelBooking.SharedKernel.Exceptions;

/// <summary>
/// Represents an invalid argument supplied to domain behavior.
/// </summary>
public class DomainArgumentException : ArgumentException
{
    /// <summary>
    /// Initializes an exception for an invalid domain argument.
    /// </summary>
    /// <param name="message">The domain-focused validation message.</param>
    public DomainArgumentException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes an exception for a named invalid domain argument.
    /// </summary>
    /// <param name="message">The domain-focused validation message.</param>
    /// <param name="paramName">The invalid argument name.</param>
    public DomainArgumentException(string message, string paramName) : base(message, paramName)
    {
    }
}
