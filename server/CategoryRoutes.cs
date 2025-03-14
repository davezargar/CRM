using Npgsql;
namespace server;

public static class CategoryRoutes
{
    public record CategoryPairs(string MainCategory, string Subcategory);
    public record CategoryRecord(string MainCategory, List<string> Subcategories);
    
    public record TicketCategoryRecord(int Id, string Name, int CompanyId);
    public record TicketCategoryRequest(string Name, int CompanyId);
    public static async Task<IResult> GetFormCategories(HttpContext context, int companyId, NpgsqlDataSource db)
    {
        List<CategoryPairs> categoryPairs = new List<CategoryPairs>();
        List<CategoryRecord> categories = new List<CategoryRecord>();
        
        //todo refactor entire logic >:| ૮(•͈⌔•͈)ა 
        
        await using var cmd = db.CreateCommand(
            "SELECT categories.name, subcategories.name FROM categories "
            + "INNER JOIN subcategories ON categories.id = subcategories.main_category_id" +
            " WHERE categories.company_id = $1"
        );
        cmd.Parameters.AddWithValue(companyId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categoryPairs.Add(new CategoryPairs(reader.GetString(0), reader.GetString(1)));
        }

        List<string> buffer = new List<string>();
        string categoryPrevious = categoryPairs.Count > 0 ? categoryPairs[0].MainCategory : "";
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

        if (buffer.Count > 0)
        {
            categories.Add(new CategoryRecord(categoryPrevious, buffer));
        }
        
        return Results.Ok(categories);
    }
    
    public static async Task<IResult> GetCategories(HttpContext context, NpgsqlDataSource db)
    {
        string? userEmail = context.Session.GetString("Email");
        
        if (string.IsNullOrEmpty(userEmail))
        {
            return Results.Unauthorized();
        }
        
        int? companyId = await GetCompanyIdByEmail(db, userEmail);
        if (companyId == null)
        {
            return Results.BadRequest("Could not find company");
        }
        
        List<TicketCategoryRecord> ticketCategories = new();
        await using var cmd = db.CreateCommand("SELECT id, name, company_id FROM categories WHERE company_id = $1");
        cmd.Parameters.AddWithValue(companyId);
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
        return Results.Ok(ticketCategories);
    }

    
    
    public static async Task<IResult> CreateCategory(HttpContext context, NpgsqlDataSource db)
    {
        try
        {
            var requestBody = await context.Request.ReadFromJsonAsync<TicketCategoryRequest>();

            if (requestBody == null || string.IsNullOrWhiteSpace(requestBody.Name))
            {
                return Results.BadRequest("Category name cannot be empty.");
            }
            
            string? userEmail = context.Session.GetString("Email");
            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Results.BadRequest("Could not determine company.");
            }
            

            Console.WriteLine(
                $"Received category '{requestBody.Name}' for company ID {requestBody.CompanyId}"
            ); 

            try
            {
                await using var cmd = db.CreateCommand(
                    "INSERT INTO categories (name, company_id) VALUES ($1, $2)"
                );
                cmd.Parameters.AddWithValue(requestBody.Name);
                cmd.Parameters.AddWithValue(requestBody.CompanyId);
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine("Category successfully added!");
                return Results.Ok(new { message = "Category added!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating category: " + ex);
                Console.WriteLine("Failed to insert category into DB.");
                return Results.Problem("Failed to add category.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in /api/categories: {ex.Message}");
            return Results.Problem("Internal server error.");
        }
    }

    public static async Task<IResult> GetAssignCategories(HttpContext context, NpgsqlDataSource db)
    {
        string? userEmail = context.Session.GetString("Email");
        
        if (string.IsNullOrWhiteSpace(userEmail))
        {
            return Results.Unauthorized();
        }
        
        int? companyId = await GetCompanyIdByEmail(db, userEmail);
        
        if (companyId == null)
        {
            return Results.BadRequest("Could not determine company for this user.");
        }
        
        Dictionary<string, List<int>> assignments = new();
        
        await using var cmd = db.CreateCommand(
            "SELECT email, category_id FROM tickets_view WHERE company_id = $1"
        );
        cmd.Parameters.AddWithValue(companyId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string email = reader.GetString(0);
            int categoryId = reader.GetInt32(1);

            if (!assignments.ContainsKey(email))
                assignments.Add(email, new List<int>());

            assignments[email].Add(categoryId);
        }
        return Results.Ok(assignments);
    }
    
    private static async Task<int?> GetCompanyIdByEmail(NpgsqlDataSource db, string email)
    {
        await using var cmd = db.CreateCommand("SELECT company_id FROM users WHERE email = $1");
        cmd.Parameters.AddWithValue(email);
        var result = await cmd.ExecuteScalarAsync();
        return result as int?;
    }

    public static async Task<IResult> AssignCategories(HttpContext context, NpgsqlDataSource db)
    {
        var assignments = await context.Request.ReadFromJsonAsync<Dictionary<string, List<int>>>();

        if (assignments == null || assignments.Count == 0)
        {
            return Results.BadRequest("The request body is empty or invalid.");
        }

        try
        {
            await using var connection = await db.OpenConnectionAsync();
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
                        "INSERT INTO assigned_categories (user_id, category_id) VALUES ($1, $2)" +
                        "ON CONFLICT (user_id, category_id) DO NOTHING";
                    insertCmd.Parameters.AddWithValue(userId);
                    insertCmd.Parameters.AddWithValue(categoryId);
                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            return Results.Ok(new { message = "Assignments saved!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error assigning tickets: " + ex);
            return Results.Problem("Failed to assign tickets.");
        }
    }
}