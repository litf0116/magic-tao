using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace TtWork.Project.Web.Authentication.JwtBearer {
    public static class JwtTokenMiddleware {
        public static
#nullable disable
            IApplicationBuilder UseJwtTokenMiddleware(this IApplicationBuilder app, string schema = "Bearer") {
            return app.Use((Func<HttpContext, Func<Task>, Task>)(async (ctx, next) => {
                IIdentity identity = ctx.User.Identity;
                if (identity == null || !identity.IsAuthenticated) {
                    AuthenticateResult result = await ctx.AuthenticateAsync(schema);
                    if (result.Succeeded && result.Principal != null)
                        ctx.User = result.Principal;
                    result = (AuthenticateResult)null;
                }

                await next();
            }));
        }
    }
}