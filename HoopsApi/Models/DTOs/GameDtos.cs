using System.ComponentModel.DataAnnotations;

namespace HoopsApi.Models.DTOs;

public record GameDto(
    int Id,
    int HomeTeamId,
    string HomeTeamName,
    int AwayTeamId,
    string AwayTeamName,
    int HomeScore,
    int AwayScore,
    int CurrentQuarter,
    string Status,
    DateTime Date);

public class CreateGameDto
{
    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; set; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; set; }

    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
}

public class UpdateGameDto
{
    [Range(1, int.MaxValue)]
    public int HomeTeamId { get; set; }

    [Range(1, int.MaxValue)]
    public int AwayTeamId { get; set; }

    [Range(0, 500)]
    public int HomeScore { get; set; }

    [Range(0, 500)]
    public int AwayScore { get; set; }

    [Range(1, 4)]
    public int CurrentQuarter { get; set; }

    [AllowedValues("Scheduled", "In Progress", "Finished")]
    public string Status { get; set; } = "Scheduled";

    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;
}
