namespace HRM_Client.Middleware
{
    public class ApiErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiErrorHandlerMiddleware> _logger;

        public ApiErrorHandlerMiddleware(RequestDelegate next, ILogger<ApiErrorHandlerMiddleware> logger)
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
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "API request failed");
                
                // Determine error type and redirect
                if (ex.Message.Contains("401"))
                {
                    context.Response.Redirect("/Error/401");
                }
                else if (ex.Message.Contains("403"))
                {
                    context.Response.Redirect("/Error/403");
                }
                else if (ex.Message.Contains("404"))
                {
                    context.Response.Redirect("/Error/404");
                }
                else
                {
                    context.Response.Redirect("/Error/500");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred");
                context.Response.Redirect("/Error/500");
            }
        }
    }
}

