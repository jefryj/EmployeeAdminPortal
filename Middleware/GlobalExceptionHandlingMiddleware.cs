using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next,ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

                if (context.Response.HasStarted)
                {
                    throw;
                }
                int statusCode;
                    string title;
                    switch (ex)
                    {
                    case KeyNotFoundException:
                        statusCode = StatusCodes.Status404NotFound;
                        title = ex.Message;
                        break;
                    case ArgumentException:
                        statusCode = StatusCodes.Status400BadRequest;
                        title = ex.Message;
                        break;
                    case UnauthorizedAccessException:
                        statusCode = StatusCodes.Status401Unauthorized;
                        title = ex.Message;
                        break;
                    default:
                        statusCode = StatusCodes.Status500InternalServerError;
                        title = "An unexpected error occurred.";
                        break;
                    }
                context.Response.Clear();
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Instance = context.Request.Path
                };

                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
