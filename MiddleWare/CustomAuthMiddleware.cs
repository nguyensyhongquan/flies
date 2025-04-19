using Microsoft.AspNetCore.Authorization;

namespace FliesProject.MiddleWare
{
    public class CustomAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomAuthMiddleware> _logger;

        public CustomAuthMiddleware(RequestDelegate next, ILogger<CustomAuthMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            
            var endpoint = httpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() != null)
            {
             
                await _next(httpContext);
                return;
            }

      
            string userRole = httpContext.Session.GetString("UserRole");

            Console.WriteLine("the user roleeeeeeeeeeeeeeeee"+userRole);
         
            if (string.IsNullOrEmpty(userRole) ||
                (!userRole.Equals("admin", StringComparison.OrdinalIgnoreCase) &&
                 !userRole.Equals("mentor", StringComparison.OrdinalIgnoreCase) &&
                 !userRole.Equals("student", StringComparison.OrdinalIgnoreCase)))
            {
                //_logger.LogWarning("Forbidden access attempt by user with role: {UserRole}", userRole);
                httpContext.Response.Redirect("/Account/Home");  
                return;
            }

            await _next(httpContext); 
        }

    }
}
