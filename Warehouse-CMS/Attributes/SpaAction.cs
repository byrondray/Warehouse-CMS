using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Warehouse_CMS.Attributes
{
    public class SpaActionFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next
        )
        {
            if (context.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (context.Result is RedirectToActionResult redirectResult)
                {
                    var controllerName =
                        redirectResult.ControllerName
                        ?? context.RouteData.Values["controller"]?.ToString();

                    var actionName = redirectResult.ActionName;

                    var redirectUrl = $"/{controllerName}/{actionName}";

                    if (redirectResult.RouteValues?.ContainsKey("id") == true)
                    {
                        redirectUrl += $"/{redirectResult.RouteValues["id"]}";
                    }

                    context.Result = new JsonResult(new { redirectTo = redirectUrl });
                    context.HttpContext.Response.StatusCode = 200;
                }
                else if (context.Result is ViewResult viewResult)
                {
                    viewResult.ViewData["Layout"] = null;
                }
            }

            await next();
        }
    }
}
