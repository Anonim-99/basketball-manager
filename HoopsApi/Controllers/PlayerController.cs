using HoopsApi.Data;
using HoopsApi.Models;
using HoopsApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoopsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _context;

    public PlayersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers([FromQuery] int? teamId)
    {
        var query = _context.Players.AsNoTracking();

        if (teamId is not null)
        {
            query = query.Where(p => p.TeamId == teamId);
        }

        var players = await query
            .OrderBy(p => p.TeamId)
            .ThenBy(p => p.JerseyNumber)
            .ToListAsync();

        return Ok(players.Select(p => p.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PlayerDto>> GetPlayer(int id)
    {
        var player = await _context.Players
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        return player is null ? NotFound() : Ok(player.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<PlayerDto>> CreatePlayer(CreatePlayerDto dto)
    {
        if (!await _context.Teams.AnyAsync(t => t.Id == dto.TeamId))
        {
            return BadRequest($"Team {dto.TeamId} does not exist.");
        }

        if (await JerseyTakenAsync(dto.TeamId, dto.JerseyNumber))
        {
            return Conflict($"Jersey number {dto.JerseyNumber} is already taken on team {dto.TeamId}.");
        }

        var player = new Player
        {
            TeamId = dto.TeamId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            JerseyNumber = dto.JerseyNumber,
            Position = dto.Position.Trim(),
            Points = 0,
            Fouls = 0
        };

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayer), new { id = player.Id }, player.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePlayer(int id, UpdatePlayerDto dto)
    {
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == id);

        if (player is null)
        {
            return NotFound();
        }

        if (!await _context.Teams.AnyAsync(t => t.Id == dto.TeamId))
        {
            return BadRequest($"Team {dto.TeamId} does not exist.");
        }

        if (await JerseyTakenAsync(dto.TeamId, dto.JerseyNumber, excludingPlayerId: id))
        {
            return Conflict($"Jersey number {dto.JerseyNumber} is already taken on team {dto.TeamId}.");
        }

        player.TeamId = dto.TeamId;
        player.FirstName = dto.FirstName.Trim();
        player.LastName = dto.LastName.Trim();
        player.JerseyNumber = dto.JerseyNumber;
        player.Position = dto.Position.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player = await _context.Players.FindAsync(id);

        if (player is null)
        {
            return NotFound();
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> JerseyTakenAsync(int teamId, int jerseyNumber, int? excludingPlayerId = null)
    {
        return await _context.Players.AnyAsync(p =>
            p.TeamId == teamId &&
            p.JerseyNumber == jerseyNumber &&
            p.Id != excludingPlayerId);
    }
}
