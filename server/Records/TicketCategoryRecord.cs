namespace server.Records;

public record TicketCategoryRecord(int Id, string Name, int CompanyId);

public record TicketCategoryRequest(string Name, int CompanyId);