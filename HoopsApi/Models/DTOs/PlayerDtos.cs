using System.ComponentModel.DataAnnotations;

namespace HoopsApi.Models.DTOs;

public record PlayerDto(
    int Id,
    int TeamId,
    string FirstName,
    string LastName,
    int JerseyNumber,
    string Position,
    int Points,
    int Fouls);

public class CreatePlayerDto
{
    [Range(1, int.MaxValue)]
    public int TeamId { get; set; }

    [Required, StringLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Range(0, 99)]
    public int JerseyNumber { get; set; }

    [Required, StringLength(20)]
    public string Position { get; set; } = string.Empty;
}

public class UpdatePlayerDto : CreatePlayerDto
{
}