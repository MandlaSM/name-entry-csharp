using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string dbPath = Path.Combine(AppContext.BaseDirectory, "names.db");
string connectionString = $"Data Source={dbPath}";

// Create database table if it doesn't exist
using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Names (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            PersonName TEXT NOT NULL
        );
    ";
    command.ExecuteNonQuery();
}

app.MapGet("/", () =>
{
    return Results.Content(@"
        <html>
        <body>
            <h1>Save My Name</h1>
            <form method='post' action='/save'>
                <input type='text' name='name' placeholder='Enter your name' />
                <button type='submit'>Save</button>
            </form>
            <p><a href='/names'>View saved names</a></p>
        </body>
        </html>
    ", "text/html");
});

app.MapPost("/save", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    var name = form["name"].ToString().Trim();

    if (!string.IsNullOrWhiteSpace(name))
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO Names (PersonName) VALUES ($name)";
        command.Parameters.AddWithValue("$name", name);
        command.ExecuteNonQuery();
    }

    return Results.Redirect("/names");
});

app.MapGet("/names", () =>
{
    var names = new List<string>();

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = "SELECT PersonName FROM Names ORDER BY Id DESC";

    using var reader = command.ExecuteReader();
    while (reader.Read())
    {
        names.Add(reader.GetString(0));
    }

    var listItems = string.Join("", names.Select(n => $"<li>{System.Net.WebUtility.HtmlEncode(n)}</li>"));

    var html = $@"
        <html>
        <body>
            <h1>Saved Names</h1>
            <ul>{listItems}</ul>
            <p><a href='/'>Back</a></p>
        </body>
        </html>
    ";

    return Results.Content(html, "text/html");
});

app.Run();
