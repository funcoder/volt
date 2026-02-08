using Microsoft.AspNetCore.Mvc;

namespace VoltApp.Web.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();
}
