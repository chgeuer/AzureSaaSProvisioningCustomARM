namespace LandingPage.Utils
{
    using System;
    using Microsoft.AspNetCore.Mvc.Filters;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class NoCacheAttribute : ActionFilterAttribute
    {
        // Ensure no cache somewhere in ARM believes it could cache
        public override void OnResultExecuting(ResultExecutingContext ctx)
        {
            ctx.HttpContext.Response.Headers["Cache-Control"] = new[] { "no-cache", "no-store", "private" };
            base.OnResultExecuting(ctx);
        }
    }
}