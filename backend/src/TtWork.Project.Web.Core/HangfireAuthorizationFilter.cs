using System;
using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace TtWork.Project.Web;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter {
    public bool Authorize(DashboardContext context) {
        var httpContext = context.GetHttpContext();
        string header = httpContext.Request.Headers["Authorization"]; //获取授权
        if (header == null)
            return AuthenticateLogin();
        //解析授权
        var authHeader = AuthenticationHeaderValue.Parse(header);
        var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
        var username = credentials[0];
        var password = credentials[1];
        //验证登录
        if (username == "admin" && password == "1q2w3E*")
            return true;
        else
            return AuthenticateLogin();

        //跳转简单登录界面
        bool AuthenticateLogin() {
            httpContext.Response.StatusCode = 401;
            httpContext.Response.Headers.Append("WWW-Authenticate", "Basic realm=\"Hangfire Dashboard\"");
            context.Response.WriteAsync("Authentication is required.");
            return false;
        }
    }
}