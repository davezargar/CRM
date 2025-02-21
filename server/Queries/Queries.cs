using System.Linq.Expressions;
using Npgsql;
using server.Records;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using System.ComponentModel.Design;


namespace server.Queries;

public class Queries
{
    private NpgsqlDataSource _db;
    public Queries(NpgsqlDataSource db)
    {
        _db = db;
    }

    public async Task<(bool, string)> VerifyLoginTask(string email, string password)
    {
        await using var cmd = _db.CreateCommand("SELECT EXISTS(SELECT * FROM login_credentials WHERE email = $1 AND password = $2)");
        cmd.Parameters.AddWithValue(email);
        cmd.Parameters.AddWithValue(password);
        var result = await cmd.ExecuteScalarAsync();
        bool verified = (bool?)result ?? false;
        if (!verified)
        {
            return (verified, "");
        }
        await using var cmd2 = _db.CreateCommand("SELECT role FROM users WHERE email = $1");
        cmd2.Parameters.AddWithValue(email);
        var result2 = await cmd2.ExecuteScalarAsync();
        Console.WriteLine();
        string role = (string?)result2 ?? "";

        return (verified, role);
    }

    public bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";  // En vanlig e-postformatregex
        return Regex.IsMatch(email, emailRegex);
    }
    public async Task<bool> AddCustomerTask(string email, int companyId)
    {

        try
        {
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return false;  // Stop and return false if email is invalid
            }

            await using var cmd = _db.CreateCommand("INSERT INTO users (email, company_fk, verified, role) VALUES ($1, $2, $3, $4)");
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue(false);
            cmd.Parameters.AddWithValue("customerSupport");
            await cmd.ExecuteNonQueryAsync();

            string defaultPassword = "password123";
            await using var loginCmd = _db.CreateCommand("INSERT INTO login_credentials (email, password) VALUES ($1, $2)");
            loginCmd.Parameters.AddWithValue(email);
            loginCmd.Parameters.AddWithValue(defaultPassword);
            await loginCmd.ExecuteNonQueryAsync();

            return true;


        }
        catch (Exception ex)
        {
            Console.WriteLine("error adding customer support worker:" + ex);
            return false;
        }
    }

    public async Task<bool> RemoveCustomerTask(string email)
    {
        try
        {
            await using var loginCmd = _db.CreateCommand("DELETE FROM login_credentials WHERE email = $1");
            loginCmd.Parameters.AddWithValue(email);
            int loginRowsAffected = await loginCmd.ExecuteNonQueryAsync();   

            await using var cmd = _db.CreateCommand("DELETE FROM users WHERE email = $1");
            cmd.Parameters.AddWithValue(email);
            int usersRowsAffected = await cmd.ExecuteNonQueryAsync();

            

            bool success = (usersRowsAffected > 0 || loginRowsAffected > 0) ? true : false;
            return success;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("error removing customer support worker:" + ex);
            return false;
        }
    }

    public async Task<bool> CreateTicketTask(NewTicketRecord ticketMessages)
    {

        try
        {
            await using var cmd = _db.CreateCommand("WITH ticketIns AS (INSERT INTO tickets(category, subcategory, title, user_fk, company_fk) " +
                                                    "values($1, $2, $3, $4, $6) returning ticket_id) " +
                                                    "INSERT INTO messages(message, ticket_id_fk, title, user_fk) " +
                                                    "values ($5, (SELECT ticket_id FROM ticketIns),$3, $4)");

            cmd.Parameters.AddWithValue(ticketMessages.Category);     //$1
            cmd.Parameters.AddWithValue(ticketMessages.Subcategory);  //$2
            cmd.Parameters.AddWithValue(ticketMessages.Title);        //$3
            cmd.Parameters.AddWithValue(ticketMessages.UserFk);       //$4
            cmd.Parameters.AddWithValue(ticketMessages.Message);      //$5
            cmd.Parameters.AddWithValue(ticketMessages.CompanyFk);    //$6
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating ticket" + ex);
            return false;
        }
    }

    public async Task<List<TicketRecord>> GetTicketsAll(string email) //email för den som gjort request används för att få vilket företag
    {
        List<TicketRecord> tickets = new List<TicketRecord>();
        await using var cmd =
            _db.CreateCommand(
                "SELECT ticket_id, category, subcategory, title, time_posted, time_closed, user_fk, tickets.company_fk FROM tickets " +
                "INNER JOIN users ON tickets.company_fk = users.company_fk WHERE email = $1");
        cmd.Parameters.AddWithValue(email);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tickets.Add(
                new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetDateTime(4),
                    reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    reader.GetString(6),
                    reader.GetInt32(7)
                )
            );

        }

        return tickets;
    }

    public async Task<TicketRecord> GetTicket(string email, int id) //email för den som gjort request används för att få vilket företag
    {
        TicketRecord ticket;
        await using var cmd =
            _db.CreateCommand(
                "SELECT ticket_id, category, subcategory, title, time_posted, time_closed, user_fk, tickets.company_fk FROM tickets " +
                "INNER JOIN users ON tickets.company_fk = users.company_fk WHERE ticket_id = $1 AND email = $2");
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(email);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            ticket = new(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetDateTime(4),
                    reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    reader.GetString(6),
                    reader.GetInt32(7)
                );
            return ticket;
        }
        return null;
    }

    public async Task<List<MessagesRecord>> GetTicketMessages(int id)
    {
        List<MessagesRecord> messages = new List<MessagesRecord>();
        await using var cmd = _db.CreateCommand("SELECT message_id, message, ticket_id_fk, title, user_fk, time_sent FROM messages WHERE ticket_id_fk = $1");
        cmd.Parameters.AddWithValue(id);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            messages.Add(new(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetDateTime(5)
            ));
        }
        return messages;
    }


    public async Task<bool> PostMessageTask(SendEmail message)
    {
        try
        {
            await using var cmd = _db.CreateCommand("INSERT INTO messages (Title, message, User_fk, ticket_id_fk) VALUES ($1, $2, $3, $4)");
            cmd.Parameters.AddWithValue(message.Title.ToString());
            cmd.Parameters.AddWithValue(message.Description.ToString());
            cmd.Parameters.AddWithValue(message.User_fk.ToString());
            cmd.Parameters.AddWithValue(message.Ticket_id_fk);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Couldn't post message" + ex);
            return false;
        }
    }

    public async Task<bool> PostTicketStatusTask(NewTicketStatus ticketStatus)
    {
        try
        {
            await using var cmd = _db.CreateCommand("UPDATE tickets set time_closed = CURRENT_TIMESTAMP WHERE ticket_id = $1 AND $2 = true");
            cmd.Parameters.AddWithValue(ticketStatus.Ticket_id);
            cmd.Parameters.AddWithValue(ticketStatus.Resolved);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Couldn't post message" + ex);
            return false;
        }
    }

    public async Task<List<GetCustomerSupportEmail>> GetCustomerSupportWorkers()
    {
        List<GetCustomerSupportEmail> customerSupportEmail = new();
        await using var cmd = _db.CreateCommand("SELECT email FROM users WHERE role = 'customerService' OR role = 'customerSupport'");
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            customerSupportEmail.Add(new GetCustomerSupportEmail(reader.GetString(0)));
        }
        return customerSupportEmail;
    }
    
    
    public async Task<bool> CustomersTask(string email, string password, int companyId)
    {
        try
        {
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return false;
            }

            
            await using var cmd = _db.CreateCommand("INSERT INTO users (email, company_fk, verified, role) VALUES ($1, $2, $3, $4)");
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue(false);
            cmd.Parameters.AddWithValue("customer");
            await cmd.ExecuteNonQueryAsync();

            // Maybe not smart to save password as plain text (*v*)
            await using var loginCmd = _db.CreateCommand("INSERT INTO login_credentials (email, password) VALUES ($1, $2)");
            loginCmd.Parameters.AddWithValue(email);
            loginCmd.Parameters.AddWithValue(password);
            await loginCmd.ExecuteNonQueryAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating account: " + ex);
            return false;
        }
    }
}