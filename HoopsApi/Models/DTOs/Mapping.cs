using HoopsApi.Models;

namespace HoopsApi.Models.DTOs;

public static class Mappings
{
    public static PlayerSummaryDto ToSummaryDto(this Player p) =>
        new(p.Id, p.FirstName, p.LastName, p.JerseyNumber, p.Position);

    public static TeamDto ToDto(this Team t) =>
        new(t.Id, t.Name, t.City, t.CoachName,
            t.Players.Select(p => p.ToSummaryDto()).ToList());

    public static PlayerDto ToDto(this Player p) =>
        new(p.Id, p.TeamId, p.FirstName, p.LastName, p.JerseyNumber,
            p.Position, p.Points, p.Fouls);

    public static GameDto ToDto(this Game g, IReadOnlyDictionary<int, string> teamNames) =>
        new(g.Id,
            g.HomeTeamId, teamNames.GetValueOrDefault(g.HomeTeamId, "Unknown"),
            g.AwayTeamId, teamNames.GetValueOrDefault(g.AwayTeamId, "Unknown"),
            g.HomeScore, g.AwayScore, g.CurrentQuarter, g.Status, g.Date);
}