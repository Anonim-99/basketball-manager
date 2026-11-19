using HoopsApi.Data;
using HoopsApi.Models;
using HoopsApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HoopsApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeamsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
    {
        var teams = await _context.Teams
            .AsNoTracking()
            .Include(t => t.Players)
            .OrderBy(t => t.Name)
            .ToListAsync();

        return Ok(teams.Select(t => t.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TeamDto>> GetTeam(int id)
    {
        var team = await _context.Teams
            .AsNoTracking()
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id);

        return team is null ? NotFound() : Ok(team.ToDto());
    }

    [HttpGet("{id:int}/games")]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetTeamGames(int id)
    {
        if (!await _context.Teams.AnyAsync(t => t.Id == id))
        {
            return NotFound();
        }

        var games = await _context.Games
            .AsNoTracking()
            .Where(g => g.HomeTeamId == id || g.AwayTeamId == id)
            .OrderByDescending(g => g.Date)
            .ToListAsync();

        var teamNames = await _context.Teams
            .AsNoTracking()
            .ToDictionaryAsync(t => t.Id, t => t.Name);

        return Ok(games.Select(g => g.ToDto(teamNames)));
    }

    [HttpPost]
    public async Task<ActionResult<TeamDto>> CreateTeam(CreateTeamDto dto)
    {
        var name = dto.Name.Trim();

        if (await NameTakenAsync(name))
        {
            return Conflict($"A team named '{name}' already exists.");
        }

        var team = new Team
        {
            Name = name,
            City = dto.City.Trim(),
            CoachName = dto.CoachName.Trim()
        };

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, team.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTeam(int id, UpdateTeamDto dto)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);

        if (team is null)
        {
            return NotFound();
        }

        var name = dto.Name.Trim();

        if (await NameTakenAsync(name, excludingTeamId: id))
        {
            return Conflict($"A team named '{name}' already exists.");
        }

        team.Name = name;
        team.City = dto.City.Trim();
        team.CoachName = dto.CoachName.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team = await _context.Teams.FindAsync(id);

        if (team is null)
        {
            return NotFound();
        }

        if (await _context.Players.AnyAsync(p => p.TeamId == id))
        {
            return Conflict("Team cannot be deleted while it still has players.");
        }

        if (await _context.Games.AnyAsync(g => g.HomeTeamId == id || g.AwayTeamId == id))
        {
            return Conflict("Team cannot be deleted while it is referenced by games.");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> NameTakenAsync(string name, int? excludingTeamId = null)
    {
        var normalized = name.ToLower();

        return await _context.Teams.AnyAsync(t =>
            t.Id != excludingTeamId && t.Name.ToLower() == normalized);
    }
}
