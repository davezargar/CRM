using Npgsql;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace server;

public static class WorkerRoutes
{
    public record AdminRequest(string Email, int? CompanyId);
    
    public record GetCustomerSupportEmail(string Email);
    
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

    public static async Task<IResult> InactivateWorker(HttpContext context, NpgsqlDataSource db)
    {
        var requestBody = await context.Request.ReadFromJsonAsync<AdminRequest>();
        if (requestBody == null)
        {
            return Results.BadRequest("Invalid email");
        }
        Console.WriteLine(requestBody.Email);
        try
        {
            await using var cmd = db.CreateCommand(
                "UPDATE users SET active = false WHERE email = $1"
            );
            cmd.Parameters.AddWithValue(requestBody.Email);
            int usersRowsAffected = await cmd.ExecuteNonQueryAsync();

            bool success = (usersRowsAffected > 0) ? true : false;
            return Results.Ok(new { message = "Successfully removed wroker" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("error removing customer support worker:" + ex);
            return Results.Problem("failed to remove worker");
        }
    }

    public static async Task<IResult> GetActiveWorkers(NpgsqlDataSource db)
    {
        try
        {
            List<GetCustomerSupportEmail> customerSupportEmails = new();
            await using var cmd = db.CreateCommand("SELECT email FROM users WHERE role = 'support' AND active = true");
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                customerSupportEmails.Add(new GetCustomerSupportEmail(reader.GetString(0)));
            }

            if (customerSupportEmails.Count == 0)
            {
                return Results.NotFound("no customerWorker users found");
            }
            return Results.Ok(customerSupportEmails);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");

            return Results.Problem("internal error", statusCode: 500);
        }
    }
}