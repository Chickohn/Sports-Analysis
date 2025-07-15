using CsvHelper.Configuration;

namespace Sports_Analysis.Models
{
    public class FootballMatchMap : ClassMap<FootballMatch>
    {
        public FootballMatchMap()
        {
            Map(m => m.Season).Name("season");
            Map(m => m.Date).Name("date");
            Map(m => m.HomeTeam).Name("home_team");
            Map(m => m.AwayTeam).Name("away_team");
            Map(m => m.HomeClearances).Name("home_clearances");
            Map(m => m.HomeCorners).Name("home_corners");
            Map(m => m.HomeFoulsConceded).Name("home_fouls_conceded");
            Map(m => m.HomeOffsides).Name("home_offsides");
            Map(m => m.HomePasses).Name("home_passes");
            Map(m => m.HomePossession).Name("home_possession");
            Map(m => m.HomeRedCards).Name("home_red_cards");
            Map(m => m.HomeShots).Name("home_shots");
            Map(m => m.HomeShotsOnTarget).Name("home_shots_on_target");
            Map(m => m.HomeTackles).Name("home_tackles");
            Map(m => m.HomeTouches).Name("home_touches");
            Map(m => m.HomeYellowCards).Name("home_yellow_cards");
            Map(m => m.AwayClearances).Name("away_clearances");
            Map(m => m.AwayCorners).Name("away_corners");
            Map(m => m.AwayFoulsConceded).Name("away_fouls_conceded");
            Map(m => m.AwayOffsides).Name("away_offsides");
            Map(m => m.AwayPasses).Name("away_passes");
            Map(m => m.AwayPossession).Name("away_possession");
            Map(m => m.AwayRedCards).Name("away_red_cards");
            Map(m => m.AwayShots).Name("away_shots");
            Map(m => m.AwayShotsOnTarget).Name("away_shots_on_target");
            Map(m => m.AwayTackles).Name("away_tackles");
            Map(m => m.AwayTouches).Name("away_touches");
            Map(m => m.AwayYellowCards).Name("away_yellow_cards");
            Map(m => m.HomeGoals).Name("goal_home_ft");
            Map(m => m.AwayGoals).Name("goal_away_ft");
            Map(m => m.GoalDifference).Name("sg_match_ft");
            Map(m => m.Result).Name("result");
        }
    }
}