namespace HotelBooking.SharedKernel.Exceptions;

/// <summary>
/// Represents a rejected domain rule or state transition.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Initializes an exception with the violated domain rule.
    /// </summary>
    /// <param name="message">The domain-focused failure message.</param>
    public DomainException(string message) : base(message)
    {
    }
}
