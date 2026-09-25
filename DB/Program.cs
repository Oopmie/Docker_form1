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
    
    var requests = await conn.QueryAsync<UserRequest>(
        "SELECT id, full_name, phone, email, comment FROM requests ORDER BY id DESC;");

    var sb = new StringBuilder();
    sb.Append(@"
    <!DOCTYPE html>
    <html lang='ru'>
    <head>
        <meta charset='UTF-8'>
        <title>Панель просмотра (Бэкенд)</title>
        <style>
            body { font-family: Arial, sans-serif; margin: 40px; background-color: #f4f4f9; }
            .container { max-width: 800px; margin: 0 auto; }
            table { width: 100%; border-collapse: collapse; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); margin-top: 20px; }
            th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; font-size: 14px; }
            th { background-color: #007bff; color: white; }
            tr:hover { background-color: #f1f1f1; }
            .btn { display: inline-block; background: #007bff; color: white; padding: 10px 15px; text-decoration: none; border-radius: 4px; font-weight: bold; margin-bottom: 10px; }
            .btn:hover { background-color: #0056b3; }
            .no-data { text-align: center; color: #777; font-style: italic; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.05); }
        </style>
    </head>
    <body>
        <div class='container'>
            <h2>Второй контейнер: Архив всех заявок в БД</h2>
            <a href='/' class='btn'>🔄 Обновить архив</a>");

    if (!requests.Any())
    {
        sb.Append("<div class='no-data'>В базе данных пока нет сохраненных заявок.</div>");
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
                    <th>Текст</th>
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

app.Run("http://0.0.0");

public record UserRequest(int id, string full_name, string phone, string email, string comment);
