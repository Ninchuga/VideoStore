using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace VideoStore.Shared
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate requestDelegate, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = requestDelegate;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request was cancelled");
                httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
            }
            catch (Exception ex)
            {
                _logger.LogError("Unhandled exception occurred with message {Message}", ex.Message);
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // In case we want to return object as a response
                //var errorResponse = new ObjectResult(ex);

                //httpContext.Response.ContentType = "application/json";
                //httpContext.Response.StatusCode = (int)errorResponse.StatusCode;

                //var result = JsonSerializer.Serialize(errorResponse);
                //await httpContext.Response.WriteAsync(result);
            }
        }
    }
}
