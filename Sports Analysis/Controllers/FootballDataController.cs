using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_Analysis.Data;
using Sports_Analysis.Models;

namespace Sports_Analysis.Controllers
{
    public class FootballDataController : Controller
    {
        private readonly FootballDbContext _context;

        public FootballDataController(FootballDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var matches = await _context.FootballMatches.ToListAsync();
            return View(matches);
        }
    }
    public class AdminController : Controller
    {
        private readonly CsvImportService _importService;

        public AdminController(CsvImportService importService)
        {
            _importService = importService;
        }

        public IActionResult Import()
        {
            string path = Path.Combine("Data", "sorted_football_matches.csv");
            _importService.ImportFootballMatches(path);
            return Content("Import complete!");
        }
    }
} 