using System.Text;
using Npgsql;
namespace server.Queries;

public class DatabaseConnection
{
    private NpgsqlDataSource _connection;

    public NpgsqlDataSource Connection()
    {
        return _connection;
    }

    public DatabaseConnection()
    {
        if(String.IsNullOrEmpty(Environment.GetEnvironmentVariable("DatabaseConnectString")))
        {
            throw new Exception("no connectstring in .env");
        }
        
        Console.WriteLine(Environment.GetEnvironmentVariable("DatabaseConnectString"));
        _connection = NpgsqlDataSource.Create(Environment.GetEnvironmentVariable("DatabaseConnectString"));
        
        Console.WriteLine(Environment.GetEnvironmentVariable("DatabaseConnectString"));
        using var conn = _connection.OpenConnection();
    }
}