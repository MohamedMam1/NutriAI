namespace NutriAI.Application.Configuration;

public class AdminSeedSettings
{
    public const string SectionName = "AdminSeed";
    public string Email { get; set; } = "admin@nutriai.com";
    public string Password { get; set; } = "Admin@12345";
    public string FullName { get; set; } = "System Admin";
}
