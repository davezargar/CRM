using Npgsql;
using server.Records;

namespace server;

public static class WorkerRoutes
{

    public static async Task<IResult>GetCustomerSupportWorkers(HttpContext context, NpgsqlDataSource db)
    {
        Console.WriteLine(context.Session.GetInt32("company"));
        try
        {
        List<GetCustomerSupportEmail> customerSupportEmails = new();
        await using var cmd = db.CreateCommand("SELECT email FROM users WHERE role = 'support' AND company_id = $1");
        cmd.Parameters.AddWithValue(context.Session.GetInt32("company"));
        using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            customerSupportEmails.Add(new GetCustomerSupportEmail(reader.GetString(0)));
        }

            if (customerSupportEmails == null)
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
};