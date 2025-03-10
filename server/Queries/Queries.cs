using System.ComponentModel.Design;
using System.ComponentModel.Design;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using Npgsql;
using server.Records;

namespace server.Queries;

public class Queries
{
    private NpgsqlDataSource _db;

    public Queries(NpgsqlDataSource db)
    {
        _db = db;
    }

    public async Task<bool> StoreResetToken(string email, string token)
    {
        try
        {
            await using var getUserIdCmd = _db.CreateCommand(
                "SELECT id from users WHERE email = $1"
            );
            getUserIdCmd.Parameters.AddWithValue(email);
            Console.WriteLine(email);
            Console.WriteLine(token);

            object? result = await getUserIdCmd.ExecuteScalarAsync();
            if (result is null)
            {
                return false;
            }

            int userId = (int)result;

            Console.WriteLine(userId);

            if (userId == 0 || string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Invalid userId or token");
                return false;
            }

            await using var cmd = _db.CreateCommand(
                "INSERT INTO reset_tokens (user_id, token) VALUES ($1, $2)"
            );
            cmd.Parameters.AddWithValue(userId);
            cmd.Parameters.AddWithValue(token);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StoreResetToken: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool, string)> VerifyLoginTask(string email, string password)
    {
        await using var cmd = _db.CreateCommand(
            "SELECT password, salt, role FROM users WHERE email = $1"
        );
        cmd.Parameters.AddWithValue(email);
        string db_salt = "";
        string db_hashedPassword = "";
        string db_role = "";
        bool verified = false;

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                db_hashedPassword = reader.GetString(0);
                db_salt = reader.GetString(1);
                db_role = reader.GetString(2);
            }
        }

        if (db_salt != "" && db_hashedPassword != "")
        {
            verified = PasswordHasher.VerifyHashedPassword(password, db_salt, db_hashedPassword);
        }

        return (verified, db_role);
    }

    public bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // En vanlig e-postformatregex
        return Regex.IsMatch(email, emailRegex);
    }

    public async Task<bool> AddCustomerTask(
        string email,
        int companyId,
        string defaultPassword,
        string salt
    )
    {
        try
        {
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return false; // Stop and return false if email is invalid
            }
            await using var cmd = _db.CreateCommand(
                "INSERT INTO users (email, company_id, role, password, salt) VALUES ($1, $2, $3::role, $4, $5)"
            );
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue("support");
            cmd.Parameters.AddWithValue(defaultPassword);
            cmd.Parameters.AddWithValue(salt);
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
            await using var cmd = _db.CreateCommand(
                "UPDATE users SET active = false WHERE email = $1"
            );
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
            await using var cmd = _db.CreateCommand(
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
        await using var cmd = _db.CreateCommand(
            "SELECT tickets.id, title, status, categories.name, subcategories.name, posted, closed, users.email, tickets.company_id, elevated FROM tickets "
                + "INNER JOIN categories ON tickets.category_id = categories.id "
                + "INNER JOIN subcategories ON tickets.subcategory_id = subcategories.id "
                + "INNER JOIN users ON tickets.company_id = users.company_id WHERE email = $1"
        );
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
        await using var cmd = _db.CreateCommand(
            "SELECT tickets.id, title, status, categories.name, subcategories.name, posted, closed, users.email,  tickets.company_id, elevated FROM tickets "
                + "INNER JOIN categories ON categories.id = tickets.category_id "
                + "INNER JOIN subcategories ON subcategories.id = tickets.subcategory_id "
                + "INNER JOIN users ON tickets.company_id = users.company_id WHERE tickets.id = $1 AND email = $2"
        );
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
        await using var cmd = _db.CreateCommand(
            "SELECT messages.id, message, ticket_id, title, users.email, sent, encryption_key, encryption_iv FROM messages "
                + "INNER JOIN users ON users.id = messages.user_id WHERE ticket_id = $1"
        );
        cmd.Parameters.AddWithValue(id);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            int messageId = reader.GetInt32(0);
            string encryptedMessage = reader.GetString(1);
            int ticketId = reader.GetInt32(2);
            string title = reader.GetString(3);
            string email = reader.GetString(4);
            DateTime sentTime = reader.GetDateTime(5);
            string encryptionKey = reader.GetString(6);
            string encryptionIv = reader.GetString(7);

            byte[] key = Convert.FromBase64String(encryptionKey);
            byte[] iv = Convert.FromBase64String(encryptionIv);

            byte[] encryptedBytes = Convert.FromBase64String(encryptedMessage);

            string decryptedMessage = EncryptionSolver.Decrypt(encryptedBytes, key, iv);

            messages.Add(
                new MessagesRecord(messageId, decryptedMessage, ticketId, title, email, sentTime)
            );
        }
        return messages;
    }

    public async Task<bool> PostMessageTask(SendEmail message, byte[] key, byte[] iv)
    {
        try
        {
            await using var cmd = _db.CreateCommand(
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

    public async Task<bool> PostTicketStatusTask(NewTicketStatus ticketStatus)
    {
        try
        {
            await using var cmd = _db.CreateCommand(
                "UPDATE tickets set time_closed = CURRENT_TIMESTAMP, status = 'closed' WHERE ticket_id = $1 AND $2 = true"
            );
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

            await using var cmd = _db.CreateCommand(
                "INSERT INTO users (email, company_id, role, password) VALUES ($1, $2, $3, $4)"
            );
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

    public async Task<List<CategoryRecord>> GetCategories(int companyId)
    {
        List<CategoryPairs> categoryPairs = new List<CategoryPairs>();
        List<CategoryRecord> categories = new List<CategoryRecord>();

        await using var cmd = _db.CreateCommand(
            "SELECT categories.name, subcategories.name FROM categories "
                + "INNER JOIN subcategories ON categories.id = subcategories.main_category_id WHERE company_id = $1"
        );
        cmd.Parameters.AddWithValue(companyId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categoryPairs.Add(new CategoryPairs(reader.GetString(0), reader.GetString(1)));
        }

        List<string> buffer = new List<string>();
        string categoryPrevious = categoryPairs[0].MainCategory;
        foreach (var categoryPair in categoryPairs)
        {
            if (categoryPair.MainCategory == categoryPrevious)
            {
                buffer.Add(categoryPair.Subcategory);
                categoryPrevious = categoryPair.MainCategory;
                continue;
            }

            categories.Add(new CategoryRecord(categoryPrevious, new List<string>(buffer)));
            categoryPrevious = categoryPair.MainCategory;
            buffer.Clear();
            buffer.Add(categoryPair.Subcategory);
        }
        categories.Add(new CategoryRecord(categoryPrevious, buffer));

        return categories;
    }
}
