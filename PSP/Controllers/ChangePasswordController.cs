using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSP.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Authorize]
    public class ChangePasswordController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.User = HttpContext.Session.GetString("UserId");
            TempData["UserName"] = HttpContext.Session.GetString("UserId");
            return View();
        }

        //[HttpPost]
        //public JsonResult ChangePassword([FromBody] Payroll_Users_MODEL model)
        //{
        //    Response response = new Response();
        //    using (Payroll_Users user = new Payroll_Users())
        //    {
        //        if (user.ChangePassword(model) > 0)
        //        {
        //            response.ResponseStat = 1;
        //            response.Description = "Password changed!";
        //        }
        //    }
        //    return Json(response);
        //}


    }
}
