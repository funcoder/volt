using Microsoft.AspNetCore.Mvc;

namespace VoltApp.Controllers;

public class HomeController : Controller
{
    [HttpGet("/")]
    public IActionResult Index() => View();
}
