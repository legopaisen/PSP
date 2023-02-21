using CTBC.Network;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PSP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

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
            ViewBag.CryptoHashCode = new SystemCore().CreateSessionHash();
            return View();
        }
        public IActionResult Login(string userid, string pwd)
        {
            if (ModelState.IsValid)
            { }
            if (User.Identity.IsAuthenticated)
            {
                return (RedirectToAction("Index", "Home"));
            }
            else
            {
                string strIP = Response.HttpContext.GetServerVariable("HTTP_X_FORWARDED_FOR");

                if (string.IsNullOrEmpty(strIP))
                {
                    strIP = Response.HttpContext.GetServerVariable("REMOTE_ADDR");
                }
                if (string.IsNullOrEmpty(strIP))
                {
                    strIP = Response.HttpContext.Connection.LocalIpAddress.ToString();
                }
                if (string.IsNullOrEmpty(strIP) || strIP.Trim() == "::1")
                {
                    strIP = "127.0.0.1";
                }

                //string strNetworkID = new SystemCore().DecryptStringAES(Request.Form["hdnUserIDEncrypted"].ToString(), Request.Form["HashCode"].ToString());
                //string strPassword = new SystemCore().DecryptStringAES(Request.Form["hdnPasswordEncrypted"].ToString(), Request.Form["HashCode"].ToString());
                string strNetworkID = userid;
                string strPassword = pwd;

                CTBC.Cryptography.AES crypto = new CTBC.Cryptography.AES(SystemCore.SecurityKey);

                string strLDAPPath = "LDAP://chinatrust.com.ph";
                string strLDAPUsername = "57kfZnu8PbRQDLMcXz+EJg==";
                string strLDAPPassword = "PrMYssXe9/K59cERN3IMeA==";

                if (new Payroll_Users().GetList().Where(x => (x.UserName.Equals(userid))).ToList().Count > 0)
                {
                    ActiveDirectory ad = new ActiveDirectory(strLDAPPath, crypto.Decrypt(strLDAPUsername), crypto.Decrypt(strLDAPPassword));
                    if (ad.ErrorException == null)
                    {
                        ActiveDirectory.UserDetails details = ad.GetUserDetailsSingle(strNetworkID);
                        if (!details.IsLockout && !details.IsAccountExpired && !details.IsAccountDisabled)
                        {
                            if (!CTBC.Network.Credential.Logon(strNetworkID, "CTCBPH_GL2", strPassword))
                            {
                                //response.ResponseStat = 1;
                                //response.Description = "Authentication Failed.";
                            }
                            else
                            {
                                var claims = new List<Claim>
                                {
                                new Claim(ClaimTypes.Name, "userid")
                                };

                                if (User.Identity.IsAuthenticated)
                                {
                                    return RedirectToAction("Index", "Home");
                                }
                                var claimIdentity = new ClaimsIdentity(claims, userid);
                                var ClaimsPrincipal = new ClaimsPrincipal(claimIdentity);
                                var authenticationProperty = new AuthenticationProperties
                                {
                                    IsPersistent = false,
                                };
                                HttpContext.SignInAsync(ClaimsPrincipal, authenticationProperty);

                                using (Models.AuditTrail audit = new AuditTrail())
                                {
                                    audit.Insert(new Models.AuditTrailModel()
                                    {
                                        Control_No = 1,
                                        UserName = userid,
                                        Details = "Login-Success",
                                        Date_Time = DateTime.Now
                                    });
                                }

                                HttpContext.Session.SetString("UserId", userid);
                            }
                        }
                        else
                        {
                            // if user account is locked expired or disabled
                            TempData["Login"] = "Network ID is locked out. Please contact System Administrator.";
                        }
                    }
                    else
                    {//if active directory error
                        TempData["Login"] = "LDAP Error: " + ad.ErrorException.Message.Replace("\r\n", "");
                    }

                }
                else if (new Payroll_Users().GetList().Where(x => (x.UserName.Equals(userid) && x.Active.Equals(0))).ToList().Count > 0)
                {
                    TempData["Login"] = "User is deactivated. Please contact System Administrator";
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
                }
            }
            return RedirectToAction("Index", "Login");
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Login");
        }
    }
}
