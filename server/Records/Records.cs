namespace server.Records;

public record LoginRecord(string Email, string Password);

public record TicketRecord(int TicketId, string Category, string Subcategory, string Title, DateTime TimePosted, DateTime? TimeClosed, string UserFk, string ResponseEmail ,int CompanyFk );