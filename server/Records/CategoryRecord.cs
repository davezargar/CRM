namespace server.Records;

public record CategoryRecord(int Id, string Name);

public record CategoryRequest(string Name, int CompanyId);