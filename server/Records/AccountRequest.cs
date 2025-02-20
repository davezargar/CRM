namespace server.Records;

public record AccountRequest(string email, string password);

public record UserRequest(string email, string password);
