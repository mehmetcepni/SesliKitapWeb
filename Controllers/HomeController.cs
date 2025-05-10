using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SesliKitapWeb.Models;
using SesliKitapWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace SesliKitapWeb.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Son eklenen 4 kitap
        ViewBag.RecentBooks = await _context.Books
            .OrderByDescending(b => b.CreatedAt)
            .Take(4)
            .ToListAsync();

        // En çok kitabı olan 4 kategori
        ViewBag.PopularCategories = await _context.Categories
            .Include(c => c.Books)
            .Take(4)
            .ToListAsync();

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
}
