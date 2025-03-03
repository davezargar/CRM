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
        await using var cmd = _db.CreateCommand("SELECT EXISTS(SELECT * FROM users WHERE email = $1 AND password = $2)");
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
            string defaultPassword = "password123";
            await using var cmd = _db.CreateCommand("INSERT INTO users (email, company_id, role, password) VALUES ($1, $2, $3, $4)");
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue("support");
            cmd.Parameters.AddWithValue(defaultPassword);
            await cmd.ExecuteNonQueryAsync();
            
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

            await using var cmd = _db.CreateCommand("UPDATE users SET active = false WHERE email = $1");
            cmd.Parameters.AddWithValue(email);
            int usersRowsAffected = await cmd.ExecuteNonQueryAsync();

            

            bool success = (usersRowsAffected > 0) ? true : false;
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
            await using var cmd = _db.CreateCommand("WITH ticketIns AS (INSERT INTO tickets(categories_id, subcategory_id, title, user_id, company_id) " +
                                                    "values($1, $2, $3, (SELECT id FROM users WHERE email = $4), $6) returning id) " +
                                                    "INSERT INTO messages(title, message, ticket_id, user_id) " +
                                                    "values ($3, $5, (SELECT ticket_id FROM ticketIns), (SELECT id FROM users WHERE email = $4))");
            cmd.Parameters.AddWithValue(ticketMessages.Category);     //$1
            cmd.Parameters.AddWithValue(ticketMessages.Subcategory);  //$2
            cmd.Parameters.AddWithValue(ticketMessages.Title);        //$3
            cmd.Parameters.AddWithValue(ticketMessages.UserEmail);    //$4
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
                "SELECT tickets.id, title, status, categories.name, subcategories.name, posted, closed, users.email, tickets.company_id, elevated FROM tickets " +
                "INNER JOIN categories ON tickets.category_id = categories.id " +
                "INNER JOIN subcategories ON tickets.subcategory_id = subcategories.id " +
                "INNER JOIN users ON tickets.company_id = users.company_id WHERE email = $1");
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
                    reader.GetString(4),
                    reader.GetDateTime(5),
                    reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    reader.GetString(7),
                    reader.GetInt32(8),
                    reader.GetBoolean(9)
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
                "SELECT tickets.id, title, status, categories.name, subcategories.name, posted, closed, users.email,  tickets.company_id, elevated FROM tickets " +
                "INNER JOIN categories ON categories.id = tickets.category_id " +
                "INNER JOIN subcategories ON subcategories.id = tickets.subcategory_id " +
                "INNER JOIN users ON tickets.company_id = users.company_id WHERE tickets.id = $1 AND email = $2");
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
                    reader.GetString(4),
                    reader.GetDateTime(5),
                    reader.IsDBNull(6) ? null : reader.GetDateTime(6),
                    reader.GetString(7),
                    reader.GetInt32(8),
                    reader.GetBoolean(9)
                );
            return ticket;
        }
        return null;
    }

    public async Task<List<MessagesRecord>> GetTicketMessages(int id)
    {
        List<MessagesRecord> messages = new List<MessagesRecord>();
        await using var cmd = _db.CreateCommand("SELECT messages.id, message, ticket_id, title, users.email, sent FROM messages " +
                                                "INNER JOIN users ON users.id = messages.user_id WHERE ticket_id = $1");
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
            await using var cmd = _db.CreateCommand("INSERT INTO messages (Title, message, user_id, ticket_id) VALUES ($1, $2, (SELECT id FROM users where email = $3), $4)");
            cmd.Parameters.AddWithValue(message.Title.ToString());
            cmd.Parameters.AddWithValue(message.Description.ToString());
            cmd.Parameters.AddWithValue(message.UserEmail);
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
            await using var cmd = _db.CreateCommand("UPDATE tickets set time_closed = CURRENT_TIMESTAMP, status = 'closed' WHERE ticket_id = $1 AND $2 = true");
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
        await using var cmd = _db.CreateCommand("SELECT email FROM users WHERE role = 'support'");
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

            
            await using var cmd = _db.CreateCommand("INSERT INTO users (email, company_id, role, password) VALUES ($1, $2, $3, $4)");
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue("customer");
            cmd.Parameters.AddWithValue(password);
            await cmd.ExecuteNonQueryAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating account: " + ex);
            return false;
        }
    }
}