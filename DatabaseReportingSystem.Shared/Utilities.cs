using OpenAI.Chat;

namespace DatabaseReportingSystem.Shared;

public static class Utilities
{
    public static string TrimSqlString(string query)
    {
        string[] parts = query.Split(["```sql", "```"], StringSplitOptions.None);

        if (parts.Length > 1)
        {
            query = parts[1];
        }

        query = query.Replace("\n", " ").Replace("\t", "");

        query = string.Join(" ", query.Split([' '], StringSplitOptions.RemoveEmptyEntries));

        return query.Trim();
    }
    
    public static UserChatMessage CreateUserChatMessage(string question, string schema, string dbms = "sqlite")
    {
        string message = string.Format(Constants.Strategy.UserPromptFormat,
            question,
            dbms,
            schema);
        
        return new UserChatMessage(message);
    }
}
