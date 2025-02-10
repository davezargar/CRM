namespace server;

public static class DotEnv
{
    public static void Load(string filePath)
    {
        Console.WriteLine("searching for .env at " + filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("file not found at " + filePath);
            return;
        }
        
        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }

    public static string GetString(string key)
    {
        string value = Environment.GetEnvironmentVariable(key);
        char startChar = value[0];

        if (new char[] { '"', '\'', '`' }.Contains(startChar))
            return value.Trim(startChar); //super sketchy quick fix, does not respect escape character
        return value;
    }
}