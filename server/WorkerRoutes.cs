using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using server.Classes;
using server.Services;

namespace server;

public static class WorkerRoutes
{
    public record AdminRequest(string Email, int? CompanyId);

    public record GetCustomerSupportEmail(string Email);

    public record ChangePasswordRequest(string Password, string Token);

    public static bool IsValidEmail(string email)
    {
        var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$"; // En vanlig e-postformatregex
        return Regex.IsMatch(email, emailRegex);
    }

    public static async Task<IResult> CreateWorker(
        PasswordHasher<string> hasher,
        HttpContext context,
        NpgsqlDataSource db
    )
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

    public static async Task<IResult> GetActiveWorkers(HttpContext context, NpgsqlDataSource db)
    {
        try
        {
            List<GetCustomerSupportEmail> customerSupportEmails = new();
            await using var cmd = db.CreateCommand(
                "SELECT email FROM users WHERE role = 'support' AND active = true AND company_id = $1"
            );
            cmd.Parameters.AddWithValue(context.Session.GetInt32("company"));
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

    public static async Task<IResult> PostResetPasswordRequest(
        NpgsqlDataSource db,
        HttpContext context,
        IEmailService email,
        PasswordHasher<string> passwordHasher
    )
    {
        try
        {
            var requestBody = await context.Request.ReadFromJsonAsync<AdminRequest>();
            if (requestBody == null)
            {
                return Results.BadRequest("Invalid email");
            }
            Console.WriteLine($"received email: {requestBody.Email}");
            int companyId = requestBody.CompanyId ?? 1;
            string defaultPassWord = "hej123";
            string hashedPassword = passwordHasher.HashPassword("", defaultPassWord);

            bool success = await AddCustomerTask(requestBody.Email, companyId, hashedPassword, db);

            if (!success)
            {
                return Results.Problem("Failed to add worker");
            }

            string token = Guid.NewGuid().ToString();

            bool successToken = await StoreResetToken(requestBody.Email, token, db);

            if (!successToken)
            {
                return Results.Problem("Failed to store reset token");
            }

            string resetLink =  DotEnv.GetString("Localhost") + $"admin-panel/change-password/?token={token}";
            

            var emailRequest = new EmailRequest(
                requestBody.Email,
                "Change Password",
                $"Hello, {requestBody.Email}, \n\nYour account have registered. \nYour default password is {defaultPassWord}.\nPlease press the link to change password: \n<a href='{resetLink}'>Change Password</a>"
            );

            await email.SendEmailAsync(emailRequest.To, emailRequest.Subject, emailRequest.Body);

            return Results.Ok(new { message = "Valid mail" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");

            return Results.Problem("internal error", statusCode: 500);
        }
    }

    private static async Task<bool> AddCustomerTask(
        string email,
        int companyId,
        string defaultPassword,
        NpgsqlDataSource db
    )
    {
        try
        {
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email format.");
                return false; // Stop and return false if email is invalid
            }
            await using var cmd = db.CreateCommand(
                "INSERT INTO users (email, company_id, role, password) VALUES ($1, $2, $3::role, $4)"
            );
            cmd.Parameters.AddWithValue(email);
            cmd.Parameters.AddWithValue(companyId);
            cmd.Parameters.AddWithValue("support");
            cmd.Parameters.AddWithValue(defaultPassword);
            await cmd.ExecuteNonQueryAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("error adding customer support worker:" + ex);
            return false;
        }
    }

    private static async Task<bool> StoreResetToken(string email, string token, NpgsqlDataSource db)
    {
        try
        {
            await using var getUserIdCmd = db.CreateCommand(
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

            await using var cmd = db.CreateCommand(
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

    public static async Task<IResult> PutChangePassword(
        HttpContext context,
        NpgsqlDataSource db,
        PasswordHasher<string> passwordHasher
    )
    {
        var requestBody = await context.Request.ReadFromJsonAsync<ChangePasswordRequest>();
        string hashedPassword = passwordHasher.HashPassword("", requestBody.Password);
        Console.WriteLine(hashedPassword);
        Console.WriteLine(requestBody.Token);

        bool sucess = await UpdatePasswordTask(hashedPassword, requestBody.Token, db);

        if (sucess)
        {
            return Results.Ok(new { message = "Password updated succesfully" });
        }
        else
        {
            return Results.BadRequest(new { error = "Invalid token or user not found" });
        }
    }

    private static async Task<bool> UpdatePasswordTask(
        string password,
        string token,
        NpgsqlDataSource db
    )
    {
        await using var cmd = db.CreateCommand("SELECT user_id FROM reset_tokens WHERE token = $1");
        cmd.Parameters.AddWithValue(token); //1

        int userId = -1;

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            if (await reader.ReadAsync())
            {
                userId = reader.GetInt32(0);
            }
        }

        if (userId == -1)
        {
            return false;
        }

        await using var cmd2 = db.CreateCommand("UPDATE users SET password = $1 WHERE id = $2");
        cmd2.Parameters.AddWithValue(password); //1
        cmd2.Parameters.AddWithValue(userId); //2

        int rowsAffected = await cmd2.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }
}
