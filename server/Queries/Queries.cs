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
}