using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TtWork.Project.Web.Host.Startup;

public class RealIpMiddleware(RequestDelegate next) {
    public Task Invoke(HttpContext context) {
        var headers = context.Request.Headers;
        if (!headers.TryGetValue("X-Forwarded-For", out var header)) return next(context);
        
        var ip = header.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)[0];
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        return next(context);
    }
}