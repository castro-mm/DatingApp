using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using API.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            this._env = env;
            this._logger = logger;
            this._next = next;            
        }

        public async Task InvokeAsync(HttpContext httpContext) 
        {
            try 
            {
                await this._next(httpContext);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                httpContext.Response.ContentType = "applicaton/json";
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new ApiException(httpContext.Response.StatusCode, ex.Message, this._env.IsDevelopment() ? ex.StackTrace?.ToString() : "Internal Server Error");

                var options = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};

                var json = JsonSerializer.Serialize(response, options);

                await httpContext.Response.WriteAsync(json);
            }

        }        
    }
}