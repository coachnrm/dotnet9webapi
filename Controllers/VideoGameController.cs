using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VideoGameApi.Data;

namespace VideoGameApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VideoGameController(VideoGameDbContext context) : ControllerBase
    {
        private readonly VideoGameDbContext _context = context;

        [HttpGet]
        public async Task<ActionResult<List<VideoGame>>> GetVideoGames()
        {
            return Ok(await _context.VideoGames.ToListAsync());
        } 

        [HttpGet("{id}")]
        public async Task<ActionResult<VideoGame>> GetVideoGameByid(int id)
        {
            var game = await _context.VideoGames.FindAsync(id);
            if (game is null)
                return NotFound();

            return Ok(game);
        }

        [HttpPost]
        public async Task<ActionResult<VideoGame>> AddVideoGame(VideoGame newGame)
        {
            if (newGame is null)
                return BadRequest();

            _context.VideoGames.Add(newGame);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVideoGameByid), new {id = newGame.Id }, newGame);
        }

        [HttpPost("addmoreone")]
        public async Task<ActionResult<IEnumerable<string>>> AddVideoGames(IEnumerable<VideoGame> newGames)
        {
            if (newGames == null || !newGames.Any())
                return BadRequest("No video games provided.");

            // Clear the change tracker
            _context.ChangeTracker.Clear();

            var createdGames = new List<VideoGame>();
            foreach (var game in newGames)
            {
                if (game == null) continue;
                game.Id = 0; // Reset Id if it's auto-generated
                _context.VideoGames.Add(game);
                createdGames.Add(game);
            }

            await _context.SaveChangesAsync();

            // Generate URIs for each created resource
            var uris = createdGames.Select(game => Url.Action(nameof(GetVideoGameByid), new { id = game.Id })).ToList();

            return Created(string.Join(",", uris), createdGames);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVideoGame(int id, VideoGame updatedGame)
        {
            var game = await _context.VideoGames.FindAsync(id);
            if (game is null)
                return NotFound();

            game.Title = updatedGame.Title;
            game.Platform = updatedGame.Platform;
            game.Developer = updatedGame.Developer;
            game.Publisher = updatedGame.Publisher;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVideoGame(int id)
        {
            var game = await _context.VideoGames.FindAsync(id);
            if (game is null)
                return NotFound();

            _context.VideoGames.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteVideoGames([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("No IDs provided.");

            var games = await _context.VideoGames
                .Where(game => ids.Contains(game.Id))
                .ToListAsync();

            if (!games.Any())
                return NotFound("No matching games found.");

            _context.VideoGames.RemoveRange(games);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}