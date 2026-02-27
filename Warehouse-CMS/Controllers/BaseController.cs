using Microsoft.AspNetCore.Mvc;

namespace Warehouse_CMS.Controllers
{
    public abstract class BaseController : Controller
    {
        protected bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        protected IActionResult ViewOrPartial(string partialViewName, object? model = null)
        {
            if (IsAjaxRequest())
            {
                return PartialView(partialViewName, model);
            }

            return View(model);
        }

        protected IActionResult JsonOrRedirect(string redirectAction)
        {
            if (IsAjaxRequest())
            {
                return Json(new { success = true, redirectUrl = Url.Action(redirectAction) });
            }

            return RedirectToAction(redirectAction);
        }
    }
}
