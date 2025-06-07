using System;

namespace Sports_Analysis.Models
{
    public class FootballMatch
    {
        public int Id { get; set; }
        public string? Season { get; set; }
        public DateTime Date { get; set; }
        public string? HomeTeam { get; set; }
        public string? AwayTeam { get; set; }
        
        // Home team statistics
        public int HomeClearances { get; set; }
        public int HomeCorners { get; set; }
        public int HomeFoulsConceded { get; set; }
        public int HomeOffsides { get; set; }
        public int HomePasses { get; set; }
        public double HomePossession { get; set; }
        public int HomeRedCards { get; set; }
        public int HomeShots { get; set; }
        public int HomeShotsOnTarget { get; set; }
        public int HomeTackles { get; set; }
        public int HomeTouches { get; set; }
        public int HomeYellowCards { get; set; }
        
        // Away team statistics
        public int AwayClearances { get; set; }
        public int AwayCorners { get; set; }
        public int AwayFoulsConceded { get; set; }
        public int AwayOffsides { get; set; }
        public int AwayPasses { get; set; }
        public double AwayPossession { get; set; }
        public int AwayRedCards { get; set; }
        public int AwayShots { get; set; }
        public int AwayShotsOnTarget { get; set; }
        public int AwayTackles { get; set; }
        public int AwayTouches { get; set; }
        public int AwayYellowCards { get; set; }
        
        // Match results
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public int GoalDifference { get; set; }
        public int Result { get; set; } // 0: Home Win, 1: Away Win, 2: Draw
    }
} 