using Microsoft.AspNetCore.Components.Web;

namespace server.Records;

public record LoginRecord(string Email, string Password);

public record TicketRecord(int TicketId, string Category, string Subcategory, string Title, DateTime TimePosted, DateTime? TimeClosed, string UserFk,int CompanyFk );

public record MessagesRecord(int MessageId, string Message, int TicketId, string Title, string UserId, DateTime TimePosted);

public record TicketMessagesRecord(TicketRecord TicketRecord, List<MessagesRecord> Messages);

public record NewTicketRecord(int TicketId, string Category, string Subcategory, string Title, string UserFk, int CompanyFk, int MessageId, string Message);

public record GetCustomerSupportEmail(string Email);