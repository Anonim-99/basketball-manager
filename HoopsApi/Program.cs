using Microsoft.EntityFrameworkCore;
using HoopsApi.Data;
using Scalar.AspNetCore;
using HoopsApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Ensure database exists
    await context.Database.EnsureCreatedAsync();

    if (!await context.Teams.AnyAsync())
    {
        // 1. Seed 6 Teams
        var celtics = new Team { Name = "Boston Celtics", City = "Boston", CoachName = "Joe Mazzulla" };
        var lakers = new Team { Name = "Los Angeles Lakers", City = "Los Angeles", CoachName = "JJ Redick" };
        var warriors = new Team { Name = "Golden State Warriors", City = "San Francisco", CoachName = "Steve Kerr" };
        var bucks = new Team { Name = "Milwaukee Bucks", City = "Milwaukee", CoachName = "Doc Rivers" };
        var nuggets = new Team { Name = "Denver Nuggets", City = "Denver", CoachName = "Michael Malone" };
        var mavericks = new Team { Name = "Dallas Mavericks", City = "Dallas", CoachName = "Jason Kidd" };

        await context.Teams.AddRangeAsync(celtics, lakers, warriors, bucks, nuggets, mavericks);
        await context.SaveChangesAsync(); // Generates database IDs

        // 2. Seed 18 Players (3 per team)
        var players = new List<Player>
        {
            // Boston Celtics
            new Player { FirstName = "Jayson", LastName = "Tatum", JerseyNumber = 0, Position = "Forward", TeamId = celtics.Id },
            new Player { FirstName = "Jaylen", LastName = "Brown", JerseyNumber = 7, Position = "Guard", TeamId = celtics.Id },
            new Player { FirstName = "Derrick", LastName = "White", JerseyNumber = 9, Position = "Guard", TeamId = celtics.Id },

            // Los Angeles Lakers
            new Player { FirstName = "LeBron", LastName = "James", JerseyNumber = 23, Position = "Forward", TeamId = lakers.Id },
            new Player { FirstName = "Anthony", LastName = "Davis", JerseyNumber = 3, Position = "Center", TeamId = lakers.Id },
            new Player { FirstName = "Austin", LastName = "Reaves", JerseyNumber = 15, Position = "Guard", TeamId = lakers.Id },

            // Golden State Warriors
            new Player { FirstName = "Stephen", LastName = "Curry", JerseyNumber = 30, Position = "Guard", TeamId = warriors.Id },
            new Player { FirstName = "Draymond", LastName = "Green", JerseyNumber = 23, Position = "Forward", TeamId = warriors.Id },
            new Player { FirstName = "Andrew", LastName = "Wiggins", JerseyNumber = 22, Position = "Forward", TeamId = warriors.Id },

            // Milwaukee Bucks
            new Player { FirstName = "Giannis", LastName = "Antetokounmpo", JerseyNumber = 34, Position = "Forward", TeamId = bucks.Id },
            new Player { FirstName = "Damian", LastName = "Lillard", JerseyNumber = 0, Position = "Guard", TeamId = bucks.Id },
            new Player { FirstName = "Khris", LastName = "Middleton", JerseyNumber = 22, Position = "Forward", TeamId = bucks.Id },

            // Denver Nuggets
            new Player { FirstName = "Nikola", LastName = "Jokic", JerseyNumber = 15, Position = "Center", TeamId = nuggets.Id },
            new Player { FirstName = "Jamal", LastName = "Murray", JerseyNumber = 27, Position = "Guard", TeamId = nuggets.Id },
            new Player { FirstName = "Aaron", LastName = "Gordon", JerseyNumber = 50, Position = "Forward", TeamId = nuggets.Id },

            // Dallas Mavericks
            new Player { FirstName = "Luka", LastName = "Doncic", JerseyNumber = 77, Position = "Guard", TeamId = mavericks.Id },
            new Player { FirstName = "Kyrie", LastName = "Irving", JerseyNumber = 11, Position = "Guard", TeamId = mavericks.Id },
            new Player { FirstName = "Klay", LastName = "Thompson", JerseyNumber = 31, Position = "Guard", TeamId = mavericks.Id }
        };

        await context.Players.AddRangeAsync(players);

        // 3. Seed 6 Games
        var games = new List<Game>
        {
            // Finished Games
            new Game 
            { 
                HomeTeamId = celtics.Id, 
                AwayTeamId = lakers.Id, 
                HomeScore = 118, 
                AwayScore = 112, 
                CurrentQuarter = 4, 
                Status = "Finished", 
                Date = DateTime.UtcNow.AddDays(-5) 
            },
            new Game 
            { 
                HomeTeamId = nuggets.Id, 
                AwayTeamId = mavericks.Id, 
                HomeScore = 105, 
                AwayScore = 99, 
                CurrentQuarter = 4, 
                Status = "Finished", 
                Date = DateTime.UtcNow.AddDays(-2) 
            },

            // In-Progress Game
            new Game 
            { 
                HomeTeamId = warriors.Id, 
                AwayTeamId = bucks.Id, 
                HomeScore = 84, 
                AwayScore = 82, 
                CurrentQuarter = 3, 
                Status = "In Progress", 
                Date = DateTime.UtcNow 
            },

            // Upcoming Scheduled Games
            new Game 
            { 
                HomeTeamId = lakers.Id, 
                AwayTeamId = warriors.Id, 
                HomeScore = 0, 
                AwayScore = 0, 
                CurrentQuarter = 1, 
                Status = "Scheduled", 
                Date = DateTime.UtcNow.AddDays(1) 
            },
            new Game 
            { 
                HomeTeamId = bucks.Id, 
                AwayTeamId = celtics.Id, 
                HomeScore = 0, 
                AwayScore = 0, 
                CurrentQuarter = 1, 
                Status = "Scheduled", 
                Date = DateTime.UtcNow.AddDays(3) 
            },
            new Game 
            { 
                HomeTeamId = mavericks.Id, 
                AwayTeamId = nuggets.Id, 
                HomeScore = 0, 
                AwayScore = 0, 
                CurrentQuarter = 1, 
                Status = "Scheduled", 
                Date = DateTime.UtcNow.AddDays(4) 
            }
        };

        await context.Games.AddRangeAsync(games);
        await context.SaveChangesAsync();
    }
}

app.Run();