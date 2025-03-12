using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace server;

public record LoginRecord(string Email, string Password);
public record LoginDetails(string Email, string Password);

public static class LoginRoutes
{
    public static async Task<IResult> PostLogin(
        LoginRecord credentials,
        PasswordHasher<string> hasher,
        HttpContext context,
        NpgsqlDataSource db
    )
    {
        (bool verified, string role, int company_id) = await VerifyLoginTask(
            credentials.Email,
            credentials.Password,
            hasher,
            db
        );
        Console.WriteLine(verified);
        if (verified)
        {
            context.Session.SetString("Authenticated", "True"); // add data to a session
            context.Session.SetString("Email", credentials.Email);
            context.Session.SetString("Role", role);
            context.Session.SetInt32("company", company_id);
            return Results.Ok(role);
        }
        else
        {
            return TypedResults.Forbid();
        }
    }

    private static async Task<(bool, string, int)> VerifyLoginTask(
        string email,
        string password,
        PasswordHasher<string> hasher,
        NpgsqlDataSource db
    )
    {
        await using var cmd = db.CreateCommand(
            "SELECT password, role, company_id FROM users WHERE email = $1"
        );
        cmd.Parameters.AddWithValue(email);
        string? db_hashedPassword = null;
        string? db_role = null;
        int company_id = 0;
        bool verified = false;

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                db_role = reader.GetString(1);
                company_id = reader.GetInt32(2);
                if (!reader.IsDBNull(0))
                {
                    db_hashedPassword = reader.GetString(0);
                    Console.WriteLine("password is read");
                }
                else { }
            }
        }

        if (db_hashedPassword != null)
        {
            Console.WriteLine("try to verify password");
            var result = hasher.VerifyHashedPassword("", db_hashedPassword, password);
            if (result == PasswordVerificationResult.Failed)
            {
                verified = false;
            }
            else
            {
                verified = true;
            }
        }
        else
        {
            string hashedPassword = hasher.HashPassword("", password);
            var update_password_cmd = db.CreateCommand(
                "UPDATE users set password = $1 where email = $2"
            );
            update_password_cmd.Parameters.AddWithValue(hashedPassword);
            update_password_cmd.Parameters.AddWithValue(email);
            await update_password_cmd.ExecuteNonQueryAsync();
        }

        return (verified, db_role, company_id);
    }
}
