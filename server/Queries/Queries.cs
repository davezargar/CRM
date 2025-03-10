using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using Npgsql;
using server.Records;
using System.ComponentModel.Design;
using System.Data;
using Microsoft.AspNetCore.Mvc.Diagnostics;


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

    public async Task<int> CreateTicketTask(NewTicketRecord ticketMessages)
    {
        try
        {
            Console.WriteLine("running insert now!");
            await using var cmd1 = _db.CreateCommand(
                "INSERT INTO tickets(category_id, subcategory_id, title, user_id, company_id) " +
                "values((SELECT id FROM categories WHERE name = $1 AND company_id = $5), (SELECT id FROM subcategories WHERE name = $2 AND " +
                "main_category_id = (SELECT id FROM categories WHERE name = $1 AND company_id = $5)), $3, (SELECT id FROM users WHERE email = $4 AND " +
                "company_id = $5), $5) RETURNING id");
            cmd1.Parameters.AddWithValue(ticketMessages.CategoryName); //$1
            cmd1.Parameters.AddWithValue(ticketMessages.SubcategoryName); //$2
            cmd1.Parameters.AddWithValue(ticketMessages.Title); //$3
            cmd1.Parameters.AddWithValue(ticketMessages.UserEmail); //$4
            cmd1.Parameters.AddWithValue(ticketMessages.CompanyFk); //$5
            int ticketid =  (int?) await cmd1.ExecuteScalarAsync() ?? -1;
            
            Console.WriteLine("generated ticket, id: " + ticketid.ToString());

            if (ticketid == -1)
                return ticketid;

            await using var cmd2 = _db.CreateCommand(
                "INSERT INTO messages(title, message, ticket_id, user_id) " +
                "values ($1, $4, $5, (SELECT id FROM users WHERE email = $2 AND company_id = $3))");
            cmd2.Parameters.AddWithValue(ticketMessages.Title);     //$1
            cmd2.Parameters.AddWithValue(ticketMessages.UserEmail); //$2
            cmd2.Parameters.AddWithValue(ticketMessages.CompanyFk); //$3
            cmd2.Parameters.AddWithValue(ticketMessages.Message);   //$4
            cmd2.Parameters.AddWithValue(ticketid);                 //$5
            int rows = await cmd2.ExecuteNonQueryAsync();
            if (rows <= 0)
                return -1;
            return ticketid;
                
            
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating ticket" + ex);
            return -1;
        }
    }

    public async Task<bool> InsertTicketLink(int id, string token)
    {
        await using var cmd = _db.CreateCommand("INSERT INTO ticket_access_links (ticket_id, access_link) " +
            "VALUES ($1, $2)");
        cmd.Parameters.AddWithValue(id);
        cmd.Parameters.AddWithValue(token);

        return (await cmd.ExecuteNonQueryAsync() > 0);
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
        
        await using var cmd = _db.CreateCommand("SELECT categories.name, subcategories.name FROM categories " +
                                                "INNER JOIN subcategories ON categories.id = subcategories.main_category_id WHERE company_id = $1");
        cmd.Parameters.AddWithValue(companyId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categoryPairs.Add(new CategoryPairs (
                reader.GetString(0),
                reader.GetString(1)
             ));
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

    public async Task<List<TicketCategoryRecord>> GetTicketCategories()
    {
        List<TicketCategoryRecord> ticketCategories = new();
        await using var cmd = _db.CreateCommand("SELECT id, name, company_id FROM categories");
        using var reader = await cmd.ExecuteReaderAsync();
    
        while (await reader.ReadAsync())
        {
            ticketCategories.Add(new TicketCategoryRecord(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2)));
        }
        return ticketCategories;
    }

    public async Task<Dictionary<string, List<int>>> GetAssignedCategories()
    {
        Dictionary<string, List<int>> assignments = new();
        await using var cmd =
            _db.CreateCommand(
                "SELECT u.email, ac.category_id FROM assigned_categories ac JOIN users u ON ac.user_id = u.id");
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string email = reader.GetString(0);
            int categoryId = reader.GetInt32(1);

            if (!assignments.ContainsKey(email))
                assignments.Add(email, new List<int>());

            assignments[email].Add(categoryId);
        }

        return assignments;
    }

    public async Task<bool> AssignCategoriesToWorkers(Dictionary<string, List<int>> assignments)
    {
        try
        {
            await using var connection = await _db.OpenConnectionAsync(); 
            await using var transaction = await connection.BeginTransactionAsync(); 

            foreach (var entry in assignments)
            {
                string workerEmail = entry.Key;
                List<int> categoryIds = entry.Value;

                
                await using var getUserCmd = connection.CreateCommand();
                getUserCmd.CommandText = "SELECT id FROM users WHERE email = $1";
                getUserCmd.Parameters.AddWithValue(workerEmail);
                await using var reader = await getUserCmd.ExecuteReaderAsync();

                if (!reader.Read())
                    continue;

                int userId = reader.GetInt32(0);
                await reader.CloseAsync();

                
                await using var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE FROM assigned_categories WHERE user_id = $1";
                deleteCmd.Parameters.AddWithValue(userId);
                await deleteCmd.ExecuteNonQueryAsync();

                
                foreach (var categoryId in categoryIds)
                {
                    await using var insertCmd = connection.CreateCommand();
                    insertCmd.CommandText = "INSERT INTO assigned_categories (user_id, category_id) VALUES ($1, $2)";
                    insertCmd.Parameters.AddWithValue(userId);
                    insertCmd.Parameters.AddWithValue(categoryId);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync(); 
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error assigning tickets: " + ex);
            return false;
        }
    }
    
    public async Task<bool> CreateCategory(string name, int companyId)
    {
        try
        {
            await using var cmd = _db.CreateCommand("INSERT INTO categories (name, company_id) VALUES ($1, $2)");
            cmd.Parameters.AddWithValue(name);
            cmd.Parameters.AddWithValue(companyId);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating category: " + ex);
            return false;
        }
    }

}
