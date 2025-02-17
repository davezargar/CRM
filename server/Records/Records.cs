namespace server.Records;

public record TicketRecord(int TicketId, string Category, string Subcategory, string Title, DateTime TimePosted, DateTime? TimeClosed, string UserFk,int CompanyFk );