using System.Reflection.Metadata.Ecma335;
using HoopsApi.Data;
using HoopsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;

namespace HoopsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly AppDbContext _context;

    public GamesController(AppDbContext context)
    {
        _context=context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Game>>> GetGames()
    {
        var games=await _context.Games.ToListAsync();

        return Ok(games);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Game>> GetGame(int id)
    {
        var game=await _context.Games.FindAsync(id);

        if(game is null)
        {
            return NotFound();
        }

        return Ok(game);
    }

    [HttpPost]
    public async Task<ActionResult<Game>> CreateGame(Game game)
    {
        if(game.HomeTeamId == game.AwayTeamId)
        {
            return BadRequest("Home and away teams cannot be the same.");
        }

        var homeTeamExists = await _context.Teams.AnyAsync(t => t.Id == game.HomeTeamId);
        if (!homeTeamExists)
        {
            return BadRequest($"Home team with ID {game.HomeTeamId} does not exist.");
        }

        var awayTeamExists = await _context.Teams.AnyAsync(t => t.Id == game.AwayTeamId);
        if (!awayTeamExists)
        {
            return BadRequest($"Home team with ID {game.AwayTeamId} does not exist.");
        }

        _context.Games.Add(game);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGame), new{id = game.Id}, game);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(int id, Game game)
    {
        if (id != game.Id)
        {
            return BadRequest();
        }

        if (game.HomeTeamId == game.AwayTeamId)
        {
            return BadRequest("Home and away teams cannot be the same.");
        }

        var homeTeamExists = await _context.Teams.AnyAsync(t => t.Id == game.HomeTeamId);

        if (!homeTeamExists)
        {
            return BadRequest($"Home team with ID {game.HomeTeamId} does not exist.");
        }

        var awayTeamExists = await _context.Teams.AnyAsync(t => t.Id == game.AwayTeamId);

        if (!awayTeamExists)
        {
            return BadRequest($"Away team with ID {game.AwayTeamId} does not exist.");
        }

        _context.Entry(game).State=EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(); 
        }
        catch (DbUpdateConcurrencyException)
        {
            if(!await _context.Games.AnyAsync(g => g.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var game=await _context.Games.FindAsync(id);

        if(game is null)
        {
            return NotFound();
        }

        _context.Games.Remove(game);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Game>>> GetGamesByTeamAndPlayer(int teamId, int playerId)
    {
        var playerBelongsToTeam = await _context.Players
            .AnyAsync(p => p.Id == playerId && p.TeamId == teamId);

        if (!playerBelongsToTeam)
        {
            return BadRequest("The specified player does not belong to the specified team.");
        }

        var games = await _context.Games
            .Where(g => g.HomeTeamId == teamId || g.AwayTeamId == teamId)
            .ToListAsync();

        return Ok(games);
    }

    
}