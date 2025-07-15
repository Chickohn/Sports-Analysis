using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Sports_Analysis.Models;
using Sports_Analysis.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Sports_Analysis.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFootballDataService _footballDataService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFootballDataService footballDataService, ILogger<HomeController> logger)
        {
            _footballDataService = footballDataService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Fetching initial matches data");
                var matches = await _footballDataService.GetMatchesAsync(0, 1000);
                matches = matches.OrderByDescending(m => m.Date).ToList();
                _logger.LogInformation($"Retrieved {matches.Count} matches");

                var viewModel = new FootballMatchViewModel
                {
                    Matches = matches.Take(50).ToList(),
                    CurrentPage = 0,
                    PageSize = 50
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching matches data");
                return View(new FootballMatchViewModel { Matches = new List<FootballMatch>() });
            }
        }

        [HttpGet]
        public async Task<IActionResult> LoadMore(int page, int pageSize = 50)
        {
            try
            {
                _logger.LogInformation($"Loading more matches: page {page}, pageSize {pageSize}");
                var matches = await _footballDataService.GetMatchesAsync(page * pageSize, pageSize);
                _logger.LogInformation($"Retrieved {matches.Count} matches for page {page}");
                return PartialView("_MatchesTable", matches);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading more matches for page {page}");
                return PartialView("_MatchesTable", new List<FootballMatch>());
            }
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
}
