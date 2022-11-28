using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PSP.Controllers
{
    [Authorize]
    public class ParameterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
