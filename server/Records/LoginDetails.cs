using System.ComponentModel.DataAnnotations;

namespace server.Records;

public record LoginDetails(string Email, string Password);

public record TicketRequest(string Category, string Subcategory, string Title, string User_fk, string Response_email, int Company_fk);