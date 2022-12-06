using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;
using PSP.Models;
using Microsoft.AspNetCore.Http;

namespace PSP.Controllers.Reports
{
    [Authorize]
    public class ReportController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.User = HttpContext.Session.GetString("UserId");
            return View();
        }

        public IActionResult AuditTrail()
        {
            ViewBag.User = HttpContext.Session.GetString("UserId");
            return View();
        }

        public JsonResult AuditTrailReport()
        {
            return Json(new Models.AuditTrail().GetList());
        }

        public IActionResult DownloadViewPDF()
        {
            var model = new AuditTrailModel();
            return new ViewAsPdf("AuditTrail");

        }
    }
}
