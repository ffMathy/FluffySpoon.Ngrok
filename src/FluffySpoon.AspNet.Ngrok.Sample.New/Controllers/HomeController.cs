using FluffySpoon.AspNet.Ngrok.New;
using Microsoft.AspNetCore.Mvc;

namespace FluffySpoon.AspNet.Ngrok.Sample.New.Controllers;

public class HomeController : Controller
{
    private readonly INgrokHostedService _ngrokService;

    public HomeController(
        INgrokHostedService ngrokService)
    {
        _ngrokService = ngrokService;
    }

    public async Task<IActionResult> Index()
    {
        return View(_ngrokService.ActiveTunnel);
    }
}