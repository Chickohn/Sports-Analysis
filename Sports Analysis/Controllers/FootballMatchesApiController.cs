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

        [HttpPost]
        public async Task<IActionResult> AddMatch([FromBody] FootballMatch match)
        {
            if (match == null)
                return BadRequest();
            _context.FootballMatches.Add(match);
            await _context.SaveChangesAsync();
            return Ok(match);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatch(int id)
        {
            var match = await _context.FootballMatches.FindAsync(id);
            if (match == null)
                return NotFound();
            _context.FootballMatches.Remove(match);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}