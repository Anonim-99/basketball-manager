namespace HoopsApi.Models;

public class Game
{
    public int Id {get; set;}

    //IDs of the 2 teams playing
    public int HomeTeamId {get;set;}
    public int AwayTeamId {get; set;}

    //Scoreboard Stats
    public int HomeScore {get; set;} = 0;
    public int AwayScore {get; set;} = 0;
    public int CurrentQuarter{get; set;} = 1;

    // Match Status & Schedule
    public string Status {get; set;} = "Scheduled";
    public DateTime Date {get; set;} = DateTime.UtcNow;


}