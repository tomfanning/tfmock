using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace tfmock.Middleware
{
    public class AllRequestsMiddleware
    {
        public AllRequestsMiddleware(RequestDelegate next)
        {
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Clear();
            
            var obj = new
            {
                protocol = httpContext.Request.Protocol,
                method = httpContext.Request.Method,
                headers = FlattenHeaders(httpContext.Request.Headers),
                body = await ReadBody(httpContext.Request),
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            await httpContext.Response.WriteAsJsonAsync(obj, options: options);
        }

        private static async Task<string> ReadBody(HttpRequest request)
        {
            using var bodyReader = new StreamReader(request.Body);
            var body = await bodyReader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(body))
            {
                return null;
            }

            return body;
        }

        private static Dictionary<string, object> FlattenHeaders(IHeaderDictionary headers)
        {
            var result = new Dictionary<string, object>();

            foreach (var header in headers)
            {
                if (header.Value.Count == 1)
                {
                    result.Add(header.Key, header.Value.Single());
                }
                else
                {
                    result.Add(header.Key, header.Value);
                }
            }

            return result;
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseMyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AllRequestsMiddleware>();
        }
    }
}