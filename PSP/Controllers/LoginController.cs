using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PSP.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated && !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return (RedirectToAction("Index", "Home"));
            }
            else
            {
                HttpContext.SignOutAsync();
            }
            return View();
        }
        public IActionResult Login(string userid, string pwd)
        {
            if(ModelState.IsValid)
            { }
            if (User.Identity.IsAuthenticated)
            {
                return (RedirectToAction("Index", "Home"));
            }
            else
            {
                
                if (new Payroll_Users().GetList().Where(x => (x.UserName.Equals(userid) && x.Password.Equals(pwd))).ToList().Count > 0)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "claim")
                };

                    if (User.Identity.IsAuthenticated)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    var claimIdentity = new ClaimsIdentity(claims, userid);
                    var ClaimsPrincipal = new ClaimsPrincipal(claimIdentity);
                    var authenticationProperty = new AuthenticationProperties
                    {
                        IsPersistent = false
                    };
                    HttpContext.SignInAsync(ClaimsPrincipal, authenticationProperty);

                    using (Models.AuditTrail audit = new AuditTrail())
                    {
                        audit.Insert(new Models.AuditTrailModel()
                        {
                            UserName = userid,
                            Details = "Login-Success",
                            Date_Time = DateTime.Now
                        });
                    }

                    HttpContext.Session.SetString("UserId", userid);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    using (Models.AuditTrail audit = new AuditTrail())
                    {
                        audit.Insert(new Models.AuditTrailModel()
                        {
                            UserName = userid,
                            Details = "Login-Fail",
                            Date_Time = DateTime.Now
                        });
                    }

                    TempData["Login"] = "Incorrect User/Password!";
                    return RedirectToAction("Index", "Login");
                }
                
            }
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}
