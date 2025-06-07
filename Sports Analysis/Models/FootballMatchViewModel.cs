using System.Collections.Generic;

namespace Sports_Analysis.Models
{
    public class FootballMatchViewModel
    {
        public List<FootballMatch> Matches { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
} 