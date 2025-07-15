using Microsoft.AspNetCore.Mvc;
using Sports_Analysis.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Sports_Analysis.Controllers
{
    public class AnalysisController : Controller
    {
        private readonly IFootballDataService _footballDataService;
        private readonly ILogger<AnalysisController> _logger;
        private const int BatchSize = 100;
        private const int MaxRows = 1000; // Safety limit

        public AnalysisController(IFootballDataService footballDataService, ILogger<AnalysisController> logger)
        {
            _footballDataService = footballDataService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamsBatch(int offset = 0, int limit = 100)
        {
            try
            {
                var allMatches = await _footballDataService.GetAllMatchesAsync();
                var teams = allMatches.Select(m => m.HomeTeam)
                    .Concat(allMatches.Select(m => m.AwayTeam))
                    .Where(t => !string.IsNullOrEmpty(t) && t != "Unknown")
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList();
                
                _logger.LogInformation("Retrieved {Count} teams in batch", teams.Count);
                return Json(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teams batch");
                return StatusCode(500, "Error retrieving teams");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTeams()
        {
            try
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

                _logger.LogInformation("Retrieved {Count} teams", teams.Count);
                return Json(teams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all teams");
                return StatusCode(500, "Error retrieving teams");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamGoals(string team)
        {
            try
            {
                if (string.IsNullOrEmpty(team))
                {
                    return BadRequest("Team name is required");
                }

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

                _logger.LogInformation("Retrieved goals data for team {Team}", team);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team goals for {Team}", team);
                return StatusCode(500, "Error retrieving team goals");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetWinRate(string team)
        {
            try
            {
                if (string.IsNullOrEmpty(team))
                {
                    return BadRequest("Team name is required");
                }

                var allMatches = await _footballDataService.GetAllMatchesAsync();
                var teamMatches = allMatches.Where(m => m.HomeTeam == team || m.AwayTeam == team).ToList();
                
                int homeWins = teamMatches.Count(m => m.HomeTeam == team && m.Result == 0);
                int awayWins = teamMatches.Count(m => m.AwayTeam == team && m.Result == 1);
                int draws = teamMatches.Count(m => m.Result == 2 && (m.HomeTeam == team || m.AwayTeam == team));

                _logger.LogInformation("Retrieved win rate for team {Team}: {HomeWins} home wins, {AwayWins} away wins, {Draws} draws", 
                    team, homeWins, awayWins, draws);
                
                return Json(new { homeWins, awayWins, draws });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting win rate for {Team}", team);
                return StatusCode(500, "Error retrieving win rate");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPossession(string team)
        {
            try
            {
                if (string.IsNullOrEmpty(team))
                {
                    return BadRequest("Team name is required");
                }

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

                _logger.LogInformation("Retrieved possession stats for team {Team}: {AvgPossession}% vs league average {LeagueAvg}%", 
                    team, avgPossession, leagueAvg);

                return Json(new { avgPossession, leagueAvg });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting possession stats for {Team}", team);
                return StatusCode(500, "Error retrieving possession stats");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCards(string team)
        {
            try
            {
                if (string.IsNullOrEmpty(team))
                {
                    return BadRequest("Team name is required");
                }

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

                _logger.LogInformation("Retrieved cards data for team {Team}", team);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cards data for {Team}", team);
                return StatusCode(500, "Error retrieving cards data");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetShots(string team)
        {
            try
            {
                if (string.IsNullOrEmpty(team))
                {
                    return BadRequest("Team name is required");
                }

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

                _logger.LogInformation("Retrieved shots data for team {Team}", team);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting shots data for {Team}", team);
                return StatusCode(500, "Error retrieving shots data");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPoints(string team)
        {
            try
            {
                if (string.IsNullOrEmpty(team))
                {
                    return BadRequest("Team name is required");
                }

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

                _logger.LogInformation("Retrieved points data for team {Team}: {Points} total points", team, points);
                return Json(pointsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting points data for {Team}", team);
                return StatusCode(500, "Error retrieving points data");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefreshCache()
        {
            try
            {
                var success = await _footballDataService.RefreshCacheAsync();
                if (success)
                {
                    _logger.LogInformation("Cache refreshed successfully");
                    return Ok("Cache refreshed successfully");
                }
                else
                {
                    _logger.LogWarning("Failed to refresh cache");
                    return StatusCode(500, "Failed to refresh cache");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
                return StatusCode(500, "Error refreshing cache");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamMatches(string team)
        {
            if (string.IsNullOrEmpty(team))
                return BadRequest("Team name is required");

            var allMatches = await _footballDataService.GetAllMatchesAsync();
            var teamMatches = allMatches
                .Where(m => m.HomeTeam == team || m.AwayTeam == team)
                .OrderByDescending(m => m.Date)
                .Take(50) // or adjust as needed
                .OrderBy(m => m.Date)
                .ToList();

            return Json(teamMatches);
        }
    }
} 