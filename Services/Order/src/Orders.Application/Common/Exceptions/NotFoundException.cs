namespace Orders.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist. Lets the API return 404.
/// </summary>
public class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with id '{key}' was not found.");
