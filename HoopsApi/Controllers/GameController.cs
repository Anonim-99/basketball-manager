using HoopsApi.Data;
using HoopsApi.Models;
using HoopsApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoopsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private static readonly string[] AllowedStatuses = { "Scheduled", "In Progress", "Finished" };

    private readonly AppDbContext _context;

    public GamesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames(
        [FromQuery] string? status,
        [FromQuery] int? teamId)
    {
        if (status is not null && !AllowedStatuses.Contains(status))
        {
            return BadRequest($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
        }

        var query = _context.Games.AsNoTracking();

        if (status is not null)
        {
            query = query.Where(g => g.Status == status);
        }

        if (teamId is not null)
        {
            query = query.Where(g => g.HomeTeamId == teamId || g.AwayTeamId == teamId);
        }

        var games = await query
            .OrderByDescending(g => g.Date)
            .ToListAsync();

        var teamNames = await GetTeamNamesAsync();

        return Ok(games.Select(g => g.ToDto(teamNames)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GameDto>> GetGame(int id)
    {
        var game = await _context.Games
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
        {
            return NotFound();
        }

        var teamNames = await GetTeamNamesAsync();

        return Ok(game.ToDto(teamNames));
    }

    [HttpPost]
    public async Task<ActionResult<GameDto>> CreateGame(CreateGameDto dto)
    {
        var error = await ValidateTeamsAsync(dto.HomeTeamId, dto.AwayTeamId);

        if (error is not null)
        {
            return BadRequest(error);
        }

        var game = new Game
        {
            HomeTeamId = dto.HomeTeamId,
            AwayTeamId = dto.AwayTeamId,
            HomeScore = 0,
            AwayScore = 0,
            CurrentQuarter = 1,
            Status = "Scheduled",
            Date = dto.Date.UtcDateTime
        };

        _context.Games.Add(game);
        await _context.SaveChangesAsync();

        var teamNames = await GetTeamNamesAsync();

        return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game.ToDto(teamNames));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGame(int id, UpdateGameDto dto)
    {
        var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
        {
            return NotFound();
        }

        var error = await ValidateTeamsAsync(dto.HomeTeamId, dto.AwayTeamId);

        if (error is not null)
        {
            return BadRequest(error);
        }

        game.HomeTeamId = dto.HomeTeamId;
        game.AwayTeamId = dto.AwayTeamId;
        game.HomeScore = dto.HomeScore;
        game.AwayScore = dto.AwayScore;
        game.CurrentQuarter = dto.CurrentQuarter;
        game.Status = dto.Status;
        game.Date = dto.Date.UtcDateTime;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var game = await _context.Games.FindAsync(id);

        if (game is null)
        {
            return NotFound();
        }

        _context.Games.Remove(game);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<GameDto>>> SearchGames(
        [FromQuery] int teamId,
        [FromQuery] int playerId)
    {
        var playerBelongsToTeam = await _context.Players
            .AnyAsync(p => p.Id == playerId && p.TeamId == teamId);

        if (!playerBelongsToTeam)
        {
            return BadRequest("The specified player does not belong to the specified team.");
        }

        var games = await _context.Games
            .AsNoTracking()
            .Where(g => g.HomeTeamId == teamId || g.AwayTeamId == teamId)
            .OrderByDescending(g => g.Date)
            .ToListAsync();

        var teamNames = await GetTeamNamesAsync();

        return Ok(games.Select(g => g.ToDto(teamNames)));
    }

    private Task<Dictionary<int, string>> GetTeamNamesAsync()
    {
        return _context.Teams
            .AsNoTracking()
            .ToDictionaryAsync(t => t.Id, t => t.Name);
    }

    private async Task<string?> ValidateTeamsAsync(int homeTeamId, int awayTeamId)
    {
        if (homeTeamId == awayTeamId)
        {
            return "Home and away teams cannot be the same.";
        }

        var existingIds = await _context.Teams
            .Where(t => t.Id == homeTeamId || t.Id == awayTeamId)
            .Select(t => t.Id)
            .ToListAsync();

        if (!existingIds.Contains(homeTeamId))
        {
            return $"Home team {homeTeamId} does not exist.";
        }

        if (!existingIds.Contains(awayTeamId))
        {
            return $"Away team {awayTeamId} does not exist.";
        }

        return null;
    }
}
