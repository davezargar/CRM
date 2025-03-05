using Microsoft.AspNetCore.Components.Web;

namespace server.Records;

public record LoginRecord(string Email, string Password);

public record TicketRecord(int TicketId, string Title, string Status, string Category, string Subcategory,  DateTime TimePosted, DateTime? TimeClosed, string UserEmail,int CompanyFk, bool Elevated);

public record MessagesRecord(int MessageId, string Message, int TicketId, string Title, string UserId, DateTime TimePosted);

public record TicketMessagesRecord(TicketRecord TicketRecord, List<MessagesRecord> Messages);

public record NewTicketRecord(int TicketId, string CategoryName, string SubcategoryName, string Title, string UserEmail, int CompanyFk, int MessageId, string Message);

public record GetCustomerSupportEmail(string Email);

public record CategoryPairs(string MainCategory, string Subcategory);
public record CategoryRecord(string MainCategory, List<string> Subcategories);

