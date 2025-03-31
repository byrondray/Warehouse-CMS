using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Warehouse_CMS.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class VirtualDomAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.HttpContext.Response.Headers.Add("X-IsVirtualDom", "true");
            }
        }
    }
}
