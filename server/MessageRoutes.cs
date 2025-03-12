using System.Security.Cryptography;
using Npgsql;


namespace server;

public record SendEmail(string Title, string Description, string UserEmail, int Ticket_id_fk);

public static class MessageRoutes
{
    public static async Task<IResult> PostMessages(HttpContext context, NpgsqlDataSource db)
    {
        var requestBody = await context.Request.ReadFromJsonAsync<SendEmail>();
        if (requestBody == null)
        {
            return Results.BadRequest("The request body is empty");
        }
        string userId = context.Session.GetString("Email");
        Console.WriteLine("SESSION EMAIL: " + userId);
        Console.WriteLine("TICKET ID: " + requestBody.Ticket_id_fk);

        byte[] key = new byte[16];
        byte[] iv = new byte[16];

        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
            rng.GetBytes(iv);
        }

        byte[] encryptedDescriptionBytes = EncryptionSolver.Encrypt(
            requestBody.Description,
            key,
            iv
        );
        string encryptedDescription = Convert.ToBase64String(encryptedDescriptionBytes);

        var updatedRequest = requestBody with
        {
            UserEmail = userId,
            Description = encryptedDescription,
        };

        bool success = await PostMessageTask(db, updatedRequest, key, iv);

        if (!success)
        {
            Results.Problem("Couldn't process the Sql Query");
        }

        return Results.Ok(new { message = "Successfully posted the message to database" });
    }

    private static async Task<bool> PostMessageTask(
        NpgsqlDataSource db,
        SendEmail message,
        byte[] key,
        byte[] iv
    )
    {
        try
        {
            await using var cmd = db.CreateCommand(
                "INSERT INTO messages (Title, message, user_id, ticket_id, encryption_key, encryption_iv) VALUES ($1, $2, (SELECT id FROM users where email = $3), $4, $5, $6)"
            );
            cmd.Parameters.AddWithValue(message.Title.ToString());
            cmd.Parameters.AddWithValue(message.Description.ToString());
            cmd.Parameters.AddWithValue(message.UserEmail);
            cmd.Parameters.AddWithValue(message.Ticket_id_fk);
            cmd.Parameters.AddWithValue(Convert.ToBase64String(key));
            cmd.Parameters.AddWithValue(Convert.ToBase64String(iv));
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Couldn't post message" + ex);
            return false;
        }
    }
}
