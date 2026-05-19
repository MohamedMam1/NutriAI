namespace NutriAI.Application.Common;

public static class AiMessages
{
    public const string ApiKeyNotConfigured =
        "OpenAI API key is not configured. Add OpenAI__ApiKey to your .env file and restart the application.";

    public const string AiUnavailableUseDatabase =
        "AI is not available right now. Showing saved database data instead.";

    public const string InformationUnavailable =
        "This information is not available right now.";

    public const string AccountBanned =
        "Your account has been banned. Please contact support if you believe this is a mistake.";
}
