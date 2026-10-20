namespace Orders.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated. Lets the Application layer
/// translate rule breaches into a clean error response.
/// </summary>
public class DomainException(string message) : Exception(message);
