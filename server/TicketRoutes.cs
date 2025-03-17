using System.Security.Cryptography;
using Npgsql;
using Microsoft.AspNetCore.Identity;
using server.Classes;
using server.Services;

namespace server;

public static class TicketRoutes
{
    public record NewTicketRecord(
        int TicketId,
        string CategoryName,
        string SubcategoryName,
        string Title,
        string UserEmail,
        int CompanyFk,
        int MessageId,
        string Message
    );

    public record TicketRecord(
        int TicketId,
        string Title,
        string Status,
        string Category,
        string Subcategory,
        DateTime TimePosted,
        DateTime? TimeClosed,
        string UserEmail,
        int CompanyFk,
        bool Elevated
    );

    public record MessagesRecord(
        int MessageId,
        string Message,
        int TicketId,
        string Title,
        string UserId,
        DateTime TimePosted
    );

    public record TicketMessagesRecord(TicketRecord TicketRecord, List<MessagesRecord> Messages);

    public record NewTicketStatus(int Ticket_id, bool Resolved);

    public record FeedbackRecord(int Id, int Rating, string Comment, DateTime Written, int From, int Target, int TicketId);

    public record FeedbackFormRecord(int Rating, string Comment);

    public record FeedbackRequest(FeedbackFormRecord FeedbackData, string Token);

    public static async Task<bool> PostReview(NpgsqlDataSource db, FeedbackRequest request)
    {
        Console.WriteLine(request.Token);
        Console.WriteLine(request.FeedbackData.Rating);
        Console.WriteLine(request.FeedbackData.Comment);
        int ticketId = 0;
        int userId = 0;
        await using var cmd1 = db.CreateCommand(
            "SELECT tickets.id, tickets.user_id FROM tickets INNER JOIN feedback_token ON tickets.id = feedback_token.id WHERE token = $1"
        );
        cmd1.Parameters.AddWithValue(request.Token);
        using var reader = await cmd1.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            ticketId = reader.GetInt32(0);
            userId = reader.GetInt32(1);
        }
        if (ticketId == 0 || userId == 0)
            return false;

        await using var cmd2 = db.CreateCommand(
            "INSERT INTO feedback (rating, comment, from_user, ticket_id) VALUES ($1, $2, $3, $4)"
        );
        cmd2.Parameters.AddWithValue(request.FeedbackData.Rating);
        cmd2.Parameters.AddWithValue(request.FeedbackData.Comment);
        cmd2.Parameters.AddWithValue(userId);
        cmd2.Parameters.AddWithValue(ticketId);
        await cmd2.ExecuteNonQueryAsync();
        return true;
    }
    public static async Task<IResult> PostTickets(PasswordHasher<string> hasher, IEmailService emailService, HttpContext context,
        NpgsqlDataSource db)
    {
        NewTicketRecord? ticketMessages =
            await context.Request.ReadFromJsonAsync<NewTicketRecord>();

        if (ticketMessages == null)
            return Results.BadRequest();

        await using var cmd1 = db.CreateCommand(
            "SELECT EXISTS(SELECT 1 FROM users WHERE email = $1)"
        );
        cmd1.Parameters.AddWithValue(ticketMessages.UserEmail);
        bool exists = (bool?)await cmd1.ExecuteScalarAsync() ?? false;

        if (!exists)
        {
            await using var cmd2 = db.CreateCommand(
                "INSERT INTO users (email, company_id) VALUES ($1, $2)"
            );
            cmd2.Parameters.AddWithValue(ticketMessages.UserEmail);
            cmd2.Parameters.AddWithValue(ticketMessages.CompanyFk);
            int rows = await cmd2.ExecuteNonQueryAsync();
            exists = (rows > 0);
        }
        if (!exists)
            Console.WriteLine("couldnt create a user ");

        int id;
        try
        {
            await using var cmd = db.CreateCommand(
                "WITH ticketIns AS (INSERT INTO tickets(category_id, subcategory_id, title, user_id, company_id) "
                    + "values((SELECT id FROM categories WHERE name = $1 AND company_id = $6), (SELECT id FROM subcategories WHERE name = $2 AND main_category_id = (SELECT id FROM categories WHERE name = $1 AND company_id = $6)), $3, (SELECT id FROM users WHERE email = $4 AND company_id = $6), $6) returning id) "
                    + "INSERT INTO messages(title, message, ticket_id, user_id) "
                    + "values ($3, $5, (SELECT id FROM ticketIns), (SELECT id FROM users WHERE email = $4)) returning ticket_id"
            );
            cmd.Parameters.AddWithValue(ticketMessages.CategoryName); //$1
            cmd.Parameters.AddWithValue(ticketMessages.SubcategoryName); //$2
            cmd.Parameters.AddWithValue(ticketMessages.Title); //$3
            cmd.Parameters.AddWithValue(ticketMessages.UserEmail); //$4
            cmd.Parameters.AddWithValue(ticketMessages.Message); //$5
            cmd.Parameters.AddWithValue(ticketMessages.CompanyFk); //$6
            id = (int)await cmd.ExecuteScalarAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating ticket" + ex);
            return Results.Ok(false);
        }

        static string GenerateUniqueTicketLink()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[20];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('/', 'o');
        }

        bool collision = true;
        string token = "";
        while (collision)
        {
            token = GenerateUniqueTicketLink();
            await using var cmd = db.CreateCommand(
                "INSERT INTO ticket_access_links (ticket_id, access_link) " + "VALUES ($1, $2)"
            );
            cmd.Parameters.AddWithValue(id);
            cmd.Parameters.AddWithValue(token);

            if (await cmd.ExecuteNonQueryAsync() > 0)
                collision = false;
        }
        string link = DotEnv.GetString("Localhost") + "tickets/" + id + "/" + token;
        string emailSubject = "Here's a link to your new ticket";
        string emailBody =
            "Hello \n"
            + " \n"
            + "We've recieved you ticket and will respond shortly,"
            + " to acceess your ticket you can click the link below\n"
            + link;

        await emailService.SendEmailAsync(ticketMessages.UserEmail, emailSubject, emailBody);

        Console.WriteLine(link);
        return Results.Ok(true);
    }

    public static async Task<IResult> GetTickets(HttpContext context, NpgsqlDataSource db)
    {
        string? requesterEmail = context.Session.GetString("Email");

        if (String.IsNullOrEmpty(requesterEmail) || context.Session.GetString("Role") == "customer")
            return Results.Unauthorized();

        List<TicketRecord> tickets = new List<TicketRecord>();
        await using var cmd = db.CreateCommand(
            "SELECT id, title, status, main_category, sub_category, posted, closed, email, company_id, elevated FROM tickets_view WHERE company_id = (SELECT company_id FROM users WHERE email = $1 AND status = 'pending' LIMIT 1)"
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

    public static async Task<IResult> GetTicketWithToken(
        HttpContext context,
        string token,
        NpgsqlDataSource db
    )
    {
        TicketRecord? ticket = null;
        Console.WriteLine("token" + token);
        await using var cmd = db.CreateCommand(
            "SELECT tickets_view.id, title, status, main_category, sub_category, posted, closed, email, company_id, elevated FROM tickets_view "
                + "INNER JOIN ticket_access_links ON ticket_access_links.ticket_id = tickets_view.id "
                + "WHERE ticket_access_links.access_link = $1"
        );
        cmd.Parameters.AddWithValue(token);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            ticket = new(
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
            );
        }

        if (ticket == null)
            return Results.Problem();

        context.Session.SetString("Email", ticket.UserEmail);

        List<MessagesRecord> messages = new List<MessagesRecord>();
        await using var cmd2 = db.CreateCommand(
            "SELECT messages.id, message, ticket_id, title, users.email, sent, encryption_key, encryption_iv FROM messages "
                + "INNER JOIN users ON users.id = messages.user_id WHERE ticket_id = $1"
        );
        cmd2.Parameters.AddWithValue(ticket.TicketId);
        using var reader2 = await cmd2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            int messageId = reader2.GetInt32(0);
            string Message = reader2.GetString(1);
            int ticketId2 = reader2.GetInt32(2);
            string title = reader2.GetString(3);
            string email = reader2.GetString(4);
            DateTime sentTime = reader2.GetDateTime(5);
            if (!reader2.IsDBNull(6) && !reader2.IsDBNull(7))
            {
                string encryptionKey = reader2.GetString(6);
                string encryptionIv = reader2.GetString(7);
                byte[] key = Convert.FromBase64String(encryptionKey);
                byte[] iv = Convert.FromBase64String(encryptionIv);

                byte[] encryptedBytes = Convert.FromBase64String(Message);

                Message = EncryptionSolver.Decrypt(encryptedBytes, key, iv);
            }

            messages.Add(new MessagesRecord(messageId, Message, ticketId2, title, email, sentTime));
        }

        TicketMessagesRecord ticketMessages = new(ticket, messages);
        return Results.Ok(ticketMessages);
    }

    public static async Task<IResult> GetTicket(
        HttpContext context,
        int ticketId,
        NpgsqlDataSource db
    )
    {
        string? email1 = context.Session.GetString("Email");

        if (String.IsNullOrEmpty(email1))
            return Results.Unauthorized();

        TicketRecord ticket = null;
        await using var cmd1 = db.CreateCommand(
            "SELECT id, title, status, main_category, sub_category, posted, closed, email, company_id, elevated FROM tickets_view WHERE id = $1"
        );
        cmd1.Parameters.AddWithValue(ticketId);
        cmd1.Parameters.AddWithValue(email1);
        using var reader1 = await cmd1.ExecuteReaderAsync();
        while (await reader1.ReadAsync())
        {
            ticket = new(
                reader1.GetInt32(0),
                reader1.GetString(1),
                reader1.GetString(2),
                reader1.GetString(3),
                reader1.GetString(4),
                reader1.GetDateTime(5),
                reader1.IsDBNull(6) ? null : reader1.GetDateTime(6),
                reader1.GetString(7),
                reader1.GetInt32(8),
                reader1.GetBoolean(9)
            );
        }

        List<MessagesRecord> messages = new List<MessagesRecord>();
        await using var cmd2 = db.CreateCommand(
            "SELECT messages.id, message, ticket_id, title, users.email, sent, encryption_key, encryption_iv FROM messages "
                + "INNER JOIN users ON users.id = messages.user_id WHERE ticket_id = $1"
        );
        cmd2.Parameters.AddWithValue(ticketId);
        using var reader2 = await cmd2.ExecuteReaderAsync();
        while (await reader2.ReadAsync())
        {
            int messageId = reader2.GetInt32(0);
            string Message = reader2.GetString(1);
            int ticketId2 = reader2.GetInt32(2);
            string title = reader2.GetString(3);
            string email = reader2.GetString(4);
            DateTime sentTime = reader2.GetDateTime(5);
            if (!reader2.IsDBNull(6) && !reader2.IsDBNull(7))
            {
                string encryptionKey = reader2.GetString(6);
                string encryptionIv = reader2.GetString(7);
                byte[] key = Convert.FromBase64String(encryptionKey);
                byte[] iv = Convert.FromBase64String(encryptionIv);

                byte[] encryptedBytes = Convert.FromBase64String(Message);

                Message = EncryptionSolver.Decrypt(encryptedBytes, key, iv);
            }

            messages.Add(new MessagesRecord(messageId, Message, ticketId2, title, email, sentTime));
        }

        TicketMessagesRecord ticketMessages = new(ticket, messages);
        return Results.Ok(ticketMessages);
    }

    public static async Task<IResult> UpdateTicket(HttpContext context, NpgsqlDataSource db, IEmailService email)
    {
        NewTicketStatus requestBody = await context.Request.ReadFromJsonAsync<NewTicketStatus>();
        if (requestBody == null)
        {
            return Results.BadRequest("The request body is empty");
        }

        bool success = false;
        try
        {
            await using var cmd = db.CreateCommand(
                "UPDATE tickets set closed = CURRENT_TIMESTAMP, status = 'closed' WHERE id = $1 AND $2 = true returning user_id"
            );
            cmd.Parameters.AddWithValue(requestBody.Ticket_id);
            cmd.Parameters.AddWithValue(requestBody.Resolved);
            int id = (int?)await cmd.ExecuteScalarAsync() ?? -1;

            if (id == -1)
            return Results.Problem("Couldn't find user ID from update");

            await using var cmd1 = db.CreateCommand(
                "SELECT email FROM users WHERE id = $1"
            );

            cmd1.Parameters.AddWithValue(id);

            string fromEmail = (string?)await cmd1.ExecuteScalarAsync() ?? "";
            success = true;
            string token = Guid.NewGuid().ToString();

            bool successToken = await StoreFeedbackToken(requestBody.Ticket_id, token, db);

            if (!successToken)
            {
                return Results.Problem("Failed to store feeback token");
            }

            string feedbackLink = $"http://localhost:5173/feedback-form/?token={token}";

            var emailRequest = new EmailRequest(
                fromEmail,
                "Give Feedback",
                $"Hello, {fromEmail}, \n\nYour ticket has been resolved. \nPlease click the link to give us feedback: \n<a href='{feedbackLink}'>Feedback Form</a>"
            );

            await email.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);

        }
        catch (Exception ex)
        {
            Console.WriteLine("Couldn't post message" + ex);
            success = false;
        }

        if (!success)
        {
            Results.Problem("Couldn't process the Sql Query");
        }

        return Results.Ok(new { message = "Successfully posted the ticket status to database" });
    }

    public static async Task<IResult> GetResolvedTickets(HttpContext context, NpgsqlDataSource db)
    {
        string? requesterEmail = context.Session.GetString("Email");

        if (String.IsNullOrEmpty(requesterEmail) || context.Session.GetString("Role") == "customer")
            return Results.Unauthorized();

        List<TicketRecord> tickets = new List<TicketRecord>();
        await using var cmd = db.CreateCommand(
            "SELECT id, title, status, main_category, sub_category, posted, closed, email, company_id, elevated FROM tickets_view WHERE company_id = (SELECT company_id FROM users WHERE email = $1 LIMIT 1)"
                + "AND closed IS NOT NULL"
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
                    reader.GetDateTime(6),
                    reader.GetString(7),
                    reader.GetInt32(8),
                    reader.GetBoolean(9)
                )
            );
        }

        return Results.Ok(tickets);
    }
    
    private static async Task<bool> StoreFeedbackToken( int ticket_id, string token, NpgsqlDataSource db)
    {
        try
        {
            int user_id;

            await using var getUserIdCmd = db.CreateCommand(
                "SELECT user_id from tickets WHERE id = $1"
            );
            getUserIdCmd.Parameters.AddWithValue(ticket_id);
            Console.WriteLine(ticket_id);

            await using var reader = await getUserIdCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()){
                Console.WriteLine("No user found for ticket id" + ticket_id);
                return false;
            }

            user_id = reader.GetInt32(0);
            Console.WriteLine("User found" + user_id);

            await using var insertTokenCmd = db.CreateCommand(
                "INSERT INTO feedback_token (id, token) VALUES ($1, $2)"
            );
            insertTokenCmd.Parameters.AddWithValue(ticket_id);
            insertTokenCmd.Parameters.AddWithValue(token);

            int rowsAffected = await insertTokenCmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StoreFeedbackToken: {ex.Message}");
            return false;
        }

    }

}
