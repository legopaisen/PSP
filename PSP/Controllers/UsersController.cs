using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSP.Controllers
{
    public struct Response
    {
        public int ResponseStat { get; set; }
        public string Description { get; set; }
    }
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Authorize]
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.User = HttpContext.Session.GetString("UserId");
            return View();
        }

        public JsonResult GetUsersList()
        {
            return Json(new Models.Payroll_Users().GetList());
        }

        [HttpPost]
        public JsonResult InsertUser([FromBody] Payroll_Users_MODEL model)
        {
            Response response = new Response();
            using (Payroll_Users user = new Payroll_Users())
            {
                if (user.InsertUser(model) > 0)
                {
                    response.ResponseStat = 1;
                    response.Description = "User saved!";

                    using (Models.AuditTrail audit = new AuditTrail())
                    {
                        audit.Insert(new Models.AuditTrailModel()
                        {
                            Control_No = 2,
                            UserName = HttpContext.Session.GetString("UserId"),
                            Details = "Create User",
                            Date_Time = DateTime.Now
                        });
                    }
                    return Json(response);
                }
                else
                {
                    response.ResponseStat = 0;
                    response.Description = "User failed to save!";
                    return Json(response);
                }
            }
        }

        public JsonResult SearchUser(string UserName)
        {
            return Json(new Models.Payroll_Users().GetUser(UserName));
        }

        public JsonResult SearchEmployeeID(string sEmployeeID)
        {
            CTBC.Network.ActiveDirectory ad = new CTBC.Network.ActiveDirectory("LDAP://chinatrust.com.ph", "Z_ESSDEV", "Welcome1");
            var data = ad.GetUserDetailsSingle(sEmployeeID);
            return Json(data);
        }

        [HttpPost]
        public JsonResult ActivateUser([FromBody] Payroll_Users_MODEL model)
        {
            Response response = new Response();
            using (Payroll_Users user = new Payroll_Users())
            {
                if (user.ActivateUser(model.UserName) == 0)
                {
                    if(model.UserName == HttpContext.Session.GetString("UserId"))
                        response.ResponseStat = 2;
                    else
                        response.ResponseStat = 1;
                   
                    response.Description = "User Deactivated!";
                }
                else
                {
                    response.Description = "User Activated!";
                }
            }
            using (Models.AuditTrail audit = new AuditTrail())
            {
                audit.Insert(new Models.AuditTrailModel()
                {
                    Control_No = 4,
                    UserName = HttpContext.Session.GetString("UserId"),
                    Details = "Delete User",
                    Date_Time = DateTime.Now
                });
            }
            return Json(response);
        }

        [HttpPost]
        public JsonResult EditUser([FromBody] Payroll_Users_MODEL model)
        {
            Response response = new Response();
            using (Payroll_Users user = new Payroll_Users())
            {
                if (user.EditUser(model) > 0)
                {
                    if (model.UserName == HttpContext.Session.GetString("UserId"))
                        response.ResponseStat = 2;
                    else
                        response.ResponseStat = 1;

                    response.Description = "User updated!";
                }
            }
            using (Models.AuditTrail audit = new AuditTrail())
            {
                audit.Insert(new Models.AuditTrailModel()
                {
                    Control_No = 3,
                    UserName = HttpContext.Session.GetString("UserId"),
                    Details = "Edit User",
                    Date_Time = DateTime.Now
                });
            }
            return Json(response);
        }
    }
}
