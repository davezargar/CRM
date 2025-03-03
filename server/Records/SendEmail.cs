namespace server.Records;

public record SendEmail(string Title, string Description, int User_fk, int Ticket_id_fk);

public record NewTicketStatus(int Ticket_id, bool Resolved);