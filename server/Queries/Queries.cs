using Npgsql;
using server.Records;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;


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
            cmd.Parameters.AddWithValue(true);
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
            await using var cmd = _db.CreateCommand("DELETE FROM users WHERE email = $1");
            cmd.Parameters.AddWithValue(email);
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
        

            bool success = (rowsAffected > 0) ? true : false;
            return success;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine("error removing customer support worker:" + ex);
            return false;
        }
    }
}