using Microsoft.AspNetCore.Mvc;
using Sports_Analysis.Services;
using System.Threading.Tasks;

namespace Sports_Analysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FootballController : ControllerBase
    {
        private readonly IFootballDataService _footballDataService;

        public FootballController(IFootballDataService footballDataService)
        {
            _footballDataService = footballDataService;
        }

        [HttpGet("matches")]
        public async Task<IActionResult> GetMatches([FromQuery] int offset = 0, [FromQuery] int limit = 100)
        {
            var matches = await _footballDataService.GetMatchesAsync(offset, limit);
            return Ok(matches);
        }
    }
} 