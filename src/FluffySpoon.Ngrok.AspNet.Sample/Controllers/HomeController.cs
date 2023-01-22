using Microsoft.AspNetCore.Mvc;

namespace FluffySpoon.Ngrok.AspNet.Sample.Controllers;

public class HomeController : Controller
{
    private readonly INgrokService _ngrokService;

    public HomeController(
        INgrokService ngrokService)
    {
        _ngrokService = ngrokService;
    }

    public Task<IActionResult> Index()
    {
        return Task.FromResult<IActionResult>(View(_ngrokService.ActiveTunnels.SingleOrDefault()));
    }
}