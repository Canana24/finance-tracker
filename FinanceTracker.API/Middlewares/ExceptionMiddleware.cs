using FinanceTracker.API.Exceptions;
using System.Net;
using System.Text.Json;

namespace FinanceTracker.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Error no manejado: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                ConflictException => (int)HttpStatusCode.Conflict,// 409
                UnauthorizedException => (int)HttpStatusCode.Unauthorized, //401
                ArgumentException => (int)HttpStatusCode.BadRequest, //400
                _ => (int)HttpStatusCode.InternalServerError  // 500
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new
            {
                status = statusCode,
                message = (statusCode == (int)HttpStatusCode.Conflict || statusCode == (int)HttpStatusCode.Unauthorized) ? ex.Message : "Ocurrió un error interno en el servidor.",
                detail = ex.Message
            };
            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}