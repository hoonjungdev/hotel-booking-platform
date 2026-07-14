namespace HotelBooking.SharedKernel.Domain;

/// <summary>
/// Provides identity-based state for a domain entity.
/// </summary>
/// <typeparam name="TId">The strongly typed identifier used by the entity.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets the stable identity of this entity.
    /// </summary>
    public TId Id { get; protected init; } = default!;
}
