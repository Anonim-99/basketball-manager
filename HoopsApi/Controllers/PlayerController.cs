using HoopsApi.Data;
using HoopsApi.Models;
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
        _context=context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Player>>> GetPlayers()
    {
        var players=await _context.Players.ToListAsync();

        return Ok(players);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Player>> GetPlayer(int id)
    {
        var player=await _context.Players.FindAsync(id);

        if(player is null)
        {
            return NotFound();
        }

        return Ok(player);
    }

    [HttpPost]
    public async Task<ActionResult<Player>> CreatePlayer(Player player)
    {
        _context.Players.Add(player);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayer), new{id = player.Id}, player);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(int id, Player player)
    {
        if (id != player.Id)
        {
            return BadRequest();
        }

        _context.Entry(player).State=EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if(!await _context.Players.AnyAsync(p => p.Id == id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var player=await _context.Players.FindAsync(id);

        if(player is null)
        {
            return NotFound();
        }

        _context.Players.Remove(player);

        await _context.SaveChangesAsync();

        return NoContent();
    }

}