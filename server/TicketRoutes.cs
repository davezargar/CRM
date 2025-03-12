using Npgsql;
using Microsoft.AspNetCore.Identity;
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

    
    public record NewTicketRecord(int TicketId, string CategoryName, string SubcategoryName, string Title, string UserEmail, int CompanyFk, int MessageId, string Message);

    public static async Task<IResult> PostTickets(PasswordHasher<string> hasher, HttpContext context,
        NpgsqlDataSource db)
    {
        NewTicketRecord ticketMessages = await context.Request.ReadFromJsonAsync<NewTicketRecord>();

        //if (ticketRequest == null)
        //return Results.BadRequest();
        try
        {
            await using var cmd = db.CreateCommand(
                "WITH ticketIns AS (INSERT INTO tickets(category_id, subcategory_id, title, user_id, company_id) "
                + "values((SELECT id FROM categories WHERE name = $1 AND company_id = $6), (SELECT id FROM subcategories WHERE name = $2 AND main_category_id = (SELECT id FROM categories WHERE name = $1 AND company_id = $6)), $3, (SELECT id FROM users WHERE email = $4 AND company_id = $6), $6) returning id) "
                + "INSERT INTO messages(title, message, ticket_id, user_id) "
                + "values ($3, $5, (SELECT id FROM ticketIns), (SELECT id FROM users WHERE email = $4 AND company_id = $6))"
            );
            cmd.Parameters.AddWithValue(ticketMessages.CategoryName); //$1
            cmd.Parameters.AddWithValue(ticketMessages.SubcategoryName); //$2
            cmd.Parameters.AddWithValue(ticketMessages.Title); //$3
            cmd.Parameters.AddWithValue(ticketMessages.UserEmail); //$4
            cmd.Parameters.AddWithValue(ticketMessages.Message); //$5
            cmd.Parameters.AddWithValue(ticketMessages.CompanyFk); //$6
            await cmd.ExecuteNonQueryAsync();
            return Results.Ok(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating ticket" + ex);
            return Results.Ok(false);
        }
    }
    
}