using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Sports_Analysis.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Sports_Analysis.Services
{
    public interface IFootballDataService
    {
        Task<List<FootballMatch>> GetMatchesAsync(int offset = 0, int limit = 100);
        Task<List<FootballMatch>> GetAllMatchesAsync();
    }

    public class FootballDataService : IFootballDataService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FootballDataService> _logger;
        private const string BaseUrl = "https://datasets-server.huggingface.co/rows";
        private const int BatchSize = 100;
        private const int MaxRows = 5000;
        private static List<FootballMatch>? _allMatchesCache = null;
        private static DateTime _cacheTimestamp = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public FootballDataService(HttpClient httpClient, ILogger<FootballDataService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<FootballMatch>> GetMatchesAsync(int offset = 0, int limit = 100)
        {
            // Use cache if available and not expired
            if (_allMatchesCache != null && (DateTime.UtcNow - _cacheTimestamp) < CacheDuration)
            {
                return _allMatchesCache.Skip(offset).Take(limit).ToList();
            }
            return await FetchMatchesFromApi(offset, limit);
        }

        public async Task<List<FootballMatch>> GetAllMatchesAsync()
        {
            if (_allMatchesCache != null && (DateTime.UtcNow - _cacheTimestamp) < CacheDuration)
            {
                return _allMatchesCache;
            }
            // Fetch all in batches
            var allMatches = new List<FootballMatch>();
            for (int offset = 0; offset < MaxRows; offset += BatchSize)
            {
                var batch = await FetchMatchesFromApi(offset, BatchSize);
                if (batch.Count == 0) break;
                allMatches.AddRange(batch);
                if (batch.Count < BatchSize) break;
            }
            _allMatchesCache = allMatches;
            _cacheTimestamp = DateTime.UtcNow;
            return allMatches;
        }

        private async Task<List<FootballMatch>> FetchMatchesFromApi(int offset, int limit)
        {
            try
            {
                var url = $"{BaseUrl}?dataset=WideMan%2Ffootball_matches&config=default&split=train&offset={offset}&length={limit}";
                _logger.LogInformation($"Fetching data from URL: {url}");
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received response: {content.Substring(0, Math.Min(200, content.Length))}...");
                
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var data = JsonSerializer.Deserialize<HuggingFaceResponse>(content, options);
                if (data?.Rows == null)
                {
                    _logger.LogWarning("No rows found in the response");
                    return new List<FootballMatch>();
                }

                var matches = new List<FootballMatch>();
                foreach (var row in data.Rows)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(row.Row.Date))
                        {
                            _logger.LogWarning($"Skipping row {row.RowIdx} due to missing date");
                            continue;
                        }

                        if (!DateTime.TryParse(row.Row.Date, out DateTime matchDate))
                        {
                            _logger.LogWarning($"Skipping row {row.RowIdx} due to invalid date format: {row.Row.Date}");
                            continue;
                        }

                        var match = new FootballMatch
                        {
                            Id = row.Row.X,
                            Season = row.Row.Season ?? "Unknown",
                            Date = matchDate,
                            HomeTeam = row.Row.HomeTeam ?? "Unknown",
                            AwayTeam = row.Row.AwayTeam ?? "Unknown",
                            HomeClearances = row.Row.HomeClearances,
                            HomeCorners = row.Row.HomeCorners,
                            HomeFoulsConceded = row.Row.HomeFoulsConceded,
                            HomeOffsides = row.Row.HomeOffsides,
                            HomePasses = row.Row.HomePasses,
                            HomePossession = row.Row.HomePossession,
                            HomeRedCards = row.Row.HomeRedCards,
                            HomeShots = row.Row.HomeShots,
                            HomeShotsOnTarget = row.Row.HomeShotsOnTarget,
                            HomeTackles = row.Row.HomeTackles,
                            HomeTouches = row.Row.HomeTouches,
                            HomeYellowCards = row.Row.HomeYellowCards,
                            AwayClearances = row.Row.AwayClearances,
                            AwayCorners = row.Row.AwayCorners,
                            AwayFoulsConceded = row.Row.AwayFoulsConceded,
                            AwayOffsides = row.Row.AwayOffsides,
                            AwayPasses = row.Row.AwayPasses,
                            AwayPossession = row.Row.AwayPossession,
                            AwayRedCards = row.Row.AwayRedCards,
                            AwayShots = row.Row.AwayShots,
                            AwayShotsOnTarget = row.Row.AwayShotsOnTarget,
                            AwayTackles = row.Row.AwayTackles,
                            AwayTouches = row.Row.AwayTouches,
                            AwayYellowCards = row.Row.AwayYellowCards,
                            HomeGoals = row.Row.GoalHomeFt,
                            AwayGoals = row.Row.GoalAwayFt,
                            GoalDifference = row.Row.SgMatchFt,
                            Result = row.Row.Result
                        };
                        matches.Add(match);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error converting row {row.RowIdx}");
                    }
                }

                _logger.LogInformation($"Successfully converted {matches.Count} matches");
                return matches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching matches data");
                throw;
            }
        }
    }

    // Response models for JSON deserialization
    public class HuggingFaceResponse
    {
        public List<Feature> Features { get; set; } = new();
        public List<RowData> Rows { get; set; } = new();
        public int NumRowsTotal { get; set; }
        public int NumRowsPerPage { get; set; }
        public bool Partial { get; set; }
    }

    public class Feature
    {
        public int FeatureIdx { get; set; }
        public string Name { get; set; } = string.Empty;
        public FeatureType Type { get; set; } = new();
    }

    public class FeatureType
    {
        public string Dtype { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }

    public class RowData
    {
        public int RowIdx { get; set; }
        public MatchData Row { get; set; } = new();
        public List<string> TruncatedCells { get; set; } = new();
    }

    public class MatchData
    {
        [JsonPropertyName("X")]
        public int X { get; set; }
        [JsonPropertyName("season")]
        public string? Season { get; set; }
        [JsonPropertyName("date")]
        public string? Date { get; set; }
        [JsonPropertyName("home_team")]
        public string? HomeTeam { get; set; }
        [JsonPropertyName("away_team")]
        public string? AwayTeam { get; set; }
        [JsonPropertyName("home_clearances")]
        public int HomeClearances { get; set; }
        [JsonPropertyName("home_corners")]
        public int HomeCorners { get; set; }
        [JsonPropertyName("home_fouls_conceded")]
        public int HomeFoulsConceded { get; set; }
        [JsonPropertyName("home_offsides")]
        public int HomeOffsides { get; set; }
        [JsonPropertyName("home_passes")]
        public int HomePasses { get; set; }
        [JsonPropertyName("home_possession")]
        public double HomePossession { get; set; }
        [JsonPropertyName("home_red_cards")]
        public int HomeRedCards { get; set; }
        [JsonPropertyName("home_shots")]
        public int HomeShots { get; set; }
        [JsonPropertyName("home_shots_on_target")]
        public int HomeShotsOnTarget { get; set; }
        [JsonPropertyName("home_tackles")]
        public int HomeTackles { get; set; }
        [JsonPropertyName("home_touches")]
        public int HomeTouches { get; set; }
        [JsonPropertyName("home_yellow_cards")]
        public int HomeYellowCards { get; set; }
        [JsonPropertyName("away_clearances")]
        public int AwayClearances { get; set; }
        [JsonPropertyName("away_corners")]
        public int AwayCorners { get; set; }
        [JsonPropertyName("away_fouls_conceded")]
        public int AwayFoulsConceded { get; set; }
        [JsonPropertyName("away_offsides")]
        public int AwayOffsides { get; set; }
        [JsonPropertyName("away_passes")]
        public int AwayPasses { get; set; }
        [JsonPropertyName("away_possession")]
        public double AwayPossession { get; set; }
        [JsonPropertyName("away_red_cards")]
        public int AwayRedCards { get; set; }
        [JsonPropertyName("away_shots")]
        public int AwayShots { get; set; }
        [JsonPropertyName("away_shots_on_target")]
        public int AwayShotsOnTarget { get; set; }
        [JsonPropertyName("away_tackles")]
        public int AwayTackles { get; set; }
        [JsonPropertyName("away_touches")]
        public int AwayTouches { get; set; }
        [JsonPropertyName("away_yellow_cards")]
        public int AwayYellowCards { get; set; }
        [JsonPropertyName("goal_home_ft")]
        public int GoalHomeFt { get; set; }
        [JsonPropertyName("goal_away_ft")]
        public int GoalAwayFt { get; set; }
        [JsonPropertyName("sg_match_ft")]
        public int SgMatchFt { get; set; }
        [JsonPropertyName("result")]
        public int Result { get; set; }
    }
} 