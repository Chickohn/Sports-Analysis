using Sports_Analysis.Models;
using Microsoft.EntityFrameworkCore;

namespace Sports_Analysis.Data
{
    public class SportsAppContext : DbContext
    {
        public SportsAppContext(DbContextOptions<SportsAppContext> options)
            : base(options)
        {
        }
        public DbSet<Stats> Stats { get; set; }
    }
}
