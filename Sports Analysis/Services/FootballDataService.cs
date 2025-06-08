using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using Sports_Analysis.Models;

namespace Sports_Analysis.Services
{
    public interface IFootballDataService
    {
        Task<List<FootballMatch>> GetMatchesAsync(int offset = 0, int limit = 100);
        Task<List<FootballMatch>> GetAllMatchesAsync();
    }

    public class FootballDataService : IFootballDataService
    {
        private static List<FootballMatch>? _allMatchesCache = null;
        private static DateTime _cacheTimestamp = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(60);
        private static readonly string CsvPath = Path.Combine("Data", "sorted_football_matches.csv");

        public FootballDataService() { }

        public async Task<List<FootballMatch>> GetMatchesAsync(int offset = 0, int limit = 100)
        {
            var all = await GetAllMatchesAsync();
            return all.Skip(offset).Take(limit).ToList();
        }

        public async Task<List<FootballMatch>> GetAllMatchesAsync()
        {
            if (_allMatchesCache != null && (DateTime.UtcNow - _cacheTimestamp) < CacheDuration)
                return _allMatchesCache;

            // Read and parse CSV
            using var reader = new StreamReader(CsvPath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            });
            var records = csv.GetRecords<FootballMatchCsv>().ToList();
            var matches = records.Select(r => r.ToFootballMatch()).Where(m => m != null).ToList();
            _allMatchesCache = matches;
            _cacheTimestamp = DateTime.UtcNow;
            return matches;
        }
    }

    // CSV mapping class
    public class FootballMatchCsv
    {
        [Name("X")]
        public int X { get; set; }
        [Name("season")]
        public string Season { get; set; } = "";
        [Name("date")]
        public string Date { get; set; } = "";
        [Name("home_team")]
        public string Home_Team { get; set; } = "";
        [Name("away_team")]
        public string Away_Team { get; set; } = "";
        [Name("home_clearances")]
        public int Home_Clearances { get; set; }
        [Name("home_corners")]
        public int Home_Corners { get; set; }
        [Name("home_fouls_conceded")]
        public int Home_Fouls_Conceded { get; set; }
        [Name("home_offsides")]
        public int Home_Offsides { get; set; }
        [Name("home_passes")]
        public int Home_Passes { get; set; }
        [Name("home_possession")]
        public double Home_Possession { get; set; }
        [Name("home_red_cards")]
        public int Home_Red_Cards { get; set; }
        [Name("home_shots")]
        public int Home_Shots { get; set; }
        [Name("home_shots_on_target")]
        public int Home_Shots_On_Target { get; set; }
        [Name("home_tackles")]
        public int Home_Tackles { get; set; }
        [Name("home_touches")]
        public int Home_Touches { get; set; }
        [Name("home_yellow_cards")]
        public int Home_Yellow_Cards { get; set; }
        [Name("away_clearances")]
        public int Away_Clearances { get; set; }
        [Name("away_corners")]
        public int Away_Corners { get; set; }
        [Name("away_fouls_conceded")]
        public int Away_Fouls_Conceded { get; set; }
        [Name("away_offsides")]
        public int Away_Offsides { get; set; }
        [Name("away_passes")]
        public int Away_Passes { get; set; }
        [Name("away_possession")]
        public double Away_Possession { get; set; }
        [Name("away_red_cards")]
        public int Away_Red_Cards { get; set; }
        [Name("away_shots")]
        public int Away_Shots { get; set; }
        [Name("away_shots_on_target")]
        public int Away_Shots_On_Target { get; set; }
        [Name("away_tackles")]
        public int Away_Tackles { get; set; }
        [Name("away_touches")]
        public int Away_Touches { get; set; }
        [Name("away_yellow_cards")]
        public int Away_Yellow_Cards { get; set; }
        [Name("goal_home_ft")]
        public int Goal_Home_Ft { get; set; }
        [Name("goal_away_ft")]
        public int Goal_Away_Ft { get; set; }
        [Name("sg_match_ft")]
        public int Sg_Match_Ft { get; set; }
        [Name("result")]
        public int Result { get; set; }

        public FootballMatch ToFootballMatch()
        {
            DateTime matchDate;
            // Try parsing with yyyy-MM-dd, fallback to yyyy/MM/dd, else skip
            if (!DateTime.TryParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out matchDate) &&
                !DateTime.TryParseExact(Date, "yyyy/MM/dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out matchDate))
            {
                // If parsing fails, return null to indicate skip
                return null;
            }
            return new FootballMatch
            {
                Id = X,
                Season = Season,
                Date = matchDate,
                HomeTeam = Home_Team,
                AwayTeam = Away_Team,
                HomeClearances = Home_Clearances,
                HomeCorners = Home_Corners,
                HomeFoulsConceded = Home_Fouls_Conceded,
                HomeOffsides = Home_Offsides,
                HomePasses = Home_Passes,
                HomePossession = Home_Possession,
                HomeRedCards = Home_Red_Cards,
                HomeShots = Home_Shots,
                HomeShotsOnTarget = Home_Shots_On_Target,
                HomeTackles = Home_Tackles,
                HomeTouches = Home_Touches,
                HomeYellowCards = Home_Yellow_Cards,
                AwayClearances = Away_Clearances,
                AwayCorners = Away_Corners,
                AwayFoulsConceded = Away_Fouls_Conceded,
                AwayOffsides = Away_Offsides,
                AwayPasses = Away_Passes,
                AwayPossession = Away_Possession,
                AwayRedCards = Away_Red_Cards,
                AwayShots = Away_Shots,
                AwayShotsOnTarget = Away_Shots_On_Target,
                AwayTackles = Away_Tackles,
                AwayTouches = Away_Touches,
                AwayYellowCards = Away_Yellow_Cards,
                HomeGoals = Goal_Home_Ft,
                AwayGoals = Goal_Away_Ft,
                GoalDifference = Sg_Match_Ft,
                Result = Result
            };
        }
    }
} 