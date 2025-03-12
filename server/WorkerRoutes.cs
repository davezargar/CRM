using Npgsql;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace server;

public static class WorkerRoutes
{
    public record AdminRequest(string Email, int? CompanyId);
    
    public static bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // En vanlig e-postformatregex
        return Regex.IsMatch(email, emailRegex);
    }
    public static async Task<IResult> CreateWorker(PasswordHasher<string> hasher, HttpContext context, NpgsqlDataSource db)
    {
        var requestBody = await context.Request.ReadFromJsonAsync<AdminRequest>();
        if (requestBody == null)
        {
            return Results.BadRequest("Invalid email");
        }
        Console.WriteLine($"received email: {requestBody.Email}");
        string email = requestBody.Email;
        int companyId = requestBody.CompanyId ?? 1;
        string defaultPassWord = "hej123";
        string hashedPassword = hasher.HashPassword("", defaultPassWord);
        Console.WriteLine($"hashed password: {hashedPassword}");

        try
        {
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return Results.Problem("Failed to add worker, invalid email");
            }
            await using var cmd = db.CreateCommand(
                "INSERT INTO users (email, company_id, password, role) VALUES ($1, $2, $3, 'support')"
            );
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue(hashedPassword);
            await cmd.ExecuteNonQueryAsync();

            return Results.Ok(new { message = "Valid mail" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("error adding customer support worker:" + ex);
            return Results.Problem("Failed to add worker");
        }
    }
    
    
}