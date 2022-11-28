using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PSP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PSP.Controllers
{
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [Authorize]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.User = HttpContext.Session.GetString("UserId");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
        public JsonResult ProcessFile(string sFileName)
        {
            Response response = new Response();
            if (!sFileName.Substring(sFileName.Length - 4).Contains(".txt"))
            {
                response.ResponseStat = 1;
                response.Description = "File must be in '.txt' format";
                return Json(response);
            }
            string sFilePath = "C:\\Payroll Files\\" + sFileName;
            try
            {
                Decryptor.Decrypt(sFilePath);
            }
            catch (Exception ex)
            {
                response.ResponseStat = 1;
                response.Description = "There is a problem decrypting the file. Please verify the encrypted file";
                return Json(response);
            }

            ODSFunctions odsfunctions = new ODSFunctions();

            response = odsfunctions.StartGeneration(sFileName);

            return Json(response);
        }
    }
}
