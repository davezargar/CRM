using Npgsql;
using server.Records;

namespace server;

public static class TicketRoutes
{
    public static async Task<IResult> GetTickets(HttpContext context, NpgsqlDataSource db)
    {
        string? requesterEmail = context.Session.GetString("Email");

        if (String.IsNullOrEmpty(requesterEmail) || context.Session.GetString("Role") == "customer")
            return Results.Unauthorized();

        List<TicketRecord> tickets = new List<TicketRecord>();
        await using var cmd = db.CreateCommand(
            "SELECT tickets.id, title, status, categories.name, subcategories.name, posted, closed, users.email, tickets.company_id, elevated FROM tickets "
            + "INNER JOIN categories ON tickets.category_id = categories.id "
            + "INNER JOIN subcategories ON tickets.subcategory_id = subcategories.id "
            + "INNER JOIN users ON tickets.company_id = users.company_id WHERE email = $1"
        );
        cmd.Parameters.AddWithValue(requesterEmail);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(
                new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetDateTime(5),
                    reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    reader.GetString(7),
                    reader.GetInt32(8),
                    reader.GetBoolean(9)
                )
            );
        }

        return Results.Ok(tickets);
    }
    
}