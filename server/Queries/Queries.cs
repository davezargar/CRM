using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
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
    
    
    public bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // En vanlig e-postformatregex
        return Regex.IsMatch(email, emailRegex);
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

    public async Task<List<TicketCategoryRecord>> GetTicketCategories()
    {
        List<TicketCategoryRecord> ticketCategories = new();
        await using var cmd = _db.CreateCommand("SELECT id, name, company_id FROM categories");
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            ticketCategories.Add(
                new TicketCategoryRecord(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetInt32(2)
                )
            );
        }
        return ticketCategories;
    }

    public async Task<Dictionary<string, List<int>>> GetAssignedCategories()
    {
        Dictionary<string, List<int>> assignments = new();
        await using var cmd = _db.CreateCommand(
            "SELECT u.email, ac.category_id FROM assigned_categories ac JOIN users u ON ac.user_id = u.id"
        );
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
                    insertCmd.CommandText =
                        "INSERT INTO assigned_categories (user_id, category_id) VALUES ($1, $2)";
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
            await using var cmd = _db.CreateCommand(
                "INSERT INTO categories (name, company_id) VALUES ($1, $2)"
            );
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
