using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string dbPath = Path.Combine(AppContext.BaseDirectory, "names.db");
string connectionString = $"Data Source={dbPath}";

// Ensure database exists
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
<!DOCTYPE html>
<html>
<head>
    <title>Save My Name</title>
    <meta name='viewport' content='width=device-width, initial-scale=1'>

    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css' rel='stylesheet'>

</head>

<body class='bg-light'>

<div class='container mt-5'>

    <div class='card shadow-sm'>
        <div class='card-body'>

            <h2 class='text-center mb-4'>Save My Name</h2>

            <form method='post' action='/save'>
                <div class='input-group'>
                    <input 
                        type='text' 
                        name='name' 
                        class='form-control' 
                        placeholder='Enter your name' 
                        required>

                    <button 
                        type='submit' 
                        class='btn btn-primary'>
                        Save
                    </button>
                </div>
            </form>

            <div class='text-center mt-3'>
                <a href='/names' class='btn btn-link'>View Saved Names</a>
            </div>

        </div>
    </div>

</div>

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

    var listItems = string.Join("",
        names.Select(n =>
            $"<li class='list-group-item'>{System.Net.WebUtility.HtmlEncode(n)}</li>"));

    var html = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Saved Names</title>

    <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css' rel='stylesheet'>
</head>

<body class='bg-light'>

<div class='container mt-5'>

    <div class='card shadow-sm'>
        <div class='card-body'>

            <h2 class='mb-4'>Saved Names</h2>

            <ul class='list-group mb-4'>
                {listItems}
            </ul>

            <a href='/' class='btn btn-secondary'>Back</a>

        </div>
    </div>

</div>

</body>
</html>";

    return Results.Content(html, "text/html");
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Run($"http://0.0.0.0:{port}");
