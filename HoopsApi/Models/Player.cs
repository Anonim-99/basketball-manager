namespace HoopsApi.Models;

public class Player
{
    public int Id {get; set;}

    //Foreign key pointing to the team
    public int TeamId{get;set;}
    
    //Player details
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public string Position { get; set; } = string.Empty;

    //In-Game Statistics
    public int Points{get; set;} = 0;
    public int Fouls{get; set;} = 0;

}