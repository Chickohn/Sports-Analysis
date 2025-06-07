using Microsoft.AspNetCore.Mvc;
using Sports_Analysis.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Sports_Analysis.Controllers
{
    public class AnalysisController : Controller
    {
        private readonly IFootballDataService _footballDataService;
        private const int BatchSize = 100;
        private const int MaxRows = 1000; // Safety limit

        public AnalysisController(IFootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamsBatch(int offset = 0, int limit = 100)
        {
            var matches = await _footballDataService.GetMatchesAsync(offset, limit);
            var teams = matches.Select(m => m.HomeTeam)
                .Concat(matches.Select(m => m.AwayTeam))
                .Where(t => !string.IsNullOrEmpty(t) && t != "Unknown")
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            return Json(teams);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            var allMatches = new List<Sports_Analysis.Models.FootballMatch>();
            for (int offset = 0; offset < MaxRows; offset += BatchSize)
            {
                var batch = await _footballDataService.GetMatchesAsync(offset, BatchSize);
                if (batch.Count == 0) break;
                allMatches.AddRange(batch);
                if (batch.Count < BatchSize) break;
            }
            var teams = allMatches.Select(m => m.HomeTeam)
                .Concat(allMatches.Select(m => m.AwayTeam))
                .Where(t => !string.IsNullOrEmpty(t) && t != "Unknown")
                .Distinct()
                .OrderBy(t => t)
                .ToList();
            return Json(teams);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamGoals(string team)
        {
            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches
                .Where(m => m.HomeTeam == team || m.AwayTeam == team)
                .OrderByDescending(m => m.Date)
                .Take(15)
                .OrderBy(m => m.Date)
                .ToList();
            var result = teamMatches.Select(m => new {
                date = m.Date.ToString("yyyy-MM-dd"),
                goals = m.HomeTeam == team ? m.HomeGoals : m.AwayGoals
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetWinRate(string team)
        {
            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches.Where(m => m.HomeTeam == team || m.AwayTeam == team).ToList();
            int homeWins = teamMatches.Count(m => m.HomeTeam == team && m.Result == 0);
            int awayWins = teamMatches.Count(m => m.AwayTeam == team && m.Result == 1);
            int draws = teamMatches.Count(m => m.Result == 2 && (m.HomeTeam == team || m.AwayTeam == team));
            return Json(new { homeWins, awayWins, draws });
        }

        [HttpGet]
        public async Task<IActionResult> GetPossession(string team)
        {
            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches.Where(m => m.HomeTeam == team || m.AwayTeam == team).ToList();
            double avgPossession = 0;
            if (teamMatches.Count > 0)
            {
                avgPossession = teamMatches.Average(m => m.HomeTeam == team ? m.HomePossession : m.AwayPossession);
            }
            // League average
            double leagueAvg = 0;
            if (allMatches.Count > 0)
            {
                leagueAvg = allMatches.Average(m => m.HomePossession);
            }
            return Json(new { avgPossession, leagueAvg });
        }

        [HttpGet]
        public async Task<IActionResult> GetCards(string team)
        {
            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches
                .Where(m => m.HomeTeam == team || m.AwayTeam == team)
                .OrderByDescending(m => m.Date)
                .Take(10)
                .OrderBy(m => m.Date)
                .ToList();
            var result = teamMatches.Select(m => new {
                date = m.Date.ToString("yyyy-MM-dd"),
                yellow = m.HomeTeam == team ? m.HomeYellowCards : m.AwayYellowCards,
                red = m.HomeTeam == team ? m.HomeRedCards : m.AwayRedCards
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetShots(string team)
        {
            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches
                .Where(m => m.HomeTeam == team || m.AwayTeam == team)
                .OrderByDescending(m => m.Date)
                .Take(10)
                .OrderBy(m => m.Date)
                .ToList();
            var result = teamMatches.Select(m => new {
                date = m.Date.ToString("yyyy-MM-dd"),
                shots = m.HomeTeam == team ? m.HomeShots : m.AwayShots,
                shotsOnTarget = m.HomeTeam == team ? m.HomeShotsOnTarget : m.AwayShotsOnTarget
            });
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPoints(string team)
        {
            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches
                .Where(m => m.HomeTeam == team || m.AwayTeam == team)
                .OrderBy(m => m.Date)
                .ToList();
            int points = 0;
            var pointsList = new List<object>();
            foreach (var m in teamMatches)
            {
                if ((m.HomeTeam == team && m.Result == 0) || (m.AwayTeam == team && m.Result == 1))
                    points += 3;
                else if (m.Result == 2)
                    points += 1;
                pointsList.Add(new { date = m.Date.ToString("yyyy-MM-dd"), points });
            }
            return Json(pointsList);
        }
    }
} 