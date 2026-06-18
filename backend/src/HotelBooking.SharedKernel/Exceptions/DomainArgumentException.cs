namespace HotelBooking.SharedKernel.Exceptions;

public class DomainArgumentException : ArgumentException
{
    public DomainArgumentException(string message) : base(message)
    {
    }

    public DomainArgumentException(string message, string paramName) : base(message, paramName)
    {
    }
}
