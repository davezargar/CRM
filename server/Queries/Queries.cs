using System.Linq.Expressions;
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

public async Task<bool> CreateTicketTask(TicketRequest ticket)
{

    try
        {
        await using var cmd = _db.CreateCommand("INSERT INTO tickets (Category, Subcategory, Title, User_fk, Response_email, Company_fk) VALUES ($1, $2, $3, $4, $5, $6)");
        cmd.Parameters.AddWithValue(ticket.Category.ToString());
        cmd.Parameters.AddWithValue(ticket.Subcategory.ToString());
        cmd.Parameters.AddWithValue(ticket.Title.ToString());
        cmd.Parameters.AddWithValue(ticket.User_fk.ToString());
        cmd.Parameters.AddWithValue(ticket.Response_email.ToString());
        cmd.Parameters.AddWithValue(ticket.Company_fk);
        await cmd.ExecuteNonQueryAsync();
        return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error creating ticket" + ex);
            return false;
        }
    }
}