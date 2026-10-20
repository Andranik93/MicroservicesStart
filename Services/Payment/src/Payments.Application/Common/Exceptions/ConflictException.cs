namespace Payments.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request conflicts with existing state - e.g. creating a
/// second payment for an order that already has one. Lets the API return
/// 409, which is distinct from 400: the request is well-formed, but it
/// cannot be applied given what already exists.
/// </summary>
public class ConflictException(string message) : Exception(message);
