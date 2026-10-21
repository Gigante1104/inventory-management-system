using InventarioApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace InventarioApi.Middleware;

// Atrapa cualquier excepción no controlada y responde con un JSON uniforme (ProblemDetails)
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, titulo, detalle) = Traducir(ex);

            if (status >= 500)
                _logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalle
            });
        }
    }

    private static (int Status, string Titulo, string Detalle) Traducir(Exception ex)
    {
        switch (ex)
        {
            case NotFoundException:
                return (StatusCodes.Status404NotFound, "No encontrado", ex.Message);
            case BusinessException:
                return (StatusCodes.Status400BadRequest, "Solicitud inválida", ex.Message);
            case ConflictException:
                return (StatusCodes.Status409Conflict, "Conflicto", ex.Message);
            case UnauthorizedAppException:
                return (StatusCodes.Status401Unauthorized, "No autorizado", ex.Message);
        }

        // Errores de PostgreSQL (pueden venir envueltos dentro de otra excepción)
        var pg = ex as PostgresException ?? ex.InnerException as PostgresException;
        if (pg is not null)
        {
            return pg.SqlState switch
            {
                // P0001 = RAISE EXCEPTION de nuestros procedures (stock insuficiente, producto inexistente...)
                PostgresErrorCodes.RaiseException => (StatusCodes.Status400BadRequest, "Solicitud inválida", pg.MessageText),
                PostgresErrorCodes.UniqueViolation => (StatusCodes.Status409Conflict, "Conflicto", "Ya existe un registro con ese valor"),
                PostgresErrorCodes.ForeignKeyViolation => (StatusCodes.Status409Conflict, "Conflicto", "El registro está relacionado con otros datos"),
                PostgresErrorCodes.CheckViolation => (StatusCodes.Status400BadRequest, "Solicitud inválida", "Algún valor no cumple las reglas de la base de datos"),
                _ => (StatusCodes.Status500InternalServerError, "Error del servidor", "Error al acceder a la base de datos")
            };
        }

        // Nunca mostramos detalles internos al cliente
        return (StatusCodes.Status500InternalServerError, "Error del servidor", "Ocurrió un error inesperado. Intenta de nuevo");
    }
}
