namespace server.Config;

public record EmailSettings(string SmtpServer, int SmtpPort, string FromEmail, string Password);
