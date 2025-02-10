namespace server;

public static class DotEnv
{
    public static void Load(string filePath)
    {
        Console.WriteLine(filePath);
        if (!File.Exists(filePath))
        {
            Console.WriteLine("file not found");
            return;
        }
            

        foreach (var line in File.ReadAllLines(filePath))
        {
            var parts = line.Split('=', StringSplitOptions.RemoveEmptyEntries);
            Console.WriteLine(parts[0] + parts[1]);
            if (parts.Length == 2)
            {
                Console.WriteLine(parts[0] + parts[1]);
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}