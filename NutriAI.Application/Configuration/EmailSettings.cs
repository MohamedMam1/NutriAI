namespace NutriAI.Application.Configuration;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string SenderPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
