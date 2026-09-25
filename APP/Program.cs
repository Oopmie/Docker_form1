using Dapper;
using Npgsql;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "db";
var connectionString = $"Host={dbHost};Database=mydb;Username=user;Password=password";

app.MapGet("/", async () =>
{
    using var conn = new NpgsqlConnection(connectionString);
    var requests = await conn.QueryAsync<UserRequest>("SELECT id, full_name, phone, email, comment FROM requests ORDER BY id DESC;");

    var sb = new StringBuilder();
    sb.Append(@"
    <!DOCTYPE html>
    <html lang='ru'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Форма заявки</title>
        <style>
            body { font-family: Arial, sans-serif; margin: 40px; background-color: #f4f4f9; color: #333; }
            .container { max-width: 700px; margin: 0 auto; }
            .form-container { background: white; padding: 25px; border-radius: 8px; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin-bottom: 30px; }
            .form-group { margin-bottom: 15px; }
            label { display: block; margin-bottom: 5px; font-weight: bold; }
            input[type='text'], input[type='tel'], input[type='email'], textarea { 
                width: 100%; padding: 10px; box-sizing: border-box; border: 1px solid #ccc; border-radius: 4px; font-size: 14px;
            }
            textarea { resize: vertical; height: 80px; }
            button { background-color: #28a745; color: white; padding: 12px; border: none; border-radius: 4px; cursor: pointer; width: 100%; font-size: 16px; font-weight: bold; }
            button:hover { background-color: #218838; }
            
            table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin-top: 15px; }
            th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; font-size: 14px; }
            th { background-color: #007bff; color: white; }
            tr:hover { background-color: #f1f1f1; }
            .no-data { text-align: center; color: #777; font-style: italic; padding: 20px; background: #fff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='form-container'>
                <h2>Оставить заявку</h2>
                <form method='POST'>
                    <div class='form-group'>
                        <label for='fullName'>ФИО:</label>
                        <input type='text' id='fullName' name='fullName' placeholder='Иванов Иван Иванович' required autocomplete='off'>
                    </div>
                    <div class='form-group'>
                        <label for='phone'>Телефон:</label>
                        <input type='tel' id='phone' name='phone' placeholder='+7 (999) 123-45-67' required autocomplete='off'>
                    </div>
                    <div class='form-group'>
                        <label for='email'>Email:</label>
                        <input type='email' id='email' name='email' placeholder='example@mail.ru' required autocomplete='off'>
                    </div>
                    <div class='form-group'>
                        <label for='comment'>Комментарий:</label>
                        <textarea id='comment' name='comment' placeholder='Ваш текст или комментарий...' required></textarea>
                    </div>
                    <button type='submit'>Отправить форму</button>
                </form>
            </div>

            <h2>Введенные данные на экране (из БД):</h2>");

    if (!requests.Any())
    {
        sb.Append("<div class='no-data'>Данных в базе пока нет.</div>");
    }
    else
    {
        sb.Append(@"
            <table>
                <tr>
                    <th>ID</th>
                    <th>ФИО</th>
                    <th>Телефон</th>
                    <th>Email</th>
                    <th>Комментарий</th>
                </tr>");

        foreach (var req in requests)
        {
            sb.Append("<tr>");
            sb.Append($"<td>{req.id}</td>");
            sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(req.full_name)}</td>");
            sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(req.phone)}</td>");
            sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(req.email)}</td>");
            sb.Append($"<td>{System.Net.WebUtility.HtmlEncode(req.comment)}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");
    }

    sb.Append("</div></body></html>");
    return Results.Content(sb.ToString(), "text/html", Encoding.UTF8);
});

app.MapPost("/", async (HttpContext context) =>
{
    var form = await context.Request.ReadFormAsync();
    
    var requestData = new
    {
        FullName = form["fullName"].ToString(),
        Phone = form["phone"].ToString(),
        Email = form["email"].ToString(),
        Comment = form["comment"].ToString()
    };

    if (!string.IsNullOrEmpty(requestData.FullName))
    {
        using var conn = new NpgsqlConnection(connectionString);
        var sql = "INSERT INTO requests (full_name, phone, email, comment) VALUES (@FullName, @Phone, @Email, @Comment);";
        await conn.ExecuteAsync(sql, requestData);
    }

    context.Response.Redirect("/");
});

app.Run("http://0.0.0.0:80");

public record UserRequest(int id, string full_name, string phone, string email, string comment);
