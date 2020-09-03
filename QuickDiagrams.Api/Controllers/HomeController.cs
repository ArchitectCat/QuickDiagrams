using Microsoft.AspNetCore.Mvc;
using QuickDiagrams.Api.Models;
using System.Diagnostics;

namespace QuickDiagrams.Api.Controllers
{
    [Route("[controller]/[action]")]
    public class HomeController : Controller
    {
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}