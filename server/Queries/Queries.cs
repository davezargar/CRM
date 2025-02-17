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

    public async Task<List<TicketRecord>> GetTicketHistory(string email)
    {
        List<TicketRecord> Tickets = new List<TicketRecord>();
        await using var cmd = _db.CreateCommand("SELECT * from tickets where user_fk = $1");
        cmd.Parameters.AddWithValue(email);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Tickets.Add(
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

        return Tickets;
    }
}

