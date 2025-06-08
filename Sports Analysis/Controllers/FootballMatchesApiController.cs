using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sports_Analysis.Data;
using Sports_Analysis.Models;

namespace Sports_Analysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FootballMatchesApiController : ControllerBase
    {
        private readonly FootballDbContext _context;

        public FootballMatchesApiController(FootballDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var matches = await _context.FootballMatches.ToListAsync();
            return Ok(matches);
        }
    }
}