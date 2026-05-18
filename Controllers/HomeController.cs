using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicfyWebApp.Models;

namespace MusicfyWebApp.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Library()
    {
        return View();
    }

    public IActionResult Songs()
    {
        return View();
    }

    public IActionResult Upload()
    {
        return View();
    }

    public IActionResult Profile()
    {
        return View();
    }

    public IActionResult Artist()
    {
        return View();
    }
}