using Microsoft.AspNetCore.Mvc;

namespace TaskManager.CoreMVC.Authorization
{
    public class AuthorizeRouteMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var userId = context.Session.GetString("Id");
            var admin = context.Session.GetString("Admin");

            if(context.Request.Path.ToString() == "/" 
                || context.Request.Path.StartsWithSegments("/Login"))
            {
                await next(context);
                return;
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                context.Response.Redirect("/");
                await next(context);
                return;
            }

            if (context.Request.Path.StartsWithSegments("/" + nameof(Enums.MenuName.Dashboard))
                || admin == "Admin")
            {
                await next(context);
                return;
            }

            context.Session.Remove("Id");
            context.Session.Remove("Admin");
            context.Response.Redirect("/");
            await next(context);
        }
    }
}
