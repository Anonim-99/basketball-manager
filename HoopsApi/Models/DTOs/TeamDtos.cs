using System.ComponentModel.DataAnnotations;

namespace HoopsApi.Models.DTOs;

public record TeamDto(
    int Id,
    string Name,
    string City,
    string CoachName,
    List<PlayerSummaryDto> Players);

public record PlayerSummaryDto(
    int Id,
    string FirstName,
    string LastName,
    int JerseyNumber,
    string Position);

public class CreateTeamDto
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string CoachName { get; set; } = string.Empty;
}

public class UpdateTeamDto : CreateTeamDto
{
}
