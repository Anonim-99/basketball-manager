using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HoopsApi.Data;
using HoopsApi.Models;
using HoopsApi.Models.DTOs;
using Microsoft.IdentityModel.Tokens;
using System.Data;

[ApiController] // Handles Web API requests, not web pages
[Route("api/[controller]")] // Sets the URL path
public class TeamsController : ControllerBase // ControllerBase -> gives access to standard HTTP tools
{
    private readonly AppDbContext _context; // private variable to hold our database tool

    public TeamsController(AppDbContext context)
    {
        _context=context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
    {
        var teams = await _context.Teams
        .Include(t => t.Players)
        .ToListAsync();

        return Ok(teams);
    }

    [HttpPost]
    public async Task<ActionResult<Team>> CreateTeam(Team team)
    {
        _context.Teams.Add(team);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTeam), new {id = team.Id}, team);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Team>> GetTeam(int id)
    {
        var team = await _context.Teams
        .Include(t => t.Players)
        .FirstOrDefaultAsync(t => t.Id == id);

        if(team is null)
        {
            return NotFound();
        }

        return Ok(team);
    }

    // BIG HIERARCHICAL REQUEST MISSING !!!

    [HttpPut("{id}")]
    public async Task<ActionResult<Team>> UpdateTeam(int id, Team team)
    {
        if (id != team.Id)
        {
            return BadRequest();
        }

        _context.Entry(team).State=EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch(DBConcurrencyException)
        {
            if(!await _context.Teams.AnyAsync(t => t.Id == id)) // if it gets deleted while trying to insert
            {
                return NotFound();
            }

            throw;
        }

        return Ok(team);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var team=await _context.Teams.FindAsync(id);

        if(team is null)
        {
            return NotFound();
        }

        _context.Teams.Remove(team);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    

}