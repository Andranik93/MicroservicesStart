using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Common.Exceptions;
using Payments.Domain.Exceptions;

namespace Payments.Api.ExceptionHandling;

/// <summary>
/// Single place that turns an exception into an HTTP response, so endpoints
/// don't each need their own try/catch. Registered with AddExceptionHandler
/// + UseExceptionHandler() in Program.cs.
/// </summary>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "One or more fields are invalid.",
                GroupErrors(validationException)),

            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                notFoundException.Message,
                null),

            ConflictException conflictException => (
                StatusCodes.Status409Conflict,
                conflictException.Message,
                null),

            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                domainException.Message,
                null),

            // Thrown by ASP.NET Core's own model binder - e.g. a required route or
            // query parameter that the request didn't supply. Framework-level bad
            // input, not a bug, so it's a 400 like everything else above.
            BadHttpRequestException badRequestException => (
                StatusCodes.Status400BadRequest,
                badRequestException.Message,
                null),

            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
        };

        if (errors is not null)
            problemDetails.Extensions["errors"] = errors;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static Dictionary<string, string[]> GroupErrors(ValidationException exception) =>
        exception.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
