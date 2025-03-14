namespace server.Classes;

public record EmailRequest(string To, string Subject, string Body);
